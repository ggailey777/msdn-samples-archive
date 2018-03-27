using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NorthwindClient
{  
    public class Registration
    {
        public string ChannelUri { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
        public string RegistrationId { get; set; }
        public ISet<string> Tags { get; set; }
        public string Type { get; set; }
    }

    public class Notification
    {
        [JsonProperty(propertyName: "feed")]
        public string Feed { get; set; }
        [JsonProperty(propertyName: "operation")]
        public string Operation { get; set; }
        [JsonProperty(propertyName: "entity")]
        public string Entity { get; set; }
        [JsonProperty(propertyName: "message")]
        public string Message { get; set; }

    }
}
