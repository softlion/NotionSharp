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

### Setup your Notion pages and get your credentials

- Create a public page at the root of Notion.so, then add subpages to this page with a title and an icon.
- Get your Notion's credentials using [fiddler](https://www.telerik.com/fiddler) by examining the cookies (TokenV2 => key, browserId and userId)

### Using kubernetes

```
helm upgrade demonotionblog helm\notionsharpblog --install -f your-value.yaml
start http://localhost:5080/
```

This is an example of a simple `your-values.yaml` file suitable for minikube. Check `helm\notionsharpblog\values.yaml` for the configurable values.

```yaml
appSettingsSecrets:
  appsettings-secrets.Production.json: |-
    {
      "Notion": {
        "Key": "aabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccdd",
        "BrowserId": "aabbccdd-aabb-aabb-aabb-aabbccddaabb",
        "UserId": "eeffeeff-eeff-eeff-eeff-eeffeeffeeff",
        "CmsPageTitle": "My Blog"
      }
    }

#For minikube only
service:
  type: LoadBalancer
  port: 5080
```

### Cloning the template using dotnetcore

Issue these commands to create your own notion blog demo. Replace the fake credentials with yours.

```
md DemoNotionBlog
cd DemoNotionBlog
dotnet new -i Softlion.NotionSharp.TemplateProjects

dotnet new blazorblog -p "MySite CMS"
dotnet user-secrets init
dotnet user-secrets set "Notion:Key" "xxXxxXXxxXxxxXXxxx...xxXxxX"
dotnet user-secrets set "Notion:BrowserId" "aabbccdd-aabb-aabb-aabb-aabbccddaabb"
dotnet user-secrets set "Notion:UserId" "eeffeeff-eeff-eeff-eeff-eeffeeffeeff"

or

dotnet new blazorblog -p "MySite CMS" --key xxXxxXXxxXxxxXXxxx...xxXxxX --browserId aabbccdd-aabb-aabb-aabb-aabbccddaabb --userId eeffeeff-eeff-eeff-eeff-eeffeeffeeff

dotnet run
```

To uninstall the template:
```
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
```

### Cloning the git repo

```
//Install the template
dotnet new -i Demos\DemoNotionBlog

dotnet new blazorblog -p "MySite CMS" --key xxXxxXXxxXxxxXXxxx...xxXxxX --browserId aabbccdd-aabb-aabb-aabb-aabbccddaabb --userId eeffeeff-eeff-eeff-eeff-eeffeeffeeff

dotnet run
```

To uninstall the template:
```
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
```

### Using Docker

A dockerfile is included with the demo/template project.
Right clic the demo project and choose "publish" to publish it using the UI.
Or use these commands:

```
docker build -t yourdockerhub.com/demonotionblog:1.0.0 .
docker run -p8080:5000 yourdockerhub.com/demonotionblog:1.0.0
start http://localhost:8080/
```

Note: the prebuilt docker image vapolia/demonotionblog:latest is made for helm and is missing the appsettings-secrets.Production.json file.

## SDK Usage

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
- text, image, header, sub_header, quote, code, bookmark
