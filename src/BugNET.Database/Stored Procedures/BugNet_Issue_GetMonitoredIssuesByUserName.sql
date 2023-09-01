CREATE PROCEDURE [dbo].[BugNet_Issue_GetMonitoredIssuesByUserName]
  @UserName NVARCHAR(255),
  @ExcludeClosedStatus BIT
AS

SET NOCOUNT ON

DECLARE @NotificationUserId  UNIQUEIDENTIFIER

EXEC dbo.BugNet_User_GetUserIdByUserName @UserName = @UserName, @UserId = @NotificationUserId OUTPUT

SELECT 
	iv.*
	, notifications.NotificationUserId
	, notifications.NotificationUserName
	, notifications.NotificationDisplayName 
FROM BugNet_IssuesView iv 
CROSS JOIN (select TOP (1) uv.UserId AS NotificationUserId
, uv.UserName AS NotificationUserName
, uv.DisplayName AS NotificationDisplayName 
FROM BugNet_UserView uv
LEFT JOIN BugNet_IssueNotifications bin ON bin.UserId = uv.UserId 
LEFT JOIN BugNet_ProjectNotifications pin ON pin.UserId = uv.UserId
WHERE uv.UserId = @NotificationUserId) AS notifications
WHERE iv.[Disabled] = 0 AND iv.ProjectDisabled = 0 
AND ((@ExcludeClosedStatus = 0) OR (iv.IsClosed = 0)) 
