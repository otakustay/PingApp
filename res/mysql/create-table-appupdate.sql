create table `AppUpdate` (
  `Id` int(11) not null auto_increment,
  `App` int(11) not null,
  `Time` datetime not null,
  `Type` int(11) not null,
  `OldValue` varchar(100) CHARACTER SET utf8 not null,
  `NewValue` varchar(100) CHARACTER SET utf8 not null,
  PRIMARY KEY (`Id`),
  KEY `AppUpdate_App` (`App`),
  KEY `AppUpdate_Time` (`Time`)
) engine=InnoDB default charset=utf8mb4;