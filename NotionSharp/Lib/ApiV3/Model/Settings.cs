using Newtonsoft.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Settings
    {
        public string Type { get; set; }
        public string Locale { get; set; }
        public string Source { get; set; }
        public string Persona { get; set; }
        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }
        [JsonProperty("signup_time")]
        public long SignupTime { get; set; }
        [JsonProperty("used_windows_app")]
        public long UsedWindowsApp { get; set; }
        [JsonProperty("used_desktop_web_app")]
        public long UsedDesktopWebApp { get; set; }
    }
}
