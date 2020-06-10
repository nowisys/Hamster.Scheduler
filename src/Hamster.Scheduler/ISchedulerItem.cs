using System;
using Hamster.Plugin.Events;

namespace Hamster.Scheduler
{
  public interface ISchedulerItem
  {
    string Name { get; }

    DateTime NextStart { get; }
    DateTime LastStart { get; }

    void Invoke();
    void Increase();

    event EventHandler<ISchedulerItem, EventArgs> Invoked;
    event EventHandler<ISchedulerItem, EventArgs> Increased;
  }
}
