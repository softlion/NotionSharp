# NotionSharp - Notion API client for C#

New! Notion powered Website Demo [here](https://raids.notionsharp.eu/).

[Notion](https://notion.so) is an app to write things. It has the most simple "wysiwyg" editor i have ever seen. Add photo, video, tables and more in one drag/drop or one click. For devs it features code blocks with highlightning.  
The app works both offline and online, is multi user and historize all your changes making it easy to revert them.  
The app is also available in the Windows Store to peacefully write your content offline.

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

- Create a page at the root of Notion.so (the root page), then add subpages to this page with a title and an icon. For each subpage, check "shared". Don't check "Shared" on the root page.
- Get your Notion's credentials using the Chrome extension [cookie-editor](https://chrome.google.com/webstore/detail/cookie-editor/hlkenndednhfkekhgcdicdfddnkalmdm) by examining the cookies (TokenV2 => key, browserId and userId). Replace CmsPageTitle below with the exact title of your root page.

### Spawn the website with [kubernetes](https://kubernetes.io/)

Run these commands after having setup kubernetes:

```
helm upgrade demonotionblog helm\notionsharpblog --install -f your-value.yaml
start http://localhost:5080/
```

Example of a simple `your-values.yaml` file suitable for [minikube](https://kubernetes.io/fr/docs/setup/learning-environment/minikube/).  
Check `helm\notionsharpblog\values.yaml` for all the configurable values.

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

### Cloning the website template

Issue these commands to create your customized notion website. Replace the fake credentials with yours.

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

Use Visual Studio or VS Code to open the solution (.sln) file and customize the website.

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

Rendered block types:
- page, collection_view_page
- text, image, header, sub_header, sub_sub_header, bulleted_list, quote, column_list, column

Unimplemented block types (yet):
- code, bookmark
