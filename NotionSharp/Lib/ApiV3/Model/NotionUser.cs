namespace NotionSharp.Lib.ApiV3.Model
{
    public class NotionUser : BaseModel
    {
        public string Email => Value.GetProperty("email").GetString();
        public string FirstName => Value.GetProperty("given_name").GetString();
        public string LastName => Value.GetProperty("family_name").GetString();
        public string ProfilePhoto => Value.GetProperty("profile_photo").GetString();
        public bool OnboardingComplete => Value.GetProperty("onboarding_completed").GetBoolean();
    }
}
