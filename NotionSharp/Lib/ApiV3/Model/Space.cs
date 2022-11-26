using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Space : BaseModel
    {
        public string Name => TheValue.GetProperty("name").GetString();
        public string Domain => TheValue.GetProperty("domain").GetString();
        public List<Guid> Pages => TheValue.GetProperty("pages").Deserialize<List<Guid>>();
        public List<string> EmailDomains => TheValue.GetProperty("email_domains").Deserialize<List<string>>();
        public Guid CreatedBy => TheValue.GetProperty("created_by").GetGuid();
        public Guid CreatedById => TheValue.GetProperty("created_by_id").GetGuid();
        public Guid LastEditedBy => TheValue.GetProperty("last_edited_by").GetGuid();
        public Guid LastEditedById => TheValue.GetProperty("last_edited_by_id").GetGuid();

        public long ShardId => TheValue.GetProperty("shard_id").GetInt64();
        public string InviteLinkCode => TheValue.GetProperty("invite_link_code").GetString();
        public long CreatedTime => TheValue.GetProperty("created_time").GetInt64();
        public long LastEditedTime => TheValue.GetProperty("last_edited_time").GetInt64();
        public List<Permission> Permissions => TheValue.GetProperty("email_domains").Deserialize<List<Permission>>();

        // "beta_enabled": false,
        // "created_by_table": "notion_user",
        // "last_edited_by_table": "notion_user",
        // "invite_link_enabled": false
    }
}
