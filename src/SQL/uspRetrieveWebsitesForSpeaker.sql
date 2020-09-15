CREATE PROCEDURE uspRetrieveWebsitesForSpeaker
(
	@SpeakerId NVARCHAR(128)
)
AS
BEGIN
	SELECT Website.Website
	  FROM Speaker
	 INNER JOIN Contact ON Contact.Id = Speaker.ContactId
	 INNER JOIN ContactWebsite ON ContactWebsite.ContactId = Contact.Id
	 INNER JOIN Website ON Website.Id = ContactWebsite.WebsiteId
	 WHERE Speaker.AspNetUserId = @SpeakerId
END