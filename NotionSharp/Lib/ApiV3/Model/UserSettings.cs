using System.Text.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class UserSettings : BaseModel
    {
        public Settings Settings => TheValue.GetProperty("settings").Deserialize<Settings>();
    }
}
