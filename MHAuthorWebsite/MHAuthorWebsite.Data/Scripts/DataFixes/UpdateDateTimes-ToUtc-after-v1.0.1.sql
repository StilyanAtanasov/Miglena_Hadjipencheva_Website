USE [MHAuthorWebsite]
GO

/* 
--------------------------- NOTE: ---------------------------
-> Run this script for app versions later than v1.0.1 only!
-> Pay careful attention to the @SourceTimeZone and alter 
the variable to match your DB server location as well!
-------------------------------------------------------------
*/

DECLARE @SourceTimeZone sysname = N'FLE Standard Time'; /* !!! Adjust if needed !!! */
DECLARE @TargetTimeZone sysname = N'UTC';

BEGIN TRANSACTION;

BEGIN TRY

UPDATE
        [ContactRequests]
SET
        [RepliedOn] =
        [RepliedOn] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [RepliedOn] IS NOT NULL;

UPDATE
        [ScheduledNotifications]
SET
        [ScheduledAt] =
        [ScheduledAt] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [ScheduledAt] IS NOT NULL;

UPDATE
        [ScheduledNotifications]
SET
        [SentAt] =
        [SentAt] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [SentAt] IS NOT NULL;

UPDATE
        [ScheduledNotifications]
SET
        [ExpirationDate] =
        [ExpirationDate] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [ExpirationDate] IS NOT NULL;

UPDATE
        [ProductComments]
SET
        [LastEdited] =
        [LastEdited] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [LastEdited] IS NOT NULL;

UPDATE
        [ProductCommentsReactions]
SET
        [CreatedAt] =
        [CreatedAt] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [CreatedAt] IS NOT NULL;

UPDATE
        [ProductDiscounts]
SET
        [StartDate] =
        [StartDate] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [StartDate] IS NOT NULL;

UPDATE
        [ProductDiscounts]
SET
        [EndDate] =
        [EndDate] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [EndDate] IS NOT NULL;

UPDATE
        [AspNetUsers]
SET
        [RegisteredOn] =
        [RegisteredOn] AT TIME ZONE @SourceTimeZone AT TIME ZONE @TargetTimeZone
WHERE
        [RegisteredOn] IS NOT NULL;

COMMIT TRANSACTION;

END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
THROW;
END CATCH;
