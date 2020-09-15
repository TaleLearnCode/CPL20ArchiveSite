CREATE PROCEDURE dbo.uspRetrieveSpeakersForEvent
(
	@EventId INT
)
AS
BEGIN
	SELECT DISTINCT Speaker.AspNetUserId,
	       Contact.FirstName,
				 Contact.LastName,
				 Speaker.Biography
		FROM Speaker
	 INNER JOIN Contact ON Contact.Id = Speaker.ContactId
	 INNER JOIN SessionSpeaker ON SessionSpeaker.SpeakerId = Speaker.AspNetUserId
	 INNER JOIN [Session] ON [Session].Id = SessionSpeaker.SessionId
	 WHERE [Session].EventId = @EventId
	   AND [Session].SessionStatusId = 6
	 ORDER BY Contact.LastName, Contact.FirstName
END