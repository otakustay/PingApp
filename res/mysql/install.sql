/* 
 * create user PingApp identified by 'password'
 * mysql -uPingApp -p
 */

create database `PingApp` default character set utf8mb4;

use `PingApp`;

source create-table-app-brief.sql;
source create-table-app.sql;
source create-table-revoked-app-brief.sql;
source create-table-revoked-app.sql;
source create-table-app-update.sql;
source create-table-app-track.sql;
source create-table-user.sql;

source create-view-app-with-brief.sql;
source create-view-app-track-with-app.sql;

show tables;