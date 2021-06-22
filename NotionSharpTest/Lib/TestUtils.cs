using NotionSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotionSharpTest
{
    class TestUtils
    {
        public static NotionSessionInfo CreateUnofficialNotionSessionInfo()
        {
            //notioncsharp@yopmail.com
            //http://yopmail.com/notioncsharp
            return new NotionSessionInfo
            {
                TokenV2 = "b630bfb31358f9aa11d511b655288fcd1ec49667ac29390e2428fb58f4dd02fbbb2d5d1c9294b471869a86e3d2d936a77744a5ff0b7a890563127fffe1fb52b82d2666e2f0032616084011620f81",
                NotionBrowserId = Guid.Parse("592b443c-5ca9-425f-b519-9c040c922e61"),
                NotionUserId = Guid.Parse("ab9257e1-d027-4494-8792-71d90b63dd35")
            };
        }
    }
}
