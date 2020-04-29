# NotionSharp
Unofficial [notion.so](https://notion.so) APIv3 library

[![NuGet][nuget-img]][nuget-link]

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp

## Usage

```csharp
public async Task GetUserContent()
{
    var sessionInfo = new NotionSessionInfo 
    {
        TokenV2 = "paste here the content of the token_v2 cookie set after you logged in notion.so",
        NotionBrowserId = Guid.Parse("00000000-0000-0000-0000-000000000000"), //paste the content of the notion_browser_id cookie
        NotionUserId = Guid.Parse("00000000-0000-0000-0000-000000000000") //paste the content of the notion_user_id cookie
    };

    var session = new NotionSession(sessionInfo);
    var userContent = await session.LoadUserContent();
    return userContent;
}
```
