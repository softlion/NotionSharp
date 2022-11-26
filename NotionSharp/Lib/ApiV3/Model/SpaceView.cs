using NotionSharp.Lib.ApiV3.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class SpaceView : BaseModel
    {
        public Guid SpaceId => Value.GetProperty("space_id").GetGuid();
        public Guid ParentId => Value.GetProperty("parent_id").GetGuid();
        public List<Guid> BookmarkedPages => Value.GetProperty("bookmarked_pages").Deserialize<List<Guid>>();
        public ParentTable ParentTable => Value.GetProperty("parent_table").Deserialize<ParentTable>();
        public bool Alive => Value.GetProperty("alive").GetBoolean();
        public List<Guid> VisitedTemplates => Value.GetProperty("visited_templates").Deserialize<List<Guid>>();
        public List<Guid> SidebarHiddenTemplates => Value.GetProperty("sidebar_hidden_templates").Deserialize<List<Guid>>();
        // "notify_mobile": true,
        // "notify_desktop": true,
        // "notify_email": true,
        // "created_getting_started": true
    }
}
