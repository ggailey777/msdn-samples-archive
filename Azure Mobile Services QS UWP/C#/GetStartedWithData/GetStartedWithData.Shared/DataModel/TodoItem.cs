using System;
using System.Collections.Generic;
using System.Text;

//// TODO: Add the following using statement.  
//using Newtonsoft.Json;  

namespace GetStartedWithData.DataModel
{
    public class TodoItem
    {
        public string Id { get; set; }

        //// TODO: Add the following serialization attribute.  
        //[JsonProperty(PropertyName = "text")]  
        public string Text { get; set; }

        //// TODO: Add the following serialization attribute.  
        //[JsonProperty(PropertyName = "complete")]  
        public bool Complete { get; set; }
    }
} 
