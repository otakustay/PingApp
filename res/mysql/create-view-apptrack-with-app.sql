create view AppTrackWithApp as
    select
        `AppTrack`.`Id` `Id`,
        `AppTrack`.`User` `User`,
        `AppTrack`.`App` `App`,
        `AppTrack`.`Status` `Status`,
        `AppTrack`.`CreateTime` `CreateTime`,
        `AppTrack`.`CreatePrice` `CreatePrice`,
        `AppTrack`.`BuyTime` `BuyTime`,
        `AppTrack`.`BuyPrice` `BuyPrice`,
        `AppTrack`.`Rate` `Rate`,
        `AppTrack`.`HasRead` `HasRead`,
        `AppBrief`.`Id` `App.Id`,
        `AppBrief`.`DeveloperId` `App.DeveloperId`,
        `AppBrief`.`DeveloperName` `App.DeveloperName`,
        `AppBrief`.`DeveloperViewUrl` `App.DeveloperViewUrl`,
        `AppBrief`.`Price` `App.Price`,
        `AppBrief`.`Currency` `App.Currency`,
        `AppBrief`.`Version` `App.Version`,
        `AppBrief`.`ReleaseDate` `App.ReleaseDate`,
        `AppBrief`.`Name` `App.Name`,
        `AppBrief`.`Introduction` `App.Introduction`,
        `AppBrief`.`ReleaseNotes` `App.ReleaseNotes`,
        `AppBrief`.`PrimaryCategory` `App.PrimaryCategory`,
        `AppBrief`.`ViewUrl` `App.ViewUrl`,
        `AppBrief`.`IconUrl` `App.IconUrl`,
        `AppBrief`.`FileSize` `App.FileSize`,
        `AppBrief`.`AverageUserRatingForCurrentVersion` `App.AverageUserRatingForCurrentVersion`,
        `AppBrief`.`UserRatingCountForCurrentVersion` `App.UserRatingCountForCurrentVersion`,
        `AppBrief`.`SupportedDevices` `App.SupportedDevices`,
        `AppBrief`.`Features` `App.Features`,
        `AppBrief`.`IsGameCenterEnabled` `App.IsGameCenterEnabled`,
        `AppBrief`.`DeviceType` `App.DeviceType`,
        `AppBrief`.`LastValidUpdateTime` `App.LastValidUpdateTime`,
        `AppBrief`.`LastValidUpdateType` `App.LastValidUpdateType`,
        `AppBrief`.`LastValidUpdateOldValue` `App.LastValidUpdateOldValue`,
        `AppBrief`.`LastValidUpdateNewValue` `App.LastValidUpdateNewValue`,
        `AppBrief`.`LanguagePriority` `App.LanguagePriority`,
        `AppBrief`.`Hash` `App.Hash`,
        `AppBrief`.`IsValid` `App.IsValid`
    from `AppTrack`
    inner join `AppBrief`
        on `AppBrief`.`Id` = `AppTrack`.`App`;