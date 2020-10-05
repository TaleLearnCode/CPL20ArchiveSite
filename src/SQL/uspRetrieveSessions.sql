CREATE PROCEDURE dbo.uspRetrieveSessions
(
	@EventId INT
)
AS
BEGIN
	SELECT [Session].Id,
	       [Session].Title,
				 [Session].Abstract,
				 [Session].Summary,
				 [Session].SessionPeriodId,
				 [Session].YouTubeId,
				 SessionLevel.Name,
				 SessionType.Name
	  FROM [Session]
	 INNER JOIN SessionLevel ON SessionLevel.Id = [Session].SessionLevelId
	 INNER JOIN SessionType  ON SessionType.Id = [Session].SessionTypeId
	 WHERE [Session].EventId = @EventId
	   AND [Session].SessionStatusId = 6
		 AND [Session].IsDeleted = 0
END