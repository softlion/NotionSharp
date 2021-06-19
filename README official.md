# WORK IN PROGRESS  
WARNING  
** THE CLIENT FOR THE OFFICIAL API IS NOT YET AVAILABLE **

# NotionSharp - Notion API client for C#

This is an unofficial [Notion](https://notion.so) public API beta library and website template. You can, for example, get notion pages as a RSS feed. You can also use it as a simple CMS (Content Management System).

[![NuGet][nuget-img]][nuget-link]  
![Nuget](https://img.shields.io/nuget/dt/Softlion.NotionSharp)

![publish to nuget](https://github.com/softlion/NotionSharp/workflows/publish%20to%20nuget/badge.svg)

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp

## Spawn a website displaying Notion pages in a few minutes

### Setup your Notion pages and get your credentials

#### Create some content  
  Create a page at the root of Notion.so (the root page), then add subpages to this page with a title and an icon.

  When you encounter `CmsPageTitle` below in this setup, replace it with the exact title of your root page.

#### Get the credentials to access this content

  Connect to your [notion.so](notion.so) account first then [navigate here](https://www.notion.so/my-integrations) and create a new integration.  
  Copy the `Internal Integration Token`, this is your credential (named `Token` below).

  Go back to your content, and share your root page with the integration you just created (tap in the invite input zone to bring the integration selector).

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
        "Token": "secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx"
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
dotnet user-secrets set "Notion:Token" "secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx"

or

dotnet new blazorblog -p "MySite CMS" --token secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx
```
```
dotnet run
```

To uninstall the template:
```
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
```

Use [Visual Studio](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [Rider](https://www.jetbrains.com/rider/) to open the solution `.sln` file and customize the website.

### Option 3: Cloning the git repo

Clone the git repo then install the template:

```
dotnet new -i Demos\DemoNotionBlog
```
Then spawn a new website from this template:
```
dotnet new blazorblog -p "MySite CMS" --key secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx
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
    var sessionInfo = new NotionSessionInfo2 
    {
        Token = "secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx"
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
All pages are blocks. All paragraphs, photos, items in a page are also blocks. `Block.Type` contains the block's type.  
For a `page` block, `Block.Content` contains the ids of the child pages.  
All item's ids are Guids.

Rendered block types:
- page, collection_view_page
- text, image, header, sub_header, sub_sub_header, bulleted_list, quote, column_list, column

Unimplemented block types (yet):
- code, bookmark
