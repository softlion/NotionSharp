using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Tests.Lib;

namespace NotionSharp.ApiClient.Tests
{
    [TestClass]
    public class TestNotionOfficial
    {
        [TestMethod]
        public async Task TestSearch()
        {
            var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        
            var searchResult = await session.Search(new PagingOptions { PageSize = 10 });
            Assert.IsNotNull(searchResult?.Results);
            Assert.IsTrue(searchResult.Results[0] is JsonElement);
        }
        
        // [TestMethod]
        // public async Task TestLoadPageChunkResult()
        // {
        //     var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        //
        //     var pageChunk = await session.LoadPageChunk(Guid.Parse("4e4999b4-161a-449d-bbd1-bdbce690c7cb"), 0, 50);
        //     Assert.IsNotNull(pageChunk);
        //     Assert.IsTrue(pageChunk.RecordMap.Block.Count > 0);
        // }
    }
}
