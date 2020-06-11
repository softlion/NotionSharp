using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NotionSharp;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionBlocks
    {
        [TestMethod]
        public void TestParseImageBlock()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData\Blocks", "image.json"));
            var block = JsonConvert.DeserializeObject<Block>(json);
            Assert.IsNotNull(block);
            Assert.AreEqual("image", block.Type);
            var imageData = block.ToImageData();
            Assert.IsNotNull(imageData);
            Assert.IsNotNull(imageData.ImageUrl);
            Assert.IsNotNull(imageData.Format);
            Assert.AreEqual("Ce schéma montre que la ....", imageData.Caption);
        }
    }
}
