using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;
using NotionSharp.Lib;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionCore
    {
        [TestMethod]
        public async Task TestGetUserTasks()
        {
            var sessionInfo = TestUtils.CreateNotionSessionInfo();
            var session = new NotionSession(sessionInfo);

            var userTasks = await session.GetUserTasks(new object());
            Assert.IsNotNull(userTasks);

            var clientExperiments = await session.GetClientExperiments(Guid.Parse("53d14533-94b9-4e87-aec8-9378b2bcedcc"));
            Assert.IsNotNull(clientExperiments);
            Assert.IsTrue(clientExperiments.Experiments.Count > 0);

            var userContent = await session.LoadUserContent();
            Assert.IsNotNull(userContent);
        }

        [TestMethod]
        public async Task TestLoadPageChunkResult()
        {
            var sessionInfo = TestUtils.CreateNotionSessionInfo();
            var session = new NotionSession(sessionInfo);

            var pageChunk = await session.LoadPageChunk(Guid.Parse("4e4999b4-161a-449d-bbd1-bdbce690c7cb"), 0, 50);
            Assert.IsNotNull(pageChunk);
            Assert.IsTrue(pageChunk.RecordMap.Block.Count > 0);
        }


        [TestMethod]
        public void TestEpoch()
        {
            Assert.AreEqual(new DateTimeOffset(2020,03,16,15,25,0, TimeSpan.Zero), 1584372300000.EpochToDateTimeOffset());
        }

        [TestMethod]
        public void TestGetPageUri()
        {
            var uri = NotionUtils.GetPageUri(new Guid("e238fae31112431696d4b192b4c87f4c"), "Creating a good Xamarin Forms control - UI Day 2");
            Assert.AreEqual("https://www.notion.so/Creating-a-good-Xamarin-Forms-control-UI-Day-2-e238fae31112431696d4b192b4c87f4c", uri.ToString());
            
            uri = NotionUtils.GetPageUri(new Guid("7ac0d129bdfe419c975328961d74f7bc"), "VSCode success: manage Windows Server 2019 (Core) remotely");
            Assert.AreEqual("https://www.notion.so/VSCode-success-manage-Windows-Server-2019-Core-remotely-7ac0d129bdfe419c975328961d74f7bc", uri.ToString());
        }
    }
}
