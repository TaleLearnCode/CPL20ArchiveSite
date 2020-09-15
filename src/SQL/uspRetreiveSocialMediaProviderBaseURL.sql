CREATE PROCEDURE uspRetrieveSocialMediaProviderBaseURL
(
	@SocialMediaProviderId INT
)
AS
BEGIN
	SELECT BaseUrl
	  FROM SocialMediaProvider
	 WHERE Id = @SocialMediaProviderId
END