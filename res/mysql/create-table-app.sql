create table `App` (
  `Id` int(11) not null,
  `CensoredName` varchar(500) not null,
  `Description` varchar(4200) not null,
  `LargeIconUrl` varchar(200) charater set utf8 not null,
  `SellerName` varchar(200) charater set utf8 not null,
  `SellerViewUrl` varchar(300) charater set utf8 DEFAULT NULL,
  `ReleaseNotes` varchar(4200) not null,
  `ContentAdvisoryRating` varchar(40) charater set utf8 not null,
  `ContentRating` varchar(20) charater set utf8 not null,
  `AverageUserRating` float DEFAULT NULL,
  `UserRatingCount` int(11) DEFAULT NULL,
  `Languages` varchar(600) charater set utf8 not null,
  `Categories` varchar(200) charater set utf8 not null,
  `ScreenshotUrls` varchar(1600) charater set utf8 not null,
  `IPadScreenshotUrls` varchar(1600) charater set utf8 not null,
  primary key (`Id`)
) engine=InnoDB default charset=utf8mb4;