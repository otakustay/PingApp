create table `AppTrack` (
  `Id` char(36) not null,
  `User` int(11) not null,
  `App` int(11) not null,
  `Status` int(11) not null,
  `CreateTime` datetime not null,
  `CreatePrice` float not null,
  `BuyTime` datetime DEFAULT NULL,
  `BuyPrice` float DEFAULT NULL,
  `Rate` int(11) not null,
  `HasRead` tinyint(1) not null,
  PRIMARY KEY (`Id`),
  KEY `AppTrack_User` (`User`),
  KEY `AppTrack_App` (`App`),
  KEY `AppTrack_CreaateTime` (`CreateTime`),
  KEY `AppTrack_BuyTime` (`BuyTime`)
) engine=InnoDB default charset=utf8mb4;