#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Demos/DemoNotionBlog/DemoNotionBlog.csproj", "Demos/DemoNotionBlog/"]
COPY ["NotionSharp/NotionSharp.csproj", "NotionSharp/"]
RUN dotnet restore "Demos/DemoNotionBlog/DemoNotionBlog.csproj"
COPY . .
WORKDIR "/src/Demos/DemoNotionBlog"
RUN dotnet build "DemoNotionBlog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoNotionBlog.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
#EXPOSE 5000
EXPOSE 80
COPY --from=publish /app/publish .
#ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "DemoNotionBlog.dll"]