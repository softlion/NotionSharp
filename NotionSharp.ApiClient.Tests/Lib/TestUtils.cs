using NotionSharp.ApiClient.Lib.Model;

namespace NotionSharp.ApiClient.Tests.Lib
{
    class TestUtils
    {
        public static NotionSessionInfo CreateOfficialNotionSessionInfo()
        {
            //notioncsharp@yopmail.com
            //http://yopmail.com/notioncsharp
            return new NotionSessionInfo
            {
                Token = "secret_jbPRU7vdj8hmpKFnT3ntld4mcXg4dTOYuqsVc7hj9KF",
            };
        }
    }
}
