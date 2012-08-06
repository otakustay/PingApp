/* 
 * create user PingApp identified by 'password'
 * mysql -uPingApp -p
 */

create database `PingApp` default character set utf8mb4;

use `PingApp`;

source create-table-appbrief.sql;
source create-table-app.sql;
source create-table-appupdate.sql;
source create-table-user.sql;
source create-table-apptrack.sql;

source create-view-app-with-brief.sql;
source create-view-apptrack-with-app.sql;

show tables;