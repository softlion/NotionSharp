using System;
using System.Collections.Generic;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Space : BaseModel
    {
        public string Name => (string)Value["name"];
        public string Domain => (string)Value["domain"];
        public List<Guid> Pages => Value["pages"].ToObject<List<Guid>>();
        public List<string> EmailDomains => Value["email_domains"].ToObject<List<string>>();
        public Guid CreatedBy => (Guid)Value["created_by"];
        public Guid CreatedById => (Guid)Value["created_by_id"];
        public Guid LastEditedBy => (Guid)Value["last_edited_by"];
        public Guid LastEditedById => (Guid)Value["last_edited_by_id"];

        public long ShardId => (long)Value["shard_id"];
        public string InviteLinkCode => (string)Value["invite_link_code"];
        public long CreatedTime => (long)Value["created_time"];
        public long LastEditedTime => (long)Value["last_edited_time"];
        public List<Permission> Permissions => Value["email_domains"].ToObject<List<Permission>>();

        // "beta_enabled": false,
        // "created_by_table": "notion_user",
        // "last_edited_by_table": "notion_user",
        // "invite_link_enabled": false
    }
}
