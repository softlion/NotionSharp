using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionBlocks
    {
        [TestMethod]
        public void TestParseImage()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData\Blocks", "image.json"));
            var block = JsonSerializer.Deserialize<Block>(json);
            Assert.IsNotNull(block);
            Assert.AreEqual("image", block.Type);
            var imageData = block.ToImageData();
            Assert.IsNotNull(imageData);
            Assert.IsNotNull(imageData.ImageUrl);
            Assert.IsNotNull(imageData.Format);
            Assert.AreEqual("Ce schéma montre que la ....", imageData.Caption);
        }
        
        [TestMethod]
        public void TestParseCallout()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData\Blocks", "callout.json"));
            var block = JsonSerializer.Deserialize<Block>(json);
            Assert.IsNotNull(block);
            Assert.AreEqual("callout", block.Type);

            var calloutData = block.ToCalloutData(true);
            Assert.IsNotNull(calloutData);
            Assert.IsNotNull(calloutData.Text?.Lines);
            Assert.IsTrue(calloutData.Text.Lines.Count > 0);
            Assert.IsTrue(calloutData.Text.Lines[0].Text.StartsWith("Cout de dev"));
            Assert.IsNotNull(calloutData.Format);
            Assert.AreEqual("💶", calloutData.Format.PageIcon);
            Assert.AreEqual("gray_background", calloutData.Format.BlockColor);
        }
    }
}
