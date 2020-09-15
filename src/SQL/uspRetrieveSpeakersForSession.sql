CREATE PROCEDURE dbo.uspRetrieveSpeakersForSession
(
	@SessionId INT
)
AS
BEGIN
	SELECT DISTINCT Speaker.AspNetUserId,
	       Contact.FirstName,
				 Contact.LastName,
				 Speaker.Biography
		FROM Speaker
	 INNER JOIN Contact ON Contact.Id = Speaker.ContactId
	 WHERE Speaker.AspNetUserId IN (SELECT SpeakerId
	                                 FROM SessionSpeaker
																	WHERE SessionId = @SessionId)
END