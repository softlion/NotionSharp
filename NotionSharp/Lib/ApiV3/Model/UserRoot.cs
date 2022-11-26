using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class UserRoot : BaseModel
    {
        public List<Guid> SpaceViews => Value.GetProperty("space_views").Deserialize<List<Guid>>();
    }
}
