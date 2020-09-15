CREATE PROCEDURE uspRetrieveSocialMediaForSpeaker
(
	@SpeakerId NVARCHAR(128)
)
AS
BEGIN
	SELECT SocialMedia.SocialMediaProviderId,
				 SocialMedia.ProfileName,
				 SocialMediaProvider.BaseUrl
		FROM Speaker
	 INNER JOIN Contact ON Contact.Id = Speaker.ContactId
	 INNER JOIN ContactSocialMedia ON ContactSocialMedia.ContactId = Contact.Id
	 INNER JOIN SocialMedia ON SocialMedia.Id = ContactSocialMedia.SocialMediaId
	 INNER JOIN SocialMediaProvider ON SocialMediaProvider.Id = SocialMedia.SocialMediaProviderId
	 WHERE Speaker.AspNetUserId = @SpeakerId
		 AND SocialMedia.IsDeleted = 0
		 AND SocialMediaProviderId IN (1, 2, 3, 6, 9, 12, 13, 14)
END