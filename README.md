# Warm Homes: Local Grant Portal BETA

This repository was cloned
from [HUG2 Portal](https://github.com/UKGovernmentBEIS/desnz-home-energy-retrofit-portal-beta) in December 2024, keeping
all previous commits.

The code was then adapted into the WH:LG service.

Note, the WH:LG project is split across 2 repositories:

- The [desnz-warm-homes-local-grant repository](https://github.com/UKGovernmentBEIS/desnz-warm-homes-local-grant), which
  runs the user site responsible for generating WH:LG referrals.
- This one, which runs the admin site responsible for viewing these referrals.

The project setup is almost identical to WH:LG main. To avoid duplication, refer to the main README for setup and the following differences:
- The database name is `whlgportaldev` instead of `whlgdev`.
- The work folder is `WhlgPortalWebsite` instead of `WhlgPublicWebsite`

Run through the WH:LG main setup instructions and set up WH:LG main before setting up this repository.

# API keys secrets

Use thw following template for `WhlgPortalWebsite` user secrets:

```json
{
  "Authentication": {
    "Cognito": {
      "ClientId": "<app client id from AWS Cognito>",
      "ClientSecret": "<app client secret from AWS Cognito>",
      "MetadataAddress": "https://cognito-idp.{your-region-id}.amazonaws.com/{your-user-pool-id}/.well-known/openid-configuration",
      "SignOutUrl": "{cognito-domain}/logout?client_id={client-id}&logout_uri=http://localhost:5000/portal/sign-out"
    }
  },
  "GovUkNotify": {
    "ApiKey": "<REAL_VALUE_HERE>"
  }
}
```

# Configuring user access

Note that when running locally we still need to connect to an AWS Cognito user pool to do the sign in flow.

There is no local equivalent for Cognito so we connect to the AWS Cognito Development environment.
This means that we can reuse the login credentials on dev.

The Keeper contains 'WH:LG Dev Cognito Login' record for a user account for WH:LG dev environment.
If you need a different account for local development, you must:

1. Complete https://softwiretech.atlassian.net/wiki/spaces/Support/pages/21607743666/DESNZ+Support+Connecting+to+AWS#3.-Accessing-a-container-terminal for WH:LG Development.
2. Run the AddLas for LA 5060.
   - Use the email you plan to use to sign up to the local referrals site on.
   - LA 5060 doesn't need to be the same LAs you want to view referrals for.
   - The reason this step is necessary is that the local sign up & sign in is linked to the dev environment AWS
     Cognito. There is a check in place that won't allow any user emails not in the dev environment Users table to sign
     up.
3. Run the following management shell scripts locally - see [here](Documentation/management-shell-scripts.md) for how to
   do this:
    1. `AddAllMissingAuthoritiesToDatabase`
    2. `AddLas` using your login email and the custodian codes of LAs you want to view referrals for
    3. `AddConsortia` using your login email and the consortium codes of Consortia you want to view referrals for
4. When running the local site, create an account using the email you used in step 2.
 
# CLI commands

See [here](Documentation/management-shell-scripts.md).

# Local URL

In a browser, visit http://localhost:5000/portal (the `/portal` is important!)

# Branches

3 branches correspond to each of the environments:

- `develop` - [Dev](https://dev.apply-warm-homes-local-grant.service.gov.uk/portal)
- `staging` - [UAT](https://uat.apply-warm-homes-local-grant.service.gov.uk/portal)
- `main` - [Production](https://www.apply-warm-homes-local-grant.service.gov.uk/portal)

# Deployment secrets

The ECS tasks use the following secrets:

- `ConnectionStrings__PostgreSQLConnection`
- `GovUkNotify__ApiKey`
- `Authentication__Cognito__ClientId`
- `Authentication__Cognito__ClientSecret`
- `Authentication__Cognito__MetadataAddress`
- `Authentication__Cognito__SignOutUrl`

The S3 configuration is also configured in ECS, as it's linked to AWS resources

- `S3__BucketName`
- `S3__Region`
