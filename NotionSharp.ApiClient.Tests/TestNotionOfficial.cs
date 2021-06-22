using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Tests.Lib;

namespace NotionSharp.ApiClient.Tests
{
    [TestClass]
    public class TestNotionOfficial
    {
        /// <summary>
        /// Automatic paging
        /// </summary>
        [TestMethod]
        public async Task TestSearch()
        {
            var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());

            var totalItems = 0;
            await foreach (var item in session.Search(pageSize: 2))
            {
                totalItems++;
            }

            Assert.AreEqual(4, totalItems);
        }

        /// <summary>
        /// Manual paging
        /// </summary>
        [TestMethod]
        public async Task TestSearchPaged()
        {
            var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());

            var pagingOptions = new PagingOptions {PageSize = 2};
            var searchResult = await session.SearchPaged(pagingOptions: pagingOptions);
            Assert.IsNotNull(searchResult?.Results);
            Assert.IsTrue(searchResult.Results[0] is JsonElement);

            var totalItems = searchResult.Results.Count;
            var i = 10;
            while (searchResult.HasMore && i-- > 0)
            {
                pagingOptions.StartCursor = searchResult.NextCursor;
                searchResult = await session.SearchPaged(pagingOptions: pagingOptions);
                totalItems += searchResult.Results.Count;
            }
            Assert.AreNotEqual(0, i);
            Assert.AreEqual(4, totalItems);
        }

        [TestMethod]
        public async Task TestGetUsers()
        {
            var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
            var users = session.GetUsers();
            Assert.IsNotNull(users);
            int i, iBot, iPerson;
            i = iBot = iPerson = 0;
            await foreach (var user in users)
            {
                i++;
                if (user.UserType == UserTypeConst.Bot)
                    iBot++;
                if (user.UserType == UserTypeConst.Person)
                    iPerson++;
            }
            
            Assert.AreEqual(2, i);
            Assert.AreEqual(1, iBot);
            Assert.AreEqual(1, iPerson);
        }
        
        [TestMethod]
        public async Task TestGetUser()
        {
            var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
            var user1a = await session.GetUsers().FirstAsync();
            Assert.IsNotNull(user1a);
            
            var user1b = await session.GetUser(user1a.Id);
            
            Assert.IsTrue(user1a.Equals(user1b));
        }
        
        [TestMethod]
        public async Task TestGetPage()
        {
            var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
            //Get any page returned by search
            var page = await session.Search(pageSize: 1, filterOptions: FilterOptions.ObjectPage).FirstAsync();
            Assert.IsNotNull(page);

            var pageId = page.GetProperty("id").GetString();
            Assert.IsNotNull(pageId);

            //Get the page details
            var pageProperties = await session.GetPage(pageId);
            Assert.IsNotNull(pageProperties);
        }
    }
}
