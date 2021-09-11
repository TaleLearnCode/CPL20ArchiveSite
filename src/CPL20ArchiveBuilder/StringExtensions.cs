namespace CPL20ArchiveBuilder
{

	public static class StringExtensions
	{

		public static string Cleanup(this string input)
		{
			return input.Replace("\"", "").Replace(" ", "_").Trim();
		}
	}

}