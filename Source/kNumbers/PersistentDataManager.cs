using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;


namespace kNumbers
{
	[StaticConstructorOnStartup]
	public static class PersistentDataManager
	{
		private const string folderName = "Numbers";
		private const string settingsFileBaseName = "NumbersConfig";
		private const string settingsFileExtName = "xml";
		private static readonly string folderPath = Path.Combine(GenFilePaths.SaveDataFolderPath, folderName);
		private static readonly string fileName = string.Join(".", new[] { settingsFileBaseName, settingsFileExtName });
		private const string settingsNodeName = "settings";

		private static string GetFileFullPath(string path)
		{
			return Path.GetFullPath(path);
		}
		static PersistentDataManager()
		{
			//Log.Message(Path.Combine(folderPath, fileName));
		}

		private static void loadsave<T>(ref T data)
		{
			Scribe_Deep.LookDeep(ref data, settingsNodeName, new object[0]);
		}

		public static void LoadTo<T>(ref T data)
		{
			var fullPath = GetFileFullPath(Path.Combine(folderPath, fileName));
			if (!File.Exists(fullPath)) {
				return;
			}
			try {
				Scribe.InitLoading(fullPath);
				loadsave(ref data);
			} finally {
				Scribe.FinalizeLoading();
			}
		}

		public static void SaveFrom<T>(ref T data)
		{
			Directory.CreateDirectory(folderPath);
			var fullPath = GetFileFullPath(Path.Combine(folderPath, fileName));
			try {
				Scribe.InitWriting(fullPath, folderName);
				loadsave(ref data);
			} finally {
				Scribe.FinalizeWriting();
			}
		}
	}
}
