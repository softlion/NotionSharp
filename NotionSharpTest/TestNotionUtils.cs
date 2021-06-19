using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;
using NotionSharp.Lib;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionUtils
    {
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