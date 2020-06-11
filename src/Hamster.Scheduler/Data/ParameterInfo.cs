using System;
using System.Runtime.Serialization;

namespace Hamster.Scheduler.Data
{
  [Serializable()]
  [DataContract()]
  public class ParameterInfo
  {
    public int ParameterId { get; set; }
    public CommandInfo Command { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }
  }
}
