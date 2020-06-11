using System.Xml.Serialization;

namespace Hamster.Scheduler
{
  [XmlRoot("settings", Namespace = "http://www.nowisys.de/hamster/plugins/services/scheduler.xsd")]
  public class SchedulerPluginSettings
  {
    public SchedulerPluginSettings()
    {
      CrontabPath = "scheduler.crontab";
      EventsPath = "events.xml";
      CommandsDirectory = "commands";
    }

    [XmlElement("crontab")]
    public string CrontabPath { get; set; }

    [XmlElement("events")]
    public string EventsPath { get; set; }

    [XmlElement("commands")]
    public string CommandsDirectory { get; set; }
  }
}
