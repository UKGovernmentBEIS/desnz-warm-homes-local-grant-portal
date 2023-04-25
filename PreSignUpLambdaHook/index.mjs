export const handler = async (event, context, callback) => {
    const allowedEmailAddresses = [
        "test1@example.com"
    ];

    // case-insensitive string comparison, see
    // https://stackoverflow.com/questions/2140627/how-to-do-case-insensitive-string-comparison
    if (!allowedEmailAddresses.some(
        ea => ea.localeCompare(
            event.request.userAttributes.email,
            "en-GB",
            { sensitivity: "accent" }
        ) === 0)
    ) {
        var error = new Error("Email address not allowed");
        callback(error, event);
    }

    callback(null, event);
};
