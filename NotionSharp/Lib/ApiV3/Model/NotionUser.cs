namespace NotionSharp.Lib.ApiV3.Model
{
    public class NotionUser : BaseModel
    {
        public string Email => TheValue.GetProperty("email").GetString();
        public string FirstName => TheValue.GetProperty("given_name").GetString();
        public string LastName => TheValue.GetProperty("family_name").GetString();
        public string ProfilePhoto => TheValue.GetProperty("profile_photo").GetString();
        public bool OnboardingComplete => TheValue.GetProperty("onboarding_completed").GetBoolean();
    }
}
