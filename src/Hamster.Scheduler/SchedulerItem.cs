using System;
using System.Collections.Generic;
using Hamster.Plugin.Events;
using Hamster.Scheduler.Commands;
using Hamster.Scheduler.Schedule;

namespace Hamster.Scheduler
{
  public class SchedulerItem : ISchedulerItem
  {
    private string name;
    private ICommand command;
    private IDictionary<string, object> parameters;
    private ISchedule schedule;

    private DateTime lastStart;

    public SchedulerItem(string name, ISchedule schedule, ICommand command, IDictionary<string, object> parameters)
    {
      if (string.IsNullOrEmpty(this.name = name))
        throw new ArgumentNullException("name");

      if ((this.command = command) == null)
        throw new ArgumentNullException("command");

      if ((this.parameters = parameters) == null)
        throw new ArgumentNullException("parameters");

      if ((this.schedule = schedule) == null)
        throw new ArgumentNullException("schedule");

      lastStart = DateTime.MinValue;

      this.schedule.Initialize(DateTime.Now);
    }

    public string Name
    {
      get { return name; }
    }

    public DateTime NextStart
    {
      get { return schedule.NextDate; }
    }

    public DateTime LastStart
    {
      get { return lastStart; }
    }

    public event EventHandler<ISchedulerItem, EventArgs> Invoked;

    public event EventHandler<ISchedulerItem, EventArgs> Increased;

    public void Invoke()
    {
      command.Invoke(parameters);
      lastStart = DateTime.Now;

      if (Invoked != null)
      {
        Invoked(this, EventArgs.Empty);
      }
    }

    public void Increase()
    {
      schedule.Increase();

      if (Increased != null)
      {
        Increased(this, EventArgs.Empty);
      }
    }
  }
}
