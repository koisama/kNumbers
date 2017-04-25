namespace kNumbers
{
	public static class Log
	{
		public static void Message(string text)
		{
			Verse.Log.Message("[Numbers] " + text);
		}
		public static void Warning(string text)
		{
			Verse.Log.Warning("[Numbers] " + text);
		}
		public static void Error(string text)
		{
			Verse.Log.Error("[Numbers] " + text);
		}
	}
}
