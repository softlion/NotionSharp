using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Space : BaseModel
    {
        public string Name => Value.GetProperty("name").GetString();
        public string Domain => Value.GetProperty("domain").GetString();
        public List<Guid> Pages => Value.GetProperty("pages").Deserialize<List<Guid>>();
        public List<string> EmailDomains => Value.GetProperty("email_domains").Deserialize<List<string>>();
        public Guid CreatedBy => Value.GetProperty("created_by").GetGuid();
        public Guid CreatedById => Value.GetProperty("created_by_id").GetGuid();
        public Guid LastEditedBy => Value.GetProperty("last_edited_by").GetGuid();
        public Guid LastEditedById => Value.GetProperty("last_edited_by_id").GetGuid();

        public long ShardId => Value.GetProperty("shard_id").GetInt64();
        public string InviteLinkCode => Value.GetProperty("invite_link_code").GetString();
        public long CreatedTime => Value.GetProperty("created_time").GetInt64();
        public long LastEditedTime => Value.GetProperty("last_edited_time").GetInt64();
        public List<Permission> Permissions => Value.GetProperty("email_domains").Deserialize<List<Permission>>();

        // "beta_enabled": false,
        // "created_by_table": "notion_user",
        // "last_edited_by_table": "notion_user",
        // "invite_link_enabled": false
    }
}
