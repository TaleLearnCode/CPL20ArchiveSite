using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CPL20ArchiveBuilder
{
	public class SocialMedia
	{

		public SocialMediaProvider SocialMediaProvider { get; set; }

		public Uri Link { get; set; }

		public static List<SocialMedia> GetSocialMediaForSpeaker(string speakerId, SqlConnection sqlConnection)
		{
			var socialMediaAccounts = new List<SocialMedia>();
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveSocialMediaForSpeaker"
			};
			sqlCommand.Parameters.AddWithValue("@SpeakerId", speakerId);
			const int socialMediaProviderIdIndex = 0; const int profileNameIndex = 1; const int baseUrlIndex = 2;
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			{
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						socialMediaAccounts.Add(new SocialMedia()
						{
							SocialMediaProvider = (SocialMediaProvider)reader.GetInt32(socialMediaProviderIdIndex),
							Link = new Uri(string.Format(reader.GetString(baseUrlIndex), reader.GetString(profileNameIndex)))
						});
					}
				}
			}
			return socialMediaAccounts;
		}


	}

}