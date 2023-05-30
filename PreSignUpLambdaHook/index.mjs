import pg from 'pg';
import fs from 'fs/promises';

const checkEmailAddress = async (emailAddress) => {
    const client = new pg.Client({
        ssl: {
            ca: await fs.readFile('certs/eu-west-2-bundle.pem', 'utf-8'),
        },
    });

    await client.connect();
    const res = await client.query(
        'SELECT "EmailAddress" FROM "Users" WHERE LOWER("EmailAddress")=LOWER($1)',
        [emailAddress],
    );
    await client.end();

    if (res.rowCount === 0) {
        throw new Error('Email address not found');
    }
};

export const handler = async (event) => {
    await checkEmailAddress(event.request.userAttributes.email);
    return event;
};
