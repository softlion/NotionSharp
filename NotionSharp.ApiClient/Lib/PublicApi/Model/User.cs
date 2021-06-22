using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public static class UserTypeConst
    {
        public const string Person = "person";
        public const string Bot = "bot";
    }
    
    public class User : BaseObject
    {
        //Object = "user"
        public string Id { get; set; } //GUID lowercase no {}
        
        [JsonPropertyName("type")]
        public string UserType { get; set; } //"person", "bot"
        
        public string Name { get; set; }
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; } //URI
            
        //type=person
        public Person Person { get; set; }
        //type=bot
        public Bot Bot { get; set; }
    }
}