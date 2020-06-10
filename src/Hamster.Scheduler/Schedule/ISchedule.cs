using System;

namespace Hamster.Scheduler.Schedule
{
  public interface ISchedule
  {
    DateTime LastDate { get; }
    DateTime NextDate { get; }

    void Initialize(DateTime start);

    void Increase();
  }
}
