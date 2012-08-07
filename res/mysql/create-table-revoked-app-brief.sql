create table `RevokedAppBrief` (
  `Id` int(11) not null,
  `DeveloperId` int(11) not null,
  `DeveloperName` varchar(300) CHARACTER SET utf8 not null,
  `DeveloperViewUrl` varchar(500) CHARACTER SET utf8 not null,
  `Price` float not null,
  `Currency` varchar(20) CHARACTER SET utf8 not null,
  `Version` varchar(100) CHARACTER SET utf8 not null,
  `ReleaseDate` datetime not null,
  `Name` varchar(500) not null,
  `Introduction` varchar(300) not null,
  `ReleaseNotes` varchar(300) not null,
  `PrimaryCategory` int(11) not null,
  `ViewUrl` varchar(300) CHARACTER SET utf8 not null,
  `IconUrl` varchar(200) CHARACTER SET utf8 not null,
  `FileSize` int(11) not null,
  `AverageUserRatingForCurrentVersion` float DEFAULT NULL,
  `UserRatingCountForCurrentVersion` int(11) DEFAULT NULL,
  `SupportedDevices` varchar(200) CHARACTER SET utf8 not null,
  `Features` varchar(200) CHARACTER SET utf8 not null,
  `IsGameCenterEnabled` tinyint(1) not null,
  `DeviceType` int(11) not null,
  `LastValidUpdateTime` datetime not null,
  `LastValidUpdateType` int(11) not null,
  `LastValidUpdateOldValue` varchar(100) CHARACTER SET utf8 not null,
  `LastValidUpdateNewValue` varchar(100) CHARACTER SET utf8 not null,
  `LanguagePriority` int(11) not null,
  `RevokedTime` datetime not null,
  PRIMARY KEY (`Id`)
) engine=InnoDB default charset=utf8mb4;