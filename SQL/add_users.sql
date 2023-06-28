-- Create User records
INSERT INTO "Users" ("EmailAddress", "HasLoggedIn") VALUES
  ('<user email address>', FALSE)
  -- , ('<user email address>', FALSE)
ON CONFLICT DO NOTHING;

-- Permissions for users
INSERT INTO "LocalAuthorityUser" ("LocalAuthoritiesId", "UsersId")
SELECT "LocalAuthorities"."Id", "Users"."Id"
FROM "LocalAuthorities" CROSS JOIN "Users"
WHERE
    ("Users"."EmailAddress" = '<user email address>' AND "LocalAuthorities"."CustodianCode" = '<local authority custodian code>')
    -- OR ("Users"."EmailAddress" = '<user email address>' AND "LocalAuthorities"."CustodianCode" = '<local authority custodian code>')
ON CONFLICT DO NOTHING;
