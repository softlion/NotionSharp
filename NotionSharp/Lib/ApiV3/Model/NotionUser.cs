namespace NotionSharp.Lib.ApiV3.Model
{
    public class NotionUser : BaseModel
    {
        public string Email => (string)Value["email"];
        public string FirstName => (string)Value["given_name"];
        public string LastName => (string)Value["family_name"];
        public string ProfilePhoto => (string)Value["profile_photo"];
        public bool OnboardingComplete => (bool)Value["onboarding_completed"];
    }
}
