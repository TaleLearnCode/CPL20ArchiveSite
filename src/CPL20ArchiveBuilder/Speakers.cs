using ShellProgressBar;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CPL20ArchiveBuilder
{
	public class Speakers : Dictionary<string, Speaker>
	{

		public static Speakers GetSpeakersForEvent(int eventId, SqlConnection sqlConnection)
		{
			var speakers = new Speakers();

			int numberOfSpeakers = GetNumberOfSpeakers();
			int counter = 0;
			using ProgressBar progressBar = new ProgressBar(numberOfSpeakers, "Retrieving Speakers...");

			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveSpeakersForEvent"
			};
			sqlCommand.Parameters.AddWithValue("@EventId", eventId);
			const int speakerIdIndex = 0; const int firstNameIndex = 1; const int lastNameIndex = 2; const int biographyIndex = 3;
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					var speaker = new Speaker()
					{
						Id = reader.GetString(speakerIdIndex),
						FirstName = reader.GetString(firstNameIndex),
						LastName = reader.GetString(lastNameIndex),
						Biography = reader.GetString(biographyIndex)
					};
					speaker.SocialMediaAccounts.AddRange(SocialMedia.GetSocialMediaForSpeaker(speaker.Id, sqlConnection));
					speaker.Websites.AddRange(Speaker.GetWebsitesForSpeaker(speaker.Id, sqlConnection));
					speakers.Add(speaker.Id, speaker);
					counter++;
					progressBar.Tick($"Retrieved {counter} of {numberOfSpeakers} speakers");
				}
			}
			return speakers;
		}

		private static int GetNumberOfSpeakers()
		{
			return 100;
		}

		public static List<string> GetSpeakerIdsForSession(int sessionId, SqlConnection sqlConnection)
		{
			var speakerIds = new List<string>();
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveSpeakerIdsForSession"
			};
			sqlCommand.Parameters.AddWithValue("@SessionId", sessionId);
			const int speakerIdIndex = 0;
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
				while (reader.Read())
					speakerIds.Add(reader.GetString(speakerIdIndex));
			return speakerIds;
		}


	}

}