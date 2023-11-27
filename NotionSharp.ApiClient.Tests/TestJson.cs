using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Lib.HtmlRendering;

namespace NotionSharp.ApiClient.Tests;

public static class JsonTextSerializerOptions
{
    public static JsonSerializerOptions SpecialOptions { get; } = new (JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}

public class TestDefault
{
    [JsonIgnore]
    public JsonElement TheValue => Values["value"];
        
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Values { get; set; }
}

[TestClass]
public class TestJson
{
    /// <summary>
    /// NOTE: NOW FIXED. STILL HAVE TO UPDATE CODE TO USE THAT NEW FEATURE.
    /// https://github.com/dotnet/runtime/issues/68895
    /// System.Text.Json does not fill JsonExtensionData when a property with the same name already exists or has the JsonIgnoreAttribute
    /// For .net8 ...
    /// </summary>
    [TestMethod]
    public void TestJsonExtensionData_Inherited()
    {
        var json = @"{ ""value"": { ""content"": ""test"" } }";
        var r = JsonSerializer.Deserialize<TestDefault>(json, JsonTextSerializerOptions.SpecialOptions);
        Assert.IsNotNull(r);
        Assert.IsNotNull(r.Values);
        Assert.AreEqual(1, r.Values.Count);
        Assert.IsTrue(r.Values.ContainsKey("value"));
        Assert.IsNotNull(r.TheValue);
    }

    [TestMethod]
    public void TestEmoji()
    {
        var emojiString = "🇫🇷";
        var url = emojiString.GetTwitterEmojiUrl().ToString();
        Assert.AreEqual("https://cdn.jsdelivr.net/gh/twitter/twemoji/assets/svg/1f1eb-1f1f7.svg", url);

        emojiString = "♻️";
        url = emojiString.GetTwitterEmojiUrl().ToString();
        Assert.AreEqual("https://cdn.jsdelivr.net/gh/twitter/twemoji/assets/svg/267b.svg", url);
    }
}