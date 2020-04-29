using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;
using NotionSharp.Lib;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionApiV3
    {
        [TestMethod]
        public async Task TestGetUserTasks()
        {
            var sessionInfo = GetTestInfo();
            var session = new NotionSession(sessionInfo);

            var userTasks = await session.GetUserTasks(new object());
            Assert.IsNotNull(userTasks);

            var clientExperiments = await session.GetClientExperiments(Guid.Parse("53d14533-94b9-4e87-aec8-9378b2bcedcc"));
            Assert.IsNotNull(clientExperiments);
            Assert.IsTrue(clientExperiments.Experiments.Count > 0);

            var userContent = await session.LoadUserContent();
            Assert.IsNotNull(userContent);
        }

        private NotionSessionInfo GetTestInfo()
        {
            //notioncsharp@yopmail.com
            //http://yopmail.com/notioncsharp
            return new NotionSessionInfo
            {
                TokenV2 = "f8bd7892dcdaa690df1dc36f2a4e306b23563564d9950a734460868e878c36d835868f79bc649faca0a40885b26c4886fa57c9ecc0e3413719ef17128c941c2fb62dbdf7533cf427f39e22ed5449",
                NotionBrowserId = Guid.Parse("deff37ea-82a6-4a67-a112-00dc73043c75"),
                NotionUserId = Guid.Parse("ab9257e1-d027-4494-8792-71d90b63dd35")
            };
        }
    }
}
