CREATE PROCEDURE dbo.uspRetrieveTags
AS
BEGIN
  SELECT DISTINCT Technology.Id, Technology.Name, Technology.NormalizedName, Technology.SortOrder
    FROM Session
   INNER JOIN SessionTechnology ON SessionTechnology.SessionId = Session.Id
   INNER JOIN Technology ON Technology.Id = SessionTechnology.TechnologyId
   WHERE Session.EventId = 10
     AND Session.SessionStatusId = 6
   ORDER BY Technology.SortOrder
END