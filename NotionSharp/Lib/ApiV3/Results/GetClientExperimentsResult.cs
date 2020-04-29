using NotionSharp.Lib.ApiV3.Model;
using System;
using System.Collections.Generic;

namespace NotionSharp.Lib.ApiV3.Results
{
    public class GetClientExperimentsResult
    {
        public Guid DeviceId { get; set; }
        public Guid UserId { get; set; }
        public bool IsLoaded { get; set; }
        public bool Test { get; set; }
        public List<NotionExperiment> Experiments { get; set; }
    }
}
