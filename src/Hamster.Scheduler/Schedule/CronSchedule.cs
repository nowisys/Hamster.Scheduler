using System;
using System.Collections;
using Hamster.Scheduler.Data;

namespace Hamster.Scheduler.Schedule
{
  public class CronSchedule : ISchedule
  {
    private BitArray minutes;
    private BitArray hours;
    private BitArray months;
    private BitArray daysOfMonth;
    private BitArray daysOfWeek;

    private DateTime lastDate;
    private DateTime nextDate;

    public CronSchedule(CronScheduleInfo info)
    {
      this.minutes = CronHelper.Parse(info.Minutes, 0, 59);
      this.hours = CronHelper.Parse(info.Hours, 0, 23);

      if (info.Months != "*" || info.DaysOfMonth != "*")
      {
        this.months = CronHelper.Parse(info.Months, 1, 12);
        this.daysOfMonth = CronHelper.Parse(info.DaysOfMonth, 1, 31);
      }

      if (info.DaysOfWeek != "*")
      {
        this.daysOfWeek = CronHelper.Parse(info.DaysOfWeek, 0, 7);
        this.daysOfWeek[7] = this.daysOfWeek[0] = this.daysOfWeek[0] || this.daysOfWeek[7];
      }
    }

    public DateTime LastDate
    {
      get { return lastDate; }
    }

    public DateTime NextDate
    {
      get { return nextDate; }
    }

    public void Initialize(DateTime start)
    {
      nextDate = start.Date.Add(new TimeSpan(start.Hour, start.Minute, 0));

      Increase();
      lastDate = DateTime.MinValue;
    }

    public void Increase()
    {
      lastDate = nextDate;

      int minute = lastDate.Minute + 1;
      int hour = lastDate.Hour;

      while (!minutes[minute % 60])
      {
        minute += 1;
      }

      if (minute >= 60)
      {
        hour += 1;
        minute = minute - 60;
      }

      while (!hours[hour % 24])
      {
        hour += 1;
      }

      if (hour >= 24)
      {
        nextDate = nextDate.Date.Add(new TimeSpan(1, hour - 24, minute, 0));
      }
      else
      {
        nextDate = nextDate.Date.Add(new TimeSpan(hour, minute, 0));
      }

      Predicate<DateTime> pred;

      if (daysOfWeek != null && months != null)
      {
        pred = delegate(DateTime date) { return daysOfWeek[(int)date.DayOfWeek] || (daysOfMonth[date.Day - 1] && months[date.Month - 1]); };
      }
      else if (daysOfWeek != null)
      {
        pred = delegate(DateTime date) { return daysOfWeek[(int)date.DayOfWeek]; };
      }
      else if (months != null)
      {
        pred = delegate(DateTime date) { return daysOfMonth[date.Day - 1] && months[date.Month - 1]; };
      }
      else
      {
        pred = delegate(DateTime date) { return true; };
      }

      while (!pred(nextDate))
      {
        nextDate = nextDate.AddDays(1);
      }
    }
  }
}
