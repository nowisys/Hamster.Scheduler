using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hamster.Scheduler.Data
{
  public class CronParser
  {
    public void AddSchedule(CronScheduleInfo schedule, TextReader reader, TextWriter writer)
    {
      List<string> comments = new List<string>();
      int lineno = 0;
      foreach (string line in GetLines(reader))
      {
        lineno += 1;
        if (IsComment(line))
        {
          comments.Add(line);
        }
        else
        {
          var cur = ParseSchedule(line);
          if (cur == null || string.Compare(cur.Name, schedule.Name, true) != 0)
          {
            foreach (string c in comments)
              writer.WriteLine(c);
            writer.WriteLine(line);
          }
          else
          {
            throw new ArgumentException(string.Format("There is already a schedule with the name '{0}'.", schedule.Name));
          }
          comments.Clear();
        }
      }

      WriteSchedule(writer, schedule);
    }

    public void UpdateSchedule(string name, CronScheduleInfo schedule, TextReader reader, TextWriter writer)
    {
      RemoveSchedule(name, reader, writer);
      WriteSchedule(writer, schedule);
    }

    public IEnumerable<CronScheduleInfo> ReadSchedules(TextReader reader)
    {
      List<string> comments = new List<string>();
      int lineno = 0;
      foreach (string line in GetLines(reader))
      {
        lineno += 1;
        if (IsComment(line))
        {
          comments.Add(line);
        }
        else
        {
          var sched = ParseSchedule(line);
          if (sched != null)
          {
            sched.Description = ParseDescription(comments);
            yield return sched;
          }
          comments.Clear();
        }
      }
    }

    public void WriteSchedule(TextWriter writer, CronScheduleInfo schedule)
    {
      writer.WriteLine();
      WriteDescription(writer, schedule.Description);
      writer.Write(schedule.Minutes);
      writer.Write(" ");
      writer.Write(schedule.Hours);
      writer.Write(" ");
      writer.Write(schedule.DaysOfMonth);
      writer.Write(" ");
      writer.Write(schedule.Months);
      writer.Write(" ");
      writer.Write(schedule.DaysOfWeek);
      writer.Write(" ");
      writer.Write(schedule.Name);
      writer.WriteLine();
    }

    public void RemoveSchedule(string name, TextReader reader, TextWriter writer)
    {
      List<string> comments = new List<string>();
      int lineno = 0;
      foreach (string line in GetLines(reader))
      {
        lineno += 1;
        if (IsComment(line))
        {
          comments.Add(line);
        }
        else
        {
          var sched = ParseSchedule(line);
          if (sched == null || string.Compare(sched.Name, name, true) != 0)
          {
            foreach (string c in comments)
              writer.WriteLine(c);
            writer.WriteLine(line);
          }
          comments.Clear();
        }
      }
    }

    protected IEnumerable<string> GetLines(TextReader reader)
    {
      string line = reader.ReadLine();
      while (line != null)
      {
        yield return line;
        line = reader.ReadLine();
      }
    }

    protected bool IsComment(string line)
    {
      return (from c in line
              where !char.IsWhiteSpace(c)
              select c == '#').FirstOrDefault();
    }

    protected string ParseDescription(IEnumerable<string> comments)
    {
      StringBuilder result = new StringBuilder();
      foreach (string line in comments)
      {
        int start = line.IndexOf('#');
        result.AppendLine(line.Substring(start + 1).Trim());
      }
      return result.ToString().Trim();
    }

    protected CronScheduleInfo ParseSchedule(string line)
    {
      line = line.Trim();
      if (line.Length == 0)
        return null;

      string[] parts = line.Split(new char[] { ' ', '\t' }, 6);
      if (parts.Length < 6)
      {
        throw new FormatException();
      }

      CronScheduleInfo result = new CronScheduleInfo();
      result.Minutes = parts[0];
      result.Hours = parts[1];
      result.DaysOfMonth = parts[2];
      result.Months = parts[3];
      result.DaysOfWeek = parts[4];
      result.Name = parts[5];
      return result;
    }

    protected void WriteDescription(TextWriter writer, string description)
    {
      string[] breaks = { "\r\n", "\r", "\n" };
      foreach (string line in description.Split(breaks, StringSplitOptions.None))
      {
        writer.Write("# ");
        writer.WriteLine(line);
      }
    }
  }
}
