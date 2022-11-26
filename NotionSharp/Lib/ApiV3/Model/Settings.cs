
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Settings
    {
        public string Type { get; set; }
        public string Locale { get; set; }
        public string Source { get; set; }
        public string Persona { get; set; }
        [JsonPropertyName("time_zone")]
        public string TimeZone { get; set; }
        [JsonPropertyName("signup_time")]
        public long SignupTime { get; set; }
        [JsonPropertyName("used_windows_app")]
        public long UsedWindowsApp { get; set; }
        [JsonPropertyName("used_desktop_web_app")]
        public long UsedDesktopWebApp { get; set; }
    }
}
