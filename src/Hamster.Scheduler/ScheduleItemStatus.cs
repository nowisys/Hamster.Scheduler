using System;
using System.Runtime.Serialization;

namespace Hamster.Scheduler
{
  [Serializable()]
  [DataContract()]
  public class ScheduleItemStatus
  {
    private DateTime? nextStart;
    private DateTime? lastStart;

    public ScheduleItemStatus()
    {

    }

    public ScheduleItemStatus(ISchedulerItem item)
    {
      Name = item.Name;
      NextStart = item.NextStart;
      LastStart = item.LastStart;
    }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "next")]
    public DateTime? NextStart
    {
      get { return nextStart; }
      set { nextStart = Validate(value); }
    }

    [DataMember(Name = "last")]
    public DateTime? LastStart
    {
      get { return lastStart; }
      set { lastStart = Validate(value); }
    }

    private DateTime? Validate(DateTime? value)
    {
      if (value.HasValue && value < DateTime.MaxValue && value > DateTime.MinValue)
      {
        return value;
      }
      else
      {
        return null;
      }
    }
  }
}
