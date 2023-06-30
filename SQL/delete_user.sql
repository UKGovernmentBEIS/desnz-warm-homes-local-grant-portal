-- Remove permissions for user
DELETE FROM "LocalAuthorityUser"
WHERE "UsersId" IN (SELECT "Id" FROM "Users" WHERE "EmailAddress" = '<user email address>');

-- Remove last file download records for user (not audit logs!)
DELETE FROM "CsvFileDownload"
WHERE "UserId" IN (SELECT "Id" FROM "Users" WHERE "EmailAddress" = '<user email address>');

-- Remove user
DELETE FROM "Users"
WHERE "EmailAddress" = '<user email address>';
