# NotionSharp - Notion API client for C#

[Notion](https://notion.so) is an app to write things. It has the most simple "wysiwyg" editor i have ever seen. Add photo, video, tables and more in one drag/drop or one click. For devs it features code blocks with highlightning.  
The app works both offline and online, is multi user and historize all your changes making it easy to revert them.  
The app is also available in the Microsoft Store for Windows and on mobile phones to peacefully write your contents offline.

This is an unofficial [Notion](https://notion.so) APIv3 library and website template. You can, for example, get notion pages as a RSS feed. You can also use it as a simple CMS (Content Management System).

[![NuGet][nuget-img]][nuget-link]  
![Nuget](https://img.shields.io/nuget/dt/Softlion.NotionSharp)

![publish to nuget](https://github.com/softlion/NotionSharp/workflows/publish%20to%20nuget/badge.svg)

![notion-img]

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp
[notion-img]: https://github.com/softlion/NotionSharp/raw/master/cover.png

## Use Cases

Spawn a Website displaying the pages built with Notion in a few seconds. See below 'kubernetes'.

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

## Spawn a website displaying Notion pages in a few minutes

### Setup your Notion pages and get your credentials

#### Create some content  
  Create a page at the root of Notion.so (the root page), then add subpages to this page with a title and an icon.   
  For each subpage, check "shared". Don't check "Shared" on the root page. Only shared pages will be displayed by our example, so you can prepare private pages in advance without publishing them.

  When you encounter `CmsPageTitle` below in this setup, replace it with the exact title of your root page.

#### Next steps
- Using the [unofficial integration](README unofficial.md)
  

- Using the [notion's public api beta](README official.md) (new: official integration)

## Sponsors

Big Thanks to those sponsors for supporting this project and keeping it free.

- [vapolia.eu](https://vapolia.eu)
