# Node build step
FROM public.ecr.aws/docker/library/node:18 as node_build
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
COPY HerPortal.ManagementShell/*.csproj HerPortal.ManagementShell/
COPY HerPortal.UnitTests/*.csproj HerPortal.UnitTests/
COPY Lib/ Lib/
RUN dotnet restore --use-current-runtime

# copy and publish app and libraries
COPY . .
RUN dotnet publish HerPortal/ --use-current-runtime --self-contained false --no-restore -o /app
RUN dotnet build HerPortal.ManagementShell/ --use-current-runtime --self-contained false --no-restore -o /cli


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
COPY --from=build /cli ./cli
ENTRYPOINT ["dotnet", "HerPortal.dll"]
