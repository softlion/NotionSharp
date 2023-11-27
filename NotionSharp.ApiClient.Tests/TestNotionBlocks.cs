using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Tests.Lib;

namespace NotionSharp.ApiClient.Tests;

[TestClass]
public class TestNotionBlocks
{
        
    [TestMethod]
    public async Task TestBlockColumnList()
    {
        // var blocksJson = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.Children.json"));
        // var blocks = JsonSerializer.Deserialize<PaginationResult<Block>>(blocksJson, HttpNotionSession.NotionJsonSerializationOptions);
        // Assert.IsNotNull(blocks);
        // var columnList = blocks.Results.First(b => b.Type == BlockTypes.ColumnList);

        var columnJson = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.ColumnList.Children.json"));
        var columns = JsonSerializer.Deserialize<PaginationResult<Block>>(columnJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(columns?.Results);
        Assert.AreEqual(2, columns!.Results.Count);
        
        // var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        // await session.GetChildren(columns.Results[0]);
        // await session.GetChildren(columns.Results[1]);
        
        var column1Json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.Column.1.Children.json"));
        var column1 = JsonSerializer.Deserialize<PaginationResult<Block>>(column1Json, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(column1.Results);
        var column2Json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.Column.2.Children.json"));
        var column2 = JsonSerializer.Deserialize<PaginationResult<Block>>(column2Json, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(column2.Results);
    }

    [TestMethod]
    public async Task TestVapoliaFr()
    {
        var vapoliaCmsJson = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "VapoliaCms.json"));
        var page = JsonSerializer.Deserialize<PaginationResult<Page>>(vapoliaCmsJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(page);
    }
}