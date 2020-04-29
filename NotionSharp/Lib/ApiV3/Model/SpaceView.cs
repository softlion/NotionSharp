using NotionSharp.Lib.ApiV3.Enums;
using System;
using System.Collections.Generic;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class SpaceView : BaseModel
    {
        public Guid SpaceId => (Guid)Value["space_id"];
        public Guid ParentId => (Guid)Value["parent_id"];
        public List<Guid> BookmarkedPages => Value["bookmarked_pages"].ToObject<List<Guid>>();
        public ParentTable ParentTable => Value["parent_table"].ToObject<ParentTable>();
        public bool Alive => (bool)Value["alive"];
        public List<Guid> VisitedTemplates => Value["visited_templates"].ToObject<List<Guid>>();
        public List<Guid> SidebarHiddenTemplates => Value["sidebar_hidden_templates"].ToObject<List<Guid>>();
        // "notify_mobile": true,
        // "notify_desktop": true,
        // "notify_email": true,
        // "created_getting_started": true
    }
}
