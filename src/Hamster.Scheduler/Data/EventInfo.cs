using System;
using System.Runtime.Serialization;

namespace Hamster.Scheduler.Data
{
    [Serializable()]
    [DataContract()]
    public class EventInfo
    {
        [DataMember(Name = "name")] 
        public string Name { get; set; }

        [DataMember(Name = "description")] 
        public string Description { get; set; }

        [DataMember(Name = "command")] 
        public string CommandName { get; set; }

        [DataMember(Name = "schedule")] 
        public string ScheduleName { get; set; }

        [DataMember(Name = "parameters")] 
        public string Parameters { get; set; }
    }
}