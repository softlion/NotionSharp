using System;
using System.Collections.Generic;
using System.Text.Json;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Collection : BaseModel
    {
        public string Name => Value.GetProperty("name")[0][0].GetString();
        /// <summary>
        /// JsonArray
        /// </summary>
        public JsonElement Description => Value.GetProperty("description");
        public string DescriptionText => Description[0][0].GetString();
        public string Icon => Value.GetProperty("icon").GetString();

        //"schema": {
        //"format": {

        public Guid ParentId => Value.GetProperty("parent_id").GetGuid();
        public ParentTable ParentTable => Value.GetProperty("parent_table").Deserialize<ParentTable>();

        public bool Alive => Value.GetProperty("alive").GetBoolean();
        public Guid CopiedFrom => Value.GetProperty("copied_from").GetGuid();
        public List<Guid> TemplatePages => Value.GetProperty("template_pages").Deserialize<List<Guid>>();
    }
}
