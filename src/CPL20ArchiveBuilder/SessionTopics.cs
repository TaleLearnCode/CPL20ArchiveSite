using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CPL20ArchiveBuilder
{
	public class SessionTopics : Collection<int>
	{

		public void GetSessionTopics(int sessionId, SqlConnection sqlConnection)
		{
			using SqlCommand sqlCommand = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.StoredProcedure,
				CommandText = "uspRetrieveSessionTopics"
			};
			sqlCommand.Parameters.AddWithValue("@SessionId", sessionId);
			using SqlDataReader reader = sqlCommand.ExecuteReader();
			if (reader.HasRows)
				while (reader.Read())
					this.Add(reader.GetInt32(0));
		}

		public string GetAsideLinks()
		{
			var returnValue = new StringBuilder();
			foreach (int sessionTopic in this)
			{
				switch (sessionTopic)
				{
					case 1:
						returnValue.Append("<a asp-page=\"/Sessions/Topic_AppDev\">Application Development</a><br />");
						break;
					case 2:
						returnValue.Append("<a asp-page=\"/Sessions/Topic_Infrastructure\">Infrastructure</a><br />");
						break;
					case 3:
						returnValue.Append("<a asp-page=\"/Sessions/Topic_ProjMgnt\">Project Management</a><br />");
						break;
					case 4:
						returnValue.Append("<a asp-page=\"/Sessions/Topic_Requirements\">Requirements</a><br />");
						break;
					case 5:
						returnValue.Append("<a asp-page=\"/Sessions/Topic_SoftSkills\">Soft Skills</a><br />");
						break;
					case 6:
						returnValue.Append("<a asp-page=\"Sessions/Topic_Testing\" > Software Testing</a><br />");
						break;
					case 7:
						returnValue.Append("<a asp-page=\"/Sessions/Topic_UX\">User Experience</a><br />");
						break;
				}
			}
			return returnValue.ToString();
		}


	}

}