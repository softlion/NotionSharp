using NotionSharp.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotionSharpTest
{
    class TestUtils
    {
        public static NotionSessionInfo CreateNotionSessionInfo()
        {
            //notioncsharp@yopmail.com
            //http://yopmail.com/notioncsharp
            return new NotionSessionInfo
            {
                TokenV2 = "198f2adfc90e1a91d7b8cc8a63272e1cc75ef9bfdacd35f9e0a3fcf3b28cd817ec427f72be3c2634c75c7f4401d6061c13852725c855c844b6bf8d2c677ac25c52d5ce76caa3c3bc215487587c7c",
                NotionBrowserId = Guid.Parse("f8a52372-15da-494f-ae3f-54ea2f80fe6b"),
                NotionUserId = Guid.Parse("ab9257e1-d027-4494-8792-71d90b63dd35")
            };
        }
    }
}
