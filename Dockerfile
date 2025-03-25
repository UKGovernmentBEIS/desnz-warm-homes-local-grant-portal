# Node build step
FROM public.ecr.aws/docker/library/node:18 as node_build
COPY WhlgPortalWebsite /WhlgPortalWebsite
WORKDIR /WhlgPortalWebsite
RUN npm ci
RUN npm run build

# C# build step
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY --from=node_build . .
COPY *.sln .
COPY nuget.config .
COPY WhlgPortalWebsite/*.csproj WhlgPortalWebsite/
COPY WhlgPortalWebsite.BusinessLogic/*.csproj WhlgPortalWebsite.BusinessLogic/
COPY WhlgPortalWebsite.Data/*.csproj WhlgPortalWebsite.Data/
COPY WhlgPortalWebsite.ManagementShell/*.csproj WhlgPortalWebsite.ManagementShell/
COPY WhlgPortalWebsite.UnitTests/*.csproj WhlgPortalWebsite.UnitTests/
COPY Lib/ Lib/
RUN dotnet restore --use-current-runtime

# copy and publish app and libraries
COPY . .
RUN dotnet publish WhlgPortalWebsite/ --use-current-runtime --self-contained false --no-restore -o /app
RUN dotnet build WhlgPortalWebsite.ManagementShell/ --use-current-runtime --self-contained false --no-restore -o /cli


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
COPY --from=build /cli ./cli
ENTRYPOINT ["dotnet", "WhlgPortalWebsite.dll"]
