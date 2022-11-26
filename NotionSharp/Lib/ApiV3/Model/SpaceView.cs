using NotionSharp.Lib.ApiV3.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class SpaceView : BaseModel
    {
        public Guid SpaceId => TheValue.GetProperty("space_id").GetGuid();
        public Guid ParentId => TheValue.GetProperty("parent_id").GetGuid();
        public List<Guid> BookmarkedPages => TheValue.GetProperty("bookmarked_pages").Deserialize<List<Guid>>();
        public ParentTable ParentTable => TheValue.GetProperty("parent_table").Deserialize<ParentTable>();
        public bool Alive => TheValue.GetProperty("alive").GetBoolean();
        public List<Guid> VisitedTemplates => TheValue.GetProperty("visited_templates").Deserialize<List<Guid>>();
        public List<Guid> SidebarHiddenTemplates => TheValue.GetProperty("sidebar_hidden_templates").Deserialize<List<Guid>>();
        // "notify_mobile": true,
        // "notify_desktop": true,
        // "notify_email": true,
        // "created_getting_started": true
    }
}
