# Node build step
FROM node:18 as node_build
COPY HerPortal /HerPortal
WORKDIR /HerPortal
RUN npm ci
RUN npm run build

# C# build step
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY --from=node_build . .
COPY *.sln .
COPY nuget.config .
COPY HerPortal/*.csproj HerPortal/
COPY HerPortal.BusinessLogic/*.csproj HerPortal.BusinessLogic/
COPY HerPortal.Data/*.csproj HerPortal.Data/
# COPY HerPortal.ManagementShell/*.csproj HerPortal.ManagementShell/
COPY Lib/ Lib/
RUN dotnet restore HerPortal/ --use-current-runtime
# RUN dotnet restore HerPortal.ManagementShell/ --use-current-runtime

# copy and publish app and libraries
COPY . .
RUN dotnet publish HerPortal/ --use-current-runtime --self-contained false --no-restore -o /app
# RUN dotnet build HerPortal.ManagementShell/ --use-current-runtime --self-contained false --no-restore -o /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "HerPortal.dll"]
