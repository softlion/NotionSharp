using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Collection : BaseModel
    {
        public string Name => (string)((JArray)((JArray)Value["name"])[0])[0];
        public JArray Description => (JArray)Value["description"];
        public string DescriptionText => (string)((JArray)Description[0])[0];
        public string Icon => (string)Value["icon"];

        //"schema": {
        //"format": {

        public Guid ParentId => (Guid)Value["parent_id"];
        public ParentTable ParentTable => Value["parent_table"].ToObject<ParentTable>();

        public bool Alive => (bool)Value["alive"];
        public Guid CopiedFrom => (Guid)Value["copied_from"];
        public List<Guid> TemplatePages => Value["template_pages"].ToObject<List<Guid>>();
    }
}
