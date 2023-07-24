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
