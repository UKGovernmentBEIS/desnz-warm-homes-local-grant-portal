# Pre-sign-up Lambda hook

This Lambda function is configured as a 'pre sign-up lambda trigger',
and is executed whenever a user attempts to sign up to the service.

It checks the given email address against the list of users stored in the portal's PostgreSQL database.

## Configuration

The credentials for connecting to this database are passed in with environment variables:

* `PGHOST`: the host URL to connect to
* `PGPORT`: the port to connect to (usually `5432`)
* `PGUSER`: the username to use when connecting
* `PGPASSWORD`: the password to use when connecting
* `PGDATABASE`: the name of the database to connect to

## Deployment

As we do not expect frequent changes, the lambda code is captured in the Terraform module,
rather than being deployed through a dedicated pipeline.

This means any changes need to be co-ordinated through the platform team.

## Adding users

New users need to be added to the database manually and linked to at least one Local Authority.

Access to the database is via AWS Systems Manager (SSM) in the relevant AWS environment:
- Find the database credentials in the SSM Parameter Store under `PostgreSQLConnectionhug2-rds-prs`
- Begin a new session on the jump host under SSM Session Manager
- Use the `psql` command line tool to connect to the DB: `psql -h <database host> -d <database name> -U <username>`
- Enter the password when prompted - you can still paste it here but avoid putting it in the history as this will be stored
- Run any SQL commands you need - e.g. `SELECT * FROM "Users";` to display all users

### Adding a single user

The script `SQL/add_users.sql` contains SQL for inserting users and granting them permission to access the data of specific local authorities. First, replace `<user email address>` with the email address of the user you wish to add, then replace `<local authority custodian code>` with the custodian code of the local authority they should be given access to, and run the file. This can be difficult to do against an AWS database, so you may have to execute the lines manually through `psql` as described above.

Note that each user needs access to at least one local authority, otherwise they will see an error after logging in.

### Adding multiple users

To add multiple users and give them permissions at the same time, use the commented-out lines in the above SQL script. Un-comment these lines, and duplicate them as many times as is needed to add all new users and permissions.
