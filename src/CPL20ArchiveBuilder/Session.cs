using ShellProgressBar;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CPL20ArchiveBuilder
{
	public class Session
	{

		public int Id { get; private set; }

		public string Title { get; private set; }

		public string Abstract { get; private set; }

		public string Summary { get; private set; }

		public string SessionType { get; private set; }

		public string SessionLevel { get; private set; }

		public int SessionPeriodId { get; set; }

		public bool VideoUploaded { get; set; }

		public string YouTubeId { get; set; }

		public SessionTopics Topics { get; } = new SessionTopics();

		public SessionTags Tags { get; } = new SessionTags();

		public List<Speaker> SessionSpeakers { get; } = new List<Speaker>();

		public static Dictionary<int, Session> GetSessionForEvent(int eventId, SqlConnection sqlConnection, Speakers eventSpeakers)
		{
			int totalSessionCount = GetNumberOfSessions();
			int counter = 0;
			using ProgressBar progressBar = new ProgressBar(totalSessionCount, "Retrieving session");
			var sessions = new Dictionary<int, Session>();
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveSessions"
			};
			sqlCommand.Parameters.AddWithValue("@EventId", eventId);
			const int sessionIdIndex = 0;
			const int titleIndex = 1;
			const int abstractIndex = 2;
			const int summaryIndex = 3;
			const int sessionPeriodIdIndex = 4;
			const int youTubeIdIndex = 5;
			const int sessionLevelIndex = 6;
			const int sessionTypeIndex = 7;
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					var session = new Session()
					{
						Id = reader.GetInt32(sessionIdIndex),
						Title = reader.GetString(titleIndex),
						Abstract = reader.GetString(abstractIndex),
						Summary = reader.GetString(summaryIndex),
						YouTubeId = reader.IsDBNull(youTubeIdIndex) ? string.Empty : reader.GetString(youTubeIdIndex),
						SessionLevel = reader.GetString(sessionLevelIndex),
						SessionType = reader.GetString(sessionTypeIndex),
						SessionPeriodId = reader.GetInt32(sessionPeriodIdIndex)
					};
					if (session.SessionPeriodId == 106 || session.SessionPeriodId == 107 || session.SessionPeriodId == 120)
						session.SessionPeriodId = 105;
					foreach (var speakerId in Speakers.GetSpeakerIdsForSession(session.Id, sqlConnection))
						session.SessionSpeakers.Add(eventSpeakers[speakerId]);
					session.Topics.GetSessionTopics(reader.GetInt32(sessionIdIndex), sqlConnection);
					session.Tags.GetSessionTags(reader.GetInt32(sessionIdIndex), sqlConnection);
					sessions.Add(session.Id, session);
					counter++;
					progressBar.Tick($"Retrieved {counter} of {totalSessionCount} sessions");
				}
			}
			return sessions;
		}

		public static int GetNumberOfSessions()
		{
			return 140;
		}

	}

}
