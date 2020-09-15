CREATE PROCEDURE dbo.uspRetrieveTags
AS
BEGIN
	SELECT Id, [Name] FROM Technology
END