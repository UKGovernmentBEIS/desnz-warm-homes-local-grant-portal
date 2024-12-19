# Management Shell Scripts

There are a number of useful scripts that can be run using `WhlgPortalWebsite.ManagementShell`.

The easiest way to run these is by using Rider run configurations. To do this:
- Select the drop-down icon to the left of the play icon in the top right.
- Select 'Edit configurations'
- Select '+' in the top-left -> .Net Project
- Update Project to `WhlgPortalWebsite.ManagementShell`
- Add the name of the script you want (see below) to run followed by any relevant arguments in program arguments.
- Select 'OK' in the bottom right.
- You can now select and run this script.

## List of scripts

- `AddLas [email address of user to be added] [custodian codes of LAs to be associated with user]` - Associate LAs with user and create user if they don't exist.
- `RemoveLas [email address of user for LAs to be removed] [custodian codes of LAs to be removed from user]` - Remove LAs from user.
- `RemoveUser [email address of user to be removed]` - Remove user.
- `AddConsortia [email address of user to be added] [consortium codes of Consortia to be associated with user]` - Associate consortia with user and create user if they don't exist.
- `RemoveConsortia [email address of user for Consortia to be removed] [consortiumn codes of LAs to be removed from user]` - Remove consortia from user.
- `FixAllUserOwnedConsortia` - This script will ensure the validity of the LA / Consortium relationship for users
- `AddAllMissingAuthoritiesToDatabase` - This script will ensure the database has an entry for every Local Authority & Consortium present in LocalAuthorityData or ConsortiumData. Use after adding a new Local Authority or Consortium to the code.