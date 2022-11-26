﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionUnofficial
    {
        [TestMethod]
        public async Task TestGetUserTasks()
        {
            var sessionInfo = TestUtils.CreateUnofficialNotionSessionInfo();
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
            var sessionInfo = TestUtils.CreateUnofficialNotionSessionInfo();
            var session = new NotionSession(sessionInfo);

            var pageId = "4e4999b4-161a-449d-bbd1-bdbce690c7cb";
            var spaceId = "da89a8e0-9c55-4c13-9966-796e0f0c1bac"; 
            var pageChunk = await session.LoadPageChunk(Guid.Parse(pageId), Guid.Parse(spaceId), 0, 30);
            Assert.IsNotNull(pageChunk);
            Assert.IsTrue(pageChunk.RecordMap.Block.Count > 0);
        }
    }
}
