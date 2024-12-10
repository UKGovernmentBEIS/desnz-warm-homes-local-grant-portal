# Home Energy Retrofit Portal BETA

This repository was cloned from [HUG2 Portal](https://github.com/UKGovernmentBEIS/desnz-home-energy-retrofit-portal-beta) in December 2024, keeping all previous commits.

The code was then adapted into the WH:LG service.

## Deployment

The site is deployed using github actions.

### Database Migrations

Migrations will be run automatically on deployment. If a migration needs to be rolled back for any reason there are two options:
1. Create a new inverse migration and deploy that
2. Generate and run a rollback script
   1. Check out the same commit locally
   2. [Install EF Core CLI tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) if you haven't already
   3. Generate a rollback script using `dotnet ef migrations script 2022010112345678_BadMigration 2022010112345678_LastGoodMigration -o revert.sql` from the `WhlgPortalWebsite` directory
   4. Review the script 
   5. TODO Add instructions for running the script on the Azure environment

## Development

### Process

We follow a process similar to git-flow, with 3 branches corresponding to each of the environments:
- `develop` - Dev ([https://dev.check-eligibility-for-home-upgrade-grant.service.gov.uk/portal])
- `staging` - UAT ([https://uat.check-eligibility-for-home-upgrade-grant.service.gov.uk/portal])
- `main` - Production ([https://check-eligibility-for-home-upgrade-grant.service.gov.uk/portal])

For normal development:
- Create a branch from `develop`
- Make changes on the branch, e.g. `feat/add-new-widget`
- Raise a PR back to `develop` once the feature is complete
- If the PR is accepted merge the branch into `develop`

Doing a release to staging:
- Merge `develop` into `staging`
- Deploy this branch into the UAT environment
- Run manual tests against this environment and gain sign-off to deploy

Doing a release to production:
- Ensure all sign-offs are in place
- Merge `staging` into `main`
   - To merge to main, the `production release` label must be applied to your pull request
- Deploy this branch into the production environment
- Perform any post go-live checks

For critical bug fixes on production
- Create a hotfix branch from `main`, e.g. `hotfix/update-service-name`
- Make changes on the branch
- Raise a PR back to `main` once the bug is fixed
   - To merge to main, the `production release` label must be applied to your pull request
- If the PR is accepted, merge the branch into `main`
- Then also merge the branch into `develop`

### Pre-requisites

- .Net 8 (https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Install EF Core CLI tools (https://docs.microsoft.com/en-us/ef/core/cli/dotnet)
- Node v14+ (https://nodejs.org/en/)
- If you're using Rider then you will need to install the ".net core user secrets" plugin
- Download and configure Minio (see below)
  - [Windows](https://min.io/download#/windows)
  - [Mac](https://min.io/docs/minio/macos/index.html#procedure)

In WhlgPortalWebsite run `npm install`

#### Minio

The portal site lists files hosted in an S3 bucket. For local development we need a fake S3 bucket to connect to.
To use [Minio](https://min.io/) to provide a local S3 bucket follow these steps:
1. Download minio
   * [Windows](https://min.io/download#/windows)
   * [Mac](https://min.io/docs/minio/macos/index.html#procedure)
2. Put the executable somewhere on your machine (e.g. c:\Program Files\Minio)
3. Decide on a folder for Minio to store its data in (e.g. c:\data\minio)
4. To run the server:
   * Windows
     * Open a PowerShell window and go to the folder that you put Minio in
     * Run `.\minio.exe server <path to data folder> --console-address :9090`
   * Mac:
     * Open any terminal
     * Run `export MINIO_CONFIG_ENV_FILE=/etc/default/minio`
     * Run `minio server <path to data folder> --console-address :9090`
5. The first time that you do this:
    1. Visit http://localhost:9090
    2. Login (default is minioadmin/minioadmin)
    3. Create a new bucket called `desnz-whlg-portal-referrals`

### GovUkDesignSystem

We are using the GovUkDesignSystem library from the Cabinet Office: https://github.com/cabinetoffice/govuk-design-system-dotnet

As this library is not currently published to Nuget we have a copy of the library in a nuget package in the /Lib folder of this solution.

If you need to make changes to the GovUkDesignSystem (e.g. to add a new component) then you should:
- Clone the ICS fork of the repository (currently https://github.com/UKGovernmentBEIS/govuk-design-system-dotnet) and check out the `master` branch.
- Create a branch for you feature
- Develop and commit your changes (don't forget automated tests as applicable)
- Build and package your branch with `dotnet pack -p:PackageVersion=1.0.0-$(git rev-parse --short HEAD) -c Release -o .` in the `GovUkDesignSystem` folder
- Copy the built package to /Lib and delete the old package
- Update the package version in the WHLG project
- Test that your changes work on the WHLG site
- Create a PR from your branch back to `master`
- Get the PR reviewed and merged
- From time to time create a PR to merge the `master` branch back to the Cabinet Office repository (https://github.com/cabinetoffice/govuk-design-system-dotnet)

#### GOV.UK Frontend

The GovUkDesignSystem project relies on the GOV.UK Frontend NPM package which contains images, fonts, styling, and JavaScript. When updating
the GovUkDesignSystem you may also need to update the GOV.UK Frontend NPM package. To do this:

- Update the version number of the GOV.UK Frontend package in package.json
- Run `npm install`
- Run `npm run update-govuk-assets`
- Run `npm run build`

### APIs

The app communicates with a number of APIs. You will need to obtain and configure credentials for these APIs in your user secrets file.

In Rider:
- Right-click on the `WhlgPortalWebsite` project
- Select `Tools`
- Select `Open Project User Secrets`

Fill in the opened `secrets.json` file with:

```json
{
    "Authentication": {
        "Cognito": {
            "ClientId": "<app client id from AWS Cognito>",
            "ClientSecret": "<app client secret from AWS Cognito>",
            "MetadataAddress": "https://cognito-idp.{your-region-id}.amazonaws.com/{your-user-pool-id}/.well-known/openid-configuration",
            "SignOutUrl": "{cognito-domain}/logout?client_id={client-id}&logout_uri=https://localhost:5001/portal/sign-out"
        }
    },

    "GovUkNotify": {
        "ApiKey": "<REAL_VALUE_HERE>"
    }
}
```

You can also add secrets with `dotnet user-secrets`, just pipe the JSON you want to be added to it e.g.
```
cat secrets.json | dotnet user-secrets set
```

### Running Locally

- In your Minio folder run Minio `.\minio.exe server <path to data folder> --console-address :9090`
- In Visual Studio / Rider build the solution
- In `WhlgPortalWebsite` run `npm run watch`
- In Visual Studio / Rider run the `WhlgPortalWebsite` project
- In a browser, visit https://localhost:5001/portal (the `/portal` is important!)

## Database

### Local Database Setup

#### Windows
- Download the installer and PostgreSQL 14 [here](https://www.postgresql.org/download/windows/)
- Follow default installation steps (no additional software is required from Stack Builder upon completion)
    - You may be prompted for a password for the postgres user and a port (good defaults are "postgres" and "5432", respectively). If you choose your own, you will have to update the connection string in appsettings.json

#### Mac
- Select a download option from [here](https://www.postgresql.org/download/macosx/) and download PostgreSQL 15
    - The [Postgres.app](https://postgresapp.com/) option works well
- Initialise a server with the following configuration (or update `PostgreSQLConnection` in `appsettings.json` to match your own):
    - Port: `5432`
    - User: `postgres`
    - Password: `postgres`

### Creating/updating the local database

- You can just run the website project and it will create and update the database on startup
- If you want to manually update the database (e.g. to test a new migration) in the terminal (from the solution directory) run `dotnet ef database update --project .\WhlgPortalWebsite`

### Adding Migrations

- In the terminal (from the solution directory) run `dotnet ef migrations add <YOUR_MIGRATION_NAME> --project .\WhlgPortalWebsite.Data --startup-project .\WhlgPortalWebsite`
- Then update the local database

### Reverting Migrations

You may want to revert a migration on your local database as part of a merge, or just because it's wrong and you need to fix it (only do this for migrations that haven't been merged to main yet)
- Run `dotnet ef database update <MIGRATION_BEFORE_YOURS> --project .\WhlgPortalWebsite` to rollback your local database
- Run `dotnet ef migrations remove --project .\WhlgPortalWebsite.Data --startup-project .\WhlgPortalWebsite` to delete the migration and undo the snapshot changes

#### Merging Migrations

We cannot merge branches both containing different migrations. We have marked the EF Core snapshot file as binary in git. This should mean that git throws up an error if we try to merge branches with different migrations
(because git will try to merge two sets of changes into the snapshot file and it can't merge changes in binary files).
The solution is unfortunately tedious. Given branch 1 with migration A and branch 2 with migration B:
- One branch is merged to main as normal (let's say branch 1)
- On branch 2
- Revert and remove migration B
- Merge main into branch 2
- Recreate migration B (which will now be on top of migration A)
- Merge branch 2 into main

## Environments

This app is deployed to BEIS AWS platform

### Configuration

Non-secret configuration is stored in the corresponding `appsettings.<environment>.json` file:
- appsettings.DEV.json
- appsettings.UAT.json
- appsettings.Production.json

Secrets must be configured in the ECS tasks, corresponding to the variables in `secrets.json` above:
- `ConnectionStrings__PostgreSQLConnection`
- `GovUkNotify__ApiKey`
- `Authentication__Cognito__ClientId`
- `Authentication__Cognito__ClientSecret`
- `Authentication__Cognito__MetadataAddress`
- `Authentication__Cognito__SignOutUrl`

The S3 configuration is also configured in ECS, as it's linked to AWS resources
- `S3__BucketName`
- `S3__Region`
