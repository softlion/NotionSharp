using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;
using NotionSharp.Lib;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionOfficial
    {
        // [TestMethod]
        // public async Task TestGetUserTasks()
        // {
        //     var session = new NotionSession2(TestUtils.CreateOfficialNotionSessionInfo());
        //
        //     var userTasks = await session.GetUserTasks(new object());
        //     Assert.IsNotNull(userTasks);
        //
        //     var clientExperiments = await session.GetClientExperiments(Guid.Parse("53d14533-94b9-4e87-aec8-9378b2bcedcc"));
        //     Assert.IsNotNull(clientExperiments);
        //     Assert.IsTrue(clientExperiments.Experiments.Count > 0);
        //
        //     var userContent = await session.LoadUserContent();
        //     Assert.IsNotNull(userContent);
        // }
        //
        // [TestMethod]
        // public async Task TestLoadPageChunkResult()
        // {
        //     var session = new NotionSession2(TestUtils.CreateOfficialNotionSessionInfo());
        //
        //     var pageChunk = await session.LoadPageChunk(Guid.Parse("4e4999b4-161a-449d-bbd1-bdbce690c7cb"), 0, 50);
        //     Assert.IsNotNull(pageChunk);
        //     Assert.IsTrue(pageChunk.RecordMap.Block.Count > 0);
        // }
    }
}
