using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hamster.Scheduler.Repository;

namespace Hamster.Scheduler.Data
{
  public class ScheduleRepository : IRepository<string, CronScheduleInfo>
  {
    private string path;
    private CronParser parser = new CronParser();

    public ScheduleRepository(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException("The 'path' parameter must not be empty");

      FileInfo file = new FileInfo(path);
      if (file.Directory != null && !file.Directory.Exists)
      {
        file.Directory.Create();
      }

      if (!file.Exists)
      {
        using (var writer = file.CreateText())
        {
          writer.Write("");
          writer.Flush();
        }
      }

      this.path = file.FullName;
    }

    public CronScheduleInfo Get(string key)
    {
      lock (parser)
      {
        CronScheduleInfo result;
        using (var reader = new StreamReader(path))
        {
          result = (from sched in parser.ReadSchedules(reader)
                    where sched.Name == key
                    select sched).FirstOrDefault();
        }
        return result;
      }
    }

    public IList<CronScheduleInfo> GetItems()
    {
      lock (parser)
      {
        using (var reader = new StreamReader(path))
        {
          var result = parser.ReadSchedules(reader).ToArray();
          return result;
        }
      }
    }

    public IList<string> GetKeys()
    {
      lock (parser)
      {
        string[] result;
        using (var reader = new StreamReader(path))
        {
          result = (from sched in parser.ReadSchedules(reader)
                    select sched.Name).ToArray();
        }
        return result;
      }
    }

    public IList<CronScheduleInfo> GetItems(int offset, int count)
    {
      lock (parser)
      {
        CronScheduleInfo[] result;
        using (var reader = new StreamReader(path))
        {
          var tmp = parser.ReadSchedules(reader)
              .Skip(count * offset);
          if (count > 0)
            tmp = tmp.Take(count);
          result = tmp.ToArray();
        }
        return result;
      }
    }

    public void Remove(string key)
    {
      lock (parser)
      {
        string temp = path + ".new";
        using (var reader = new StreamReader(path))
        using (var writer = new StreamWriter(temp))
        {
          parser.RemoveSchedule(key, reader, writer);
        }

        File.Replace(temp, path, path + ".old");
      }
    }

    public void Update(string key, CronScheduleInfo item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("The 'Name' property of the item must be set.");

      lock (parser)
      {
        string temp = path + ".new";
        using (var reader = new StreamReader(path))
        using (var writer = new StreamWriter(temp))
        {
          parser.UpdateSchedule(key, item, reader, writer);
        }

        File.Replace(temp, path, path + ".old");
      }
    }

    public void Add(CronScheduleInfo item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("The 'Name' property of the item must be set.");

      lock (parser)
      {
        string temp = path + ".new";
        using (var reader = new StreamReader(path))
        using (var writer = new StreamWriter(temp))
        {
          parser.AddSchedule(item, reader, writer);
        }

        File.Replace(temp, path, path + ".old");
      }
    }

    public IRepoService<CronScheduleInfo> GetService()
    {
      return new RepoService<string, CronScheduleInfo>(this);
    }
  }
}
