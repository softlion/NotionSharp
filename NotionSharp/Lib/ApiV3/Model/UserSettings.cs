namespace NotionSharp.Lib.ApiV3.Model
{
    public class UserSettings : BaseModel
    {
        public Settings Settings => Value["settings"].ToObject<Settings>();
    }
}
