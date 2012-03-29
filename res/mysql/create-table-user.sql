create table `User` (
  `Id` int(11) not null auto_increment,
  `Email` varchar(200) not null,
  `Username` varchar(50) not null,
  `Password` varchar(40) not null,
  `Description` varchar(500) not null,
  `Website` varchar(200) not null,
  `NotifyOnWishPriceDrop` tinyint(1) not null,
  `NotifyOnWishFree` tinyint(1) not null,
  `NotifyOnWishUpdate` tinyint(1) not null,
  `NotifyOnOwnedUpdate` tinyint(1) not null,
  `ReceiveSiteUpdates` tinyint(1) not null,
  `PreferredLanguagePriority` int(11) not null,
  `Status` int(11) not null,
  `RegisterTime` datetime not null,
  PRIMARY KEY (`Id`),
  KEY `User_Email` (`Email`),
  KEY `User_Username` (`Username`)
) engine=InnoDB default charset=utf8;