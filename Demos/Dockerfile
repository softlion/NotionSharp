FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS publish
WORKDIR /src
COPY . .
RUN dotnet restore DemoNotionBlog
ARG configuration=Release
RUN dotnet build DemoNotionBlog -c $configuration -o /app/build
RUN dotnet publish DemoNotionBlog -c $configuration -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 5000
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "DemoNotionBlog.dll"]
