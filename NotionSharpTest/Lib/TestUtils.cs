using NotionSharp;
using System;

namespace NotionSharpTest;

class TestUtils
{
    public static NotionSessionInfo CreateUnofficialNotionSessionInfo()
    {
        //notioncsharp@yopmail.com
        //http://yopmail.com/notioncsharp
        return new ()
        {
            TokenV2 = "1711a84274494b70ad2619956336b5a3c6fd18abeab041e96f73e481ef915806060a159237a3a9315f4bcd35eaf766f9b390b62fa65cf2d413bed488549be82060f0328562cf14466c97584a080b",
            NotionBrowserId = Guid.Parse("36eec975-2503-4480-89a7-7c8228a245e5"),
            NotionUserId = Guid.Parse("ab9257e1-d027-4494-8792-71d90b63dd35"),
            NotionUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.56"
        };
    }
}