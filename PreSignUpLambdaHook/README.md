# Pre-sign-up Lambda hook

This Lambda function is triggered whenever a user attempts to sign up to the service. It checks the given email
address against a list of recognised email addresses, stored in an AWS RDS PostgreSQL database.

The credentials for connecting to this database are passed in with environment variables:

* `PGHOST`: the host URL to connect to
* `PGPORT`: the port to connect to (usually `5432`)
* `PGUSER`: the username to use when connecting
* `PGPASSWORD`: the password to use when connecting
* `PGDATABASE`: the name of the database to connect to
