CREATE PROCEDURE dbo.uspRetrieveSessionTags
(
	@SessionId INT
)
AS
BEGIN
	SELECT TechnologyId FROM SessionTechnology WHERE SessionId = @SessionId
END