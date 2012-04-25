use pingapp;

db.apps.ensureIndex(
    {
        "brief.deviceType": 1,
        "brief.primaryCategory": 1,
        "brief.price": 1,
        "brief.lastValidUpdate.type": 1,
        "brief.lastValidUpdate.time": -1
    },
    { "name": "_complex_" }
);

db.appUpdates.ensureIndex(
    { "app": 1, "time": -1 },
    { "name": "_app_time_" }
);

db.appTracks.ensureIndex(
    { "user": 1, "status": 1 },
    { "name": "_user_status_" }
);
db.appTracks.ensureIndex(
    { "app": 1 },
    { "name": "_app_" }
);

db.users.ensureIndex(
    { "email": 1 },
    { "name": "_email_" }
);
db.users.ensureIndex(
    { "username": 1 },
    { "name": "_username_" }
);

show collections;