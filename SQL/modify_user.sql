-- Remove LA permission for user
DELETE FROM "LocalAuthorityUser"
WHERE "UsersId" IN (SELECT "Id" FROM "Users" WHERE "EmailAddress" = '<user email address>')
  AND "LocalAuthoritiesId" IN (SELECT "Id" FROM "LocalAuthorities" WHERE "CustodianCode" = '<local authority custodian code>');

-- Add LA permission for user
INSERT INTO "LocalAuthorityUser" ("LocalAuthoritiesId", "UsersId")
SELECT "LocalAuthorities"."Id", "Users"."Id"
FROM "LocalAuthorities" CROSS JOIN "Users"
WHERE
    ("Users"."EmailAddress" = '<user email address>' AND "LocalAuthorities"."CustodianCode" = '<local authority custodian code>')
ON CONFLICT DO NOTHING;
