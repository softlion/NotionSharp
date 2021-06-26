# WORK IN PROGRESS  

State 2021/06/26: all APIs are working except:
- block/append
- page/create and update
- database/get, query and list

Those are not priority and up for grabs.


# NotionSharp.ApiClient - client for official Notion API in C#

This is an unofficial [Notion](https://notion.so) public API beta library and website template. You can, for example, get notion pages as a RSS feed. You can also use it as a simple CMS (Content Management System).

[![NuGet][nuget-img]][nuget-link]  
![Nuget](https://img.shields.io/nuget/dt/Softlion.NotionSharp.ApiClient)

![publish to nuget](https://github.com/softlion/NotionSharp/workflows/publish%20to%20nuget/badge.svg)

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp.ApiClient/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp.ApiClient

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

```powershell
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

```powershell
md DemoNotionBlog
cd DemoNotionBlog
dotnet new -i Softlion.NotionSharp.TemplateProjects
```

```powershell
dotnet new blazorblog -p "MySite CMS"
dotnet user-secrets init
dotnet user-secrets set "Notion:Token" "secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx"

or

dotnet new blazorblog -p "MySite CMS" --token secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx
```
```powershell
dotnet run
```

To uninstall the template:
```powershell
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
```

Use [Visual Studio](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [Rider](https://www.jetbrains.com/rider/) to open the solution `.sln` file and customize the website.
  
  



### Option 3: Cloning the git repo

Clone the git repo then install the template:

```powershell
dotnet new -i Demos\DemoNotionBlog
```
Then spawn a new website from this template:
```powershell
dotnet new blazorblog -p "MySite CMS" --key secret_9BXXXxxxxxxxxxXXXXXXXXXXXxxxxxxxxxxxxxxx
dotnet run
```

To uninstall the template:
```powershell
dotnet new -u Softlion.NotionSharp.TemplateProject.Blog
``` 
  
  



### Option 4: Using Docker

A dockerfile is included with the demo/template project.
In Visual Studio, right click the demo project and choose "publish" to publish it using the UI.
Or use these commands:

```powershell
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

## References

[Notion official API docs](https://developers.notion.com/reference/intro)
[Notion official API guides](https://developers.notion.com/docs)
[Notion official API changelog](https://developers.notion.com/changelog)

