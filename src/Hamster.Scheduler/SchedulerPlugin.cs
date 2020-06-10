using System;
using System.Collections.Generic;
using System.Threading;
using Hamster.Plugin;
using Hamster.Scheduler.Commands;
using Hamster.Scheduler.Data;
using Hamster.Scheduler.Repository;
using Hamster.Scheduler.Schedule;
using Microsoft.Scripting.Hosting;

namespace Hamster.Scheduler
{
  public class SchedulerPlugin : AbstractPlugin<SchedulerPluginSettings>
  {


    private ScriptRuntime runtime;

    private List<SchedulerItem> schedulerItems = new List<SchedulerItem>();
    private Dictionary<string, ScriptCommand> commands = new Dictionary<string, ScriptCommand>(StringComparer.InvariantCultureIgnoreCase);

    private SchedulerQueue schedulerQueue;
    private AutoResetEvent executeEvent;
    private RegisteredWaitHandle executeHandle;
    
    public  IPluginDirectory Plugins { get; private set; }
    public IRepository<string, EventInfo> EventManager { get; private set; }
    public IRepository<string, CronScheduleInfo> ScheduleManager { get; private set; }
    public IRepository<string, CommandInfo> CommandManager { get; private set; }
    


    public SchedulerPlugin()
      : this(IronPython.Hosting.Python.CreateRuntime())
    {
    }
    
    public SchedulerPlugin(ScriptRuntime runtime)
    {
      schedulerQueue = new SchedulerQueue();
      schedulerQueue.ItemsChanged += SchedulerQueueChanged;
      this.runtime = runtime;
      
      EventManager = new EventRepository(Settings.EventsPath);
      ScheduleManager = new ScheduleRepository(Settings.CrontabPath);
      CommandManager = new CommandRepository(Settings.CommandsDirectory, runtime);
    }
    
    public override void Init()
    {

      LoadCommands();
    }

    public override void Open()
    {
      LoadSchedulerItems();

      try
      {
        ICommand startup = GetCommand("@startup");
        if (startup != null)
        {
          startup.Invoke(null);
        }
      }
      catch (Exception x)
      {
        Logger.Error(x, "Error with startup command '@startup'.");
      }
    }

    public override void Close()
    {
      try
      {
        ICommand shutdown = GetCommand("@shutdown");
        if (shutdown != null)
        {
          shutdown.Invoke(null);
        }
      }
      catch (Exception x)
      {
        Logger.Error(x, "Error with shutdown command '@shutdown'.");
      }

      lock (schedulerQueue)
      {
        schedulerQueue.Clear();
      }
    }

    

    
    private void SchedulerQueueChanged(object sender, EventArgs e)
    {
      lock (schedulerQueue)
      {
        if (executeHandle == null)
        {
          executeEvent = new AutoResetEvent(false);
          executeHandle = ThreadPool.RegisterWaitForSingleObject(executeEvent, ExecuteItemCallback, null, 0, true);
        }
        executeEvent.Set();
      }
    }

    protected virtual void ExecuteItemCallback(object state, bool timedOut)
    {
      try
      {
        ISchedulerItem item;
        bool execute;
        lock (schedulerQueue)
        {
          executeHandle.Unregister(executeEvent);
          executeEvent.Close();
          executeHandle = null;

          item = schedulerQueue.Top();
          if (item == null)
          {
            return;
          }

          DateTime now = DateTime.UtcNow;
          execute = now >= item.NextStart.ToUniversalTime();
          if (execute)
          {
            item.Increase();
          }

          ISchedulerItem top = schedulerQueue.Top();
          if (top != null)
          {
            TimeSpan interval = new TimeSpan(0, 0, 1);
            DateTime next = top.NextStart.ToUniversalTime();
            if (next > now)
            {
              interval = next - now;
              Logger.Debug("Next event at {0:u}.", top.NextStart.ToLocalTime());
            }

            executeEvent = new AutoResetEvent(false);
            executeHandle = ThreadPool.RegisterWaitForSingleObject(executeEvent, ExecuteItemCallback, null, interval, true);
          }
        }

        if (execute)
        {
          Logger.Debug("Starting scheduled item '{0}'.", item.Name);
          ThreadPool.QueueUserWorkItem(InvokeCallback, item);
        }
      }
      catch (Exception x)
      {
        Logger.Error(x, "Error while processing schedule. Scheduler is halting.");
      }
    }

    protected virtual void InvokeCallback(object state)
    {
      try
      {
        ISchedulerItem item = (ISchedulerItem)state;
        item.Invoke();
      }
      catch (Exception x)
      {
        Logger.Error(x, "Error while executing schedule item.");
      }
    }

    public ISchedule GetSchedule(string name)
    {
      CronScheduleInfo info = ScheduleManager.Get(name);
      if (info == null)
      {
        return null;
      }

      return new CronSchedule(info);
    }
    
    public void LoadCommands()
    {
      Logger.Info("Loading commands.");

      Dictionary<string, ScriptCommand> newCommands = new Dictionary<string, ScriptCommand>(StringComparer.InvariantCultureIgnoreCase);
      foreach (CommandInfo info in CommandManager.GetItems())
      {
        if (newCommands.ContainsKey(info.Name))
        {
          Logger.Warn("Duplicated command name '{0}' detected. Only the first will be loaded.", info.Name);
          continue;
        }

        try
        {
          ScriptEngine engine;
          if (!runtime.TryGetEngineByFileExtension(info.Language, out engine))
            engine = runtime.GetEngine(info.Language);
          ScriptSource source = engine.CreateScriptSourceFromString(info.ScriptCode, Microsoft.Scripting.SourceCodeKind.Statements);
          CompiledCode code = source.Compile();
          ScriptScope scope = engine.CreateScope();
          scope.SetVariable("logger", Logger.CreateChildLogger(info.Name));
          scope.SetVariable("plugins", Plugins);

          foreach (var param in info.Parameters)
          {
            scope.SetVariable(param.Name, null);
          }

          if (!string.IsNullOrEmpty(info.ScriptFile))
          {
            engine.ExecuteFile(info.ScriptFile, scope);
          }

          newCommands.Add(info.Name, new ScriptCommand(code, scope));
          Logger.Debug("Added command '{0}'.", info.Name);
        }
        catch (Exception x)
        {
          Logger.Error(x, "Error loading command '{0}'.", info.Name);
        }
      }
      commands = newCommands;
    }

    public void LoadSchedulerItems()
    {
      Logger.Info("Loading events.");

      lock (schedulerItems)
      {
        lock (schedulerQueue)
        {
          foreach (SchedulerItem item in schedulerItems)
          {
            schedulerQueue.Remove(item);
          }
          schedulerItems.Clear();
        }

        foreach (EventInfo info in EventManager.GetItems())
        {
          ScriptCommand command;

          if (!commands.TryGetValue(info.CommandName, out command))
          {
            Logger.Error("Error while loading the scheduler item '{0}'. Could not find a command with the name '{1}'.", info.Name, info.CommandName);
            continue;
          }

          ISchedule schedule = GetSchedule(info.ScheduleName);
          if (schedule == null)
          {
            Logger.Error("Error while loading the scheduler item '{0}'. Could not find a schedule with the name '{1}'.", info.Name, info.ScheduleName);
            continue;
          }

          IDictionary<string, object> parameters = new Dictionary<string, object>();
          if (!string.IsNullOrEmpty(info.Parameters))
          {
            try
            {
              ScriptEngine engine = runtime.GetEngine("Python");
              parameters = engine.Execute<IDictionary<string, object>>(info.Parameters, engine.CreateScope());
            }
            catch (Exception x)
            {
              Logger.Error(x, "Error while loading the scheduler item '{0}'. Could not parse parameters '{1}'.", info.Name, info.Parameters);
              continue;
            }
          }

          SchedulerItem item = new SchedulerItem(info.Name, schedule, command, parameters);
          schedulerItems.Add(item);
        }

        lock (schedulerQueue)
        {
          foreach (SchedulerItem item in schedulerItems)
          {
            schedulerQueue.Add(item);
            Logger.Debug("Added event '{0}'.", item.Name);
          }
        }
      }
    }

    public ICommand GetCommand(string name)
    {
      ScriptCommand result;
      if (!commands.TryGetValue(name, out result))
      {
        return null;
      }

      return result;
    }
  }
}
