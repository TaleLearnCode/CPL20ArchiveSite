CREATE PROCEDURE dbo.uspRetrieveSession
(
	@SessionId INT
)
AS
BEGIN
	SELECT [Session].Id,
	       [Session].Title,
				 [Session].Abstract,
				 SessionLevel.Name,
				 SessionType.Name
	  FROM [Session]
	 INNER JOIN SessionLevel ON SessionLevel.Id = [Session].SessionLevelId
	 INNER JOIN SessionType  ON SessionType.Id = [Session].SessionTypeId
	 WHERE [Session].Id = @SessionId
	   AND [Session].SessionStatusId = 6
		 AND [Session].IsDeleted = 0
END