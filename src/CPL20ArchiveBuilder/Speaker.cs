using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CPL20ArchiveBuilder
{
	public class Speaker
	{

		public string Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Name { get { return $"{FirstName} {LastName}"; } }

		public string Biography { get; set; }

		public List<SocialMedia> SocialMediaAccounts { get; } = new List<SocialMedia>();

		public List<string> Websites { get; } = new List<string>();

		public static List<string> GetWebsitesForSpeaker(string speakerId, SqlConnection sqlConnection)
		{
			var returnValue = new List<string>();
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveWebsitesForSpeaker"
			};
			sqlCommand.Parameters.AddWithValue("@SpeakerId", speakerId);
			const int websiteIndex = 0;
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					returnValue.Add(reader.GetString(websiteIndex));
				}
			}
			return returnValue;
		}


	}

}