import pg from "pg";
import fs from "fs";

export const handler = async (event, context, callback) => {
    const client = new pg.Client({
        ssl: {
            rejectUnauthorized: false,
            ca: fs.readFileSync("certs/eu-west-2-bundle.pem").toString(),
        },
    });
    await client.connect();
    const res = await client.query("SELECT email_address FROM local_authorities;");

    const allowedEmailAddresses = res.rows.map(row => row.email_address);

    // case-insensitive string comparison, see
    // https://stackoverflow.com/questions/2140627/how-to-do-case-insensitive-string-comparison
    if (!allowedEmailAddresses.some(
        ea => {
            console.debug(`Comparing "${event.request.userAttributes.email}" with "${ea}"`);
            const comparison = ea.localeCompare(
                event.request.userAttributes.email,
                "en-GB",
                { sensitivity: "accent" }
            );
            
            console.debug(comparison === 0 ? "  Equal!" : "  Not equal.");
            
            return comparison === 0;
        })
    ) {
        var error = new Error("Email address not allowed");
        callback(error, event);
    }

    callback(null, event);

    await client.end();
};
