using System;
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
        public string Id { get; set; } //User id (not a block id)
        
        [JsonPropertyName("type")]
        public string UserType { get; set; } //"person", "bot"
        
        public string? Name { get; set; }
        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; } //URI
            
        //type=person
        public Person? Person { get; set; }
        //type=bot
        public Bot? Bot { get; set; }

        protected bool Equals(User other)
            => Id == other.Id && UserType == other.UserType && Name == other.Name && AvatarUrl == other.AvatarUrl && Equals(Person, other.Person) && Equals(Bot, other.Bot);

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            var hs = new HashCode();
            hs.Add(Id);
            hs.Add(UserType);
            hs.Add(Name);
            hs.Add(AvatarUrl);
            hs.Add(Person);
            hs.Add(Bot);
            return hs.ToHashCode();
        }
    }
}