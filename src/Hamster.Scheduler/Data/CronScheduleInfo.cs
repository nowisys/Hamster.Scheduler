using System;
using System.Runtime.Serialization;
using Hamster.Scheduler.Schedule;

namespace Hamster.Scheduler.Data
{
  [Serializable()]
  [DataContract()]
  public class CronScheduleInfo
  {
    private string minutes = "*";
    private string hours = "*";
    private string months = "*";
    private string daysOfMonth = "*";
    private string daysOfWeek = "*";

    public CronScheduleInfo()
    {

    }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "minutes")]
    public string Minutes
    {
      get { return minutes; }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          minutes = "*";
        }
        else
        {
          CronHelper.Parse(value, 0, 59);
          minutes = value;
        }
      }
    }

    [DataMember(Name = "hours")]
    public string Hours
    {
      get { return hours; }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          hours = "*";
        }
        else
        {
          CronHelper.Parse(value, 0, 23);
          hours = value;
        }
      }
    }

    [DataMember(Name = "months")]
    public string Months
    {
      get { return months; }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          months = "*";
        }
        else
        {
          CronHelper.Parse(value, 1, 12);
          months = value;
        }
      }
    }

    [DataMember(Name = "daysOfMonth")]
    public string DaysOfMonth
    {
      get { return daysOfMonth; }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          daysOfMonth = "*";
        }
        else
        {
          CronHelper.Parse(value, 1, 31);
          daysOfMonth = value;
        }
      }
    }

    [DataMember(Name = "daysOfWeek")]
    public string DaysOfWeek
    {
      get { return daysOfWeek; }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          daysOfWeek = "*";
        }
        else
        {
          CronHelper.Parse(value, 0, 7);
          daysOfWeek = value;
        }
      }
    }
  }
}
