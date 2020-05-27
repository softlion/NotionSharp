# NotionSharp - Notion API client for C#

[Notion](https://notion.so) is an all-in-one workspace to write things, with the best simple and powerful "wysiwyg" editor, with color highlightning for code blocks, easy video interations, and much more. It includes an offline windows application to peacefully write your blog's content.

This is an unofficial [Notion](https://notion.so) APIv3 library that enables you, among other things, to get a list of pages as a RSS feed, as notion has no RSS feed as of 2020/04/30.

You can ulso use it as a CMS (Content Management System) for his nice editing capabilities.

[![NuGet][nuget-img]][nuget-link]

![publish to nuget](https://github.com/softlion/NotionSharp/workflows/publish%20to%20nuget/badge.svg)

![notion-img]

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp
[notion-img]: https://github.com/softlion/NotionSharp/raw/master/cover.png

## Use Cases

Get a RSS representation of all root pages of the main space.  
This also transforms notion data to HTML:

```csharp
    var rssFeed = await session.GetSyndicationFeed();
```
    
    
Get a RSS representation from the sub-pages of a page:

```csharp
    var space = userContent.RecordMap.Space.First().Value;
    var firstPage = userContent.RecordMap.Block[space.Pages[0]];
    var subPages = firstPage.Content;

    var feedPublicBlog = await session.GetSyndicationFeed(subPages);
    feedPublicBlog.Title = new TextSyndicationContent(firstPage.Title);
```

## Testing the Notion blog demo

- Get your Notion's credentials using fiddler (TokenV2 => key, browserId and userId)
- Create a public page at the root of Notion, then add subpages to this page with title and icon.

Issue these commands to create your own notion blog demo. Replace the fake credentials with yours.

```
md DemoNotionBlog
cd DemoNotionBlog
dotnet new -i Softlion.NotionSharp.TemplateProjects
dotnet new blazorblog -p "MySite CMS"
dotnet user-secrets init
dotnet user-secrets set "Notion:Key" "xxXxxXXxxXxxxXXxxx...xxXxxX"
dotnet user-secrets set "Notion:BrowserId" "BB083879-F2DA-4DF6-ADFB-C26344981DC3"
dotnet user-secrets set "Notion:UserId" "BB083879-F2DA-4DF6-ADFB-C26344981DC3"
dotnet run
```

## Usage

Create a session:

```csharp
    var sessionInfo = new NotionSessionInfo 
    {
        TokenV2 = "paste here the content of the token_v2 cookie set after you logged in notion.so",
        NotionBrowserId = Guid.Parse("00000000-0000-0000-0000-000000000000"), //paste the content of the notion_browser_id cookie
        NotionUserId = Guid.Parse("00000000-0000-0000-0000-000000000000") //paste the content of the notion_user_id cookie
    };

    var session = new NotionSession(sessionInfo);
```

Example: load content and info of main `space`:

```csharp
    var userContent = await session.LoadUserContent();
    var html = userContent.RecordMap.GetHtml(throwIfBlockMissing: false);
```

## Notion high-level model

Spaces  
|- Pages  
....|-- Sub-Pages

There seems to be only one space per (personnal) account.  
All pages are blocks. All paragraphs, photos, items in a page are also blocks. `Block.Type` contains the block's type.  
For a `page` block, `Block.Content` contains the ids of the child pages.  
All item's ids are Guids.

Known block types are:
- page, collection_view_page
- text, image, sub_header, quote, code, bookmark
