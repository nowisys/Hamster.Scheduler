using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hamster.Scheduler.Data
{
  [Serializable()]
  [DataContract()]
  public class CommandInfo
  {
    public CommandInfo()
    {
      Language = "Python";
      Parameters = new List<ParameterInfo>();
    }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "language")]
    public string Language { get; set; }

    [DataMember(Name = "scriptFile")]
    public string ScriptFile { get; set; }

    [DataMember(Name = "scriptCode")]
    public string ScriptCode { get; set; }

    [DataMember(Name = "parameters")]
    public IList<ParameterInfo> Parameters { get; set; }
  }
}
