using System;
using System.Collections.Generic;
using Hamster.Plugin.Events;
using Hamster.Scheduler.Commands;
using Hamster.Scheduler.Schedule;

namespace Hamster.Scheduler
{
  public class SchedulerItem : ISchedulerItem
  {
    private readonly ICommand command;
    private readonly IDictionary<string, object> parameters;
    private readonly ISchedule schedule;

    private DateTime lastStart;

    public SchedulerItem(string name, ISchedule schedule, ICommand command, IDictionary<string, object> parameters)
    {
      if (string.IsNullOrEmpty(this.Name = name))
        throw new ArgumentNullException(nameof(name));

      if ((this.command = command) == null)
        throw new ArgumentNullException(nameof(command));

      if ((this.parameters = parameters) == null)
        throw new ArgumentNullException(nameof(parameters));

      if ((this.schedule = schedule) == null)
        throw new ArgumentNullException(nameof(schedule));

      lastStart = DateTime.MinValue;

      this.schedule.Initialize(DateTime.Now);
    }

    public string Name { get; }

    public DateTime NextStart => schedule.NextDate;

    public DateTime LastStart => lastStart;

    public event EventHandler<ISchedulerItem, EventArgs> Invoked;

    public event EventHandler<ISchedulerItem, EventArgs> Increased;

    public void Invoke()
    {
      command.Invoke(parameters);
      lastStart = DateTime.Now;

      Invoked?.Invoke(this, EventArgs.Empty);
    }

    public void Increase()
    {
      schedule.Increase();

      Increased?.Invoke(this, EventArgs.Empty);
    }
  }
}
