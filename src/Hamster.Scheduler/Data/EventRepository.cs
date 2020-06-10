using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Hamster.Scheduler.Repository;
using NCM.Service.Scheduler.Repository;

namespace Hamster.Scheduler.Data
{
  public class EventRepository : IRepository<string, EventInfo>
  {
    private string path;
    private XmlWriterSettings settings;
    private XmlSerializer serializer = new XmlSerializer(typeof(List<EventInfo>));

    public EventRepository(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException("The 'path' parameter must not be empty");

      FileInfo file = new FileInfo(path);
      if (!file.Directory.Exists)
      {
        file.Directory.Create();
      }

      if (!file.Exists)
      {
        using (var writer = file.CreateText())
        {
          serializer.Serialize(writer, new List<EventInfo>());
        }
      }

      this.path = file.FullName;

      settings = new XmlWriterSettings()
      {
        Indent = true
      };
    }

    public EventInfo Get(string key)
    {
      return (from e in GetItems()
              where string.Compare(e.Name, key, true) == 0
              select e).FirstOrDefault();
    }

    public IList<EventInfo> GetItems()
    {
      return LoadEvents();
    }

    public IList<string> GetKeys()
    {
      return (from e in GetItems()
              select e.Name).ToArray<string>();
    }

    public IList<EventInfo> GetItems(int offset, int count)
    {
      var result = GetItems().Skip(offset * count);
      if (count > 0)
        result = result.Take(count);
      return result.ToArray();
    }

    public void Remove(string key)
    {
      lock (serializer)
      {
        List<EventInfo> events = LoadEvents();
        events.RemoveAll(x => x.Name == key);
        SaveEvents(events);
      }
    }

    public void Add(EventInfo item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("The 'Name' property of the item must be set.");

      lock (serializer)
      {
        List<EventInfo> events = LoadEvents();
        if (events.Exists(x => x.Name == item.Name))
          throw new ArgumentException(string.Format((string) "There is already an event with the name '{0}'.", (object) item.Name));
        events.Add(item);
        SaveEvents(events);
      }
    }

    public void Update(string key, EventInfo item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("The 'Name' property of the item must be set.");

      lock (serializer)
      {
        List<EventInfo> events = LoadEvents();
        if (key != item.Name && events.Exists(x => x.Name == item.Name))
          throw new ArgumentException(string.Format((string) "There is already an event with the name '{0}'.", (object) item.Name));
        events.RemoveAll(x => x.Name == key);
        events.Add(item);
        SaveEvents(events);
      }
    }

    public IRepoService<EventInfo> GetService()
    {
      return new RepoService<string, EventInfo>(this);
    }

    private List<EventInfo> LoadEvents()
    {
      lock (serializer)
      {
        using (TextReader reader = File.OpenText(path))
        {
          return (List<EventInfo>)serializer.Deserialize(reader);
        }
      }
    }

    private void SaveEvents(List<EventInfo> data)
    {
      lock (serializer)
      {
        string tmpfile = path + ".new";
        using (TextWriter writer = File.CreateText(tmpfile))
        {
          serializer.Serialize(writer, data);
        }

        File.Replace(tmpfile, path, path + ".old");
      }
    }
  }
}
