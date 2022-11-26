using System;
using System.Collections.Generic;
using System.Text.Json;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Collection : BaseModel
    {
        public string Name => TheValue.GetProperty("name")[0][0].GetString();
        /// <summary>
        /// JsonArray
        /// </summary>
        public JsonElement Description => TheValue.GetProperty("description");
        public string DescriptionText => Description[0][0].GetString();
        public string Icon => TheValue.GetProperty("icon").GetString();

        //"schema": {
        //"format": {

        public Guid ParentId => TheValue.GetProperty("parent_id").GetGuid();
        public ParentTable ParentTable => TheValue.GetProperty("parent_table").Deserialize<ParentTable>();

        public bool Alive => TheValue.GetProperty("alive").GetBoolean();
        public Guid CopiedFrom => TheValue.GetProperty("copied_from").GetGuid();
        public List<Guid> TemplatePages => TheValue.GetProperty("template_pages").Deserialize<List<Guid>>();
    }
}
