using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NotionSharp.Lib.ApiV3.Results
{
    public class GetUserTasksResult
    {
        public List<JObject> TaskIds { get; set; }
    }
}