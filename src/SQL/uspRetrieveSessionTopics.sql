CREATE PROCEDURE dbo.uspRetrieveSessionTopics
(
	@SessionId INT
)
AS
BEGIN
	SELECT TopicId FROM SessionTopic WHERE SessionId = @SessionId
END