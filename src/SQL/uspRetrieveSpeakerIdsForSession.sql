CREATE PROCEDURE dbo.uspRetrieveSpeakerIdsForSession
(
	@SessionId INT
)
AS
BEGIN
	SELECT SpeakerId
	  FROM SessionSpeaker
	 WHERE SessionId = @SessionId
END