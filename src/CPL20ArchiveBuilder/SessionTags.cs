using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CPL20ArchiveBuilder
{
	public class SessionTags : Collection<int>
	{

		public void GetSessionTags(int sessionId, SqlConnection sqlConnection)
		{
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveSessionTags"
			};
			sqlCommand.Parameters.AddWithValue("@SessionId", sessionId);
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
				while (reader.Read())
					this.Add(reader.GetInt32(0));
		}

		public string GetAsideLinks(Dictionary<int, string> sessionTags)
		{
			var returnValue = new StringBuilder();
			foreach (int sessionTag in this)
				returnValue.Append($"<a href=\"/SessionsByTag/{sessionTag}\">{sessionTags[sessionTag]}</a><br />");
			return returnValue.ToString();
		}

		public static Dictionary<int, string> GetTags(SqlConnection sqlConnection)
		{
			var tags = new Dictionary<int, string>();
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveTags"
			};
			const int idIndex = 0; const int nameIndex = 1;
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
				while (reader.Read())
					tags.Add(reader.GetInt32(idIndex), reader.GetString(nameIndex));
			return tags;
		}

	}

}