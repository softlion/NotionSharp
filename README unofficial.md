# NotionSharp - Notion API client for C# (unofficial version)

This is an unofficial [Notion](https://notion.so) APIv3 library and website template. You can, for example, get notion pages as a RSS feed. You can also use it as a simple CMS (Content Management System).

[![NuGet][nuget-img]][nuget-link]  
![Nuget](https://img.shields.io/nuget/dt/Softlion.NotionSharp)

![publish to nuget](https://github.com/softlion/NotionSharp/workflows/publish%20to%20nuget/badge.svg)

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp
[notion-img]: https://github.com/softlion/NotionSharp/raw/master/cover.png

## Spawn a website displaying Notion pages in a few minutes

### Setup your Notion pages and get your credentials

#### Create some content  
  Create a page at the root of Notion.so (the root page), then add subpages to this page with a title and an icon.   
  For each subpage, check "shared". Don't check "Shared" on the root page. Only shared pages will be displayed by our example, so you can prepare private pages in advance without publishing them.

  When you encounter `CmsPageTitle` below in this setup, replace it with the exact title of your root page.

#### Get the credentials to access this content

  Get your Notion's credentials using the Chrome extension [cookie-editor](https://chrome.google.com/webstore/detail/cookie-editor/hlkenndednhfkekhgcdicdfddnkalmdm) by examining the cookies:  
  `TokenV2` (named `key` below), `browserId` and `userId`. These 3 values are your credentials.  

Note that these credentials expire after a couple of months.

### Option 1: Spawn the website with [kubernetes](https://kubernetes.io/)

Run these commands after having setup kubernetes:

```
helm upgrade demonotionblog helm\notionsharpblog --install -f your-own-values.yaml
start http://localhost:5080/
```

Example of a simple `your-own-values.yaml` file suitable for [minikube](https://kubernetes.io/fr/docs/setup/learning-environment/minikube/).  
Check `helm\notionsharpblog\values.yaml` for all available configurable values.

```yaml
appSettingsSecrets:
  appsettings-secrets.Production.json: |-
    {
      "Notion": {
        "CmsPageTitle": "My Blog",
        "Key": "aabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccddaabbccdd",
        "BrowserId": "aabbccdd-aabb-aabb-aabb-aabbccddaabb",
        "UserId": "eeffeeff-eeff-eeff-eeff-eeffeeffeeff",
      }
    }

#For minikube only
service:
  type: LoadBalancer
  port: 5080
```

### Option 2: Clone the website template

Issue these commands to create your customized notion website. Replace the fake credentials with yours.  
This requires the [dotnet sdk](https://dotnet.microsoft.com/download) v5+.

```
md DemoNotionBlog
cd DemoNotionBlog
dotnet new -i Softlion.NotionSharp.TemplateProjects
```

```
dotnet new blazorblog -p "MySite CMS"
dotnet user-secrets init
dotnet user-secrets set "Notion:Key" "xxXxxXXxxXxxxXXxxx...xxXxxX"
dotnet user-secrets set "Notion:BrowserId" "aabbccdd-aabb-aabb-aabb-aabbccddaabb"
dotnet user-secrets set "Notion:UserId" "eeffeeff-eeff-eeff-eeff-eeffeeffeeff"

or

dotnet new blazorblog -p "MySite CMS" --key xxXxxXXxxXxxxXXxxx...xxXxxX --browserId aabbccdd-aabb-aabb-aabb-aabbccddaabb --userId eeffeeff-eeff-eeff-eeff-eeffeeffeeff
```
```
dotnet run
```

To uninstall the template:
```
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
```

Use [Visual Studio](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [Rider](https://www.jetbrains.com/rider/) to open the solution `.sln` file and customize the website.

### Option 3: Clone the git repo

Clone the git repo then install the template:

```
dotnet new -i Demos\DemoNotionBlog
```
Then spawn a new website from this template:
```
dotnet new blazorblog -p "MySite CMS" --key xxXxxXXxxXxxxXXxxx...xxXxxX --browserId aabbccdd-aabb-aabb-aabb-aabbccddaabb --userId eeffeeff-eeff-eeff-eeff-eeffeeffeeff
dotnet run
```

To uninstall the template:
```
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
```

### Option 4: Using Docker

A dockerfile is included with the demo/template project.  
In Visual Studio, right click the demo project and choose "publish" to publish it using the UI.  
Or use these commands:

```
docker build -t yourdockerhub.com/demonotionblog:1.0.0 .
docker run -p8080:5000 yourdockerhub.com/demonotionblog:1.0.0
start http://localhost:8080/
```

Note: the prebuilt docker image vapolia/demonotionblog:latest is made only for `helm` (ie: kubernetes) and is missing the appsettings-secrets.Production.json file.

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

There seems to be only one space per (personal) account.  
All pages are blocks. All paragraphs, photos, items in a page are also blocks. `Block.Type` contains the block's type. A block can inherit from at most one other block.  
All item's ids are Guids.  
For a `page` block, `Block.Content` contains the ids of the child pages.  

Block types rendered by this library:
- page, collection_view_page
- text, image, header, sub_header, sub_sub_header, bulleted_list, quote, column_list, column

Unimplemented block types (yet, up to grabs):
- code, bookmark
