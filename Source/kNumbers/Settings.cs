using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

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
	public class Layout : Settings
	{
		public string name;
		public MainTabWindow_Numbers.pawnType chosenPawnType = MainTabWindow_Numbers.pawnType.Colonists;
		public Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>> savedKLists = new Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>>();
		private List<KListObject> tmpKList = new List<KListObject>();

		public override void ExposeData()
		{

#if DEBUG
			var msg = new StringBuilder();
			switch (Scribe.mode) {
				case LoadSaveMode.Inactive:
					msg.AppendLine($"Scribe.mode: Inactive");
					break;
				case LoadSaveMode.Saving:
					msg.AppendLine($"Scribe.mode: Saving");
					msg.AppendLine($"\t'{name}'\n\t\tchosenPawnType: {Enum.GetName(typeof(MainTabWindow_Numbers.pawnType), chosenPawnType)}\n\t\tsavedKLists: {string.Join(", ", savedKLists.Values.Select(x => x.Count.ToString()).ToArray())}");
					break;
				case LoadSaveMode.LoadingVars:
					msg.AppendLine($"Scribe.mode: LoadingVars");

					break;
				case LoadSaveMode.ResolvingCrossRefs:
					msg.AppendLine($"Scribe.mode: ResolvingCrossRefs");
					break;
				case LoadSaveMode.PostLoadInit:
					msg.AppendLine($"Scribe.mode: PostLoadInit");
					break;
				default:
					break;
			}
			Log.Message(msg.ToString());
#endif
			if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars) {
				Scribe_Values.LookValue(ref name, "layoutName");
				Scribe_Values.LookValue(ref chosenPawnType, "chosenPawnType");

				foreach (MainTabWindow_Numbers.pawnType type in Enum.GetValues(typeof(MainTabWindow_Numbers.pawnType))) {
					if (savedKLists.TryGetValue(type, out tmpKList)) {
						savedKLists.Remove(type);
					};
					Scribe_Collections.LookList(ref tmpKList, "klist-" + type, LookMode.Deep);
					savedKLists.Add(type, tmpKList);
				}
			}
		}
	}
	public class Settings : IExposable
	{

		public virtual void ExposeData()
		{
			#region BeforeSaving
			if (Scribe.mode == LoadSaveMode.Saving) {
				Layouts = new List<Layout>(layoutDict.Values);
			}
			#endregion

			if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars) {
				Scribe_Values.LookValue(ref activity, "activity");
				Scribe_Collections.LookList(ref Layouts, "layouts", LookMode.Deep);
			}

			#region AfterLoading
			if (Scribe.mode == LoadSaveMode.LoadingVars) {
#if DEBUG
				{
					var strings = Layouts.Select(l => l.name).ToArray();
					if (strings.NullOrEmpty()) {
						strings = new[] { "null" };
					}
					Log.Message($"Settings Loading. activity: '{activity}', Layouts: {string.Join(", ", strings)}");
				}
#endif
				if (Layouts == null || Layouts.Count == 0 || (Layouts[0]?.name).NullOrEmpty()) {
					Log.Error("invalid settigns.");
					Layouts = null;
					return;
				}
				if (activity.NullOrEmpty()) {
					activity = Layouts.First()?.name;
				}
				ActivityLayout = Layouts.Where(l => {
					var name = l.name;
					if (layoutDict.TryGetValue(name, out Layout layout)) {
						layoutDict.Remove(name);
					}
					layoutDict.Add(name, l);
					return name == activity;
				}).SingleOrDefault();
			}
			#endregion
		}

		private static Dictionary<string, Layout> layoutDict = new Dictionary<string, Layout>();
		public List<Layout> Layouts;

		private static string activity;
		public static Layout ActivityLayout
		{
			get {
				if (activity.NullOrEmpty()) {
					return null;
				}
				if (layoutDict.TryGetValue(activity, out Layout layout)) {
					return layout;
				}
				return null;
			}
			set => activity = value?.name;
		}

		public static void NewLayout(string layoutName = null)
		{
			if (layoutName.NullOrEmpty()) {
				do {
					layoutName = string.Format("{0} {1}", defaultLayoutName, ++layoutCount);
				} while (layoutDict.ContainsKey(layoutName));
			} else if (layoutDict.ContainsKey(layoutName)) {
				Log.Error("Name Is In Use");
				return;
			}
			layoutDict.Add(layoutName, new Layout { name = layoutName });
			activity = layoutName;
		}

		public static bool DeleteLayout(string layoutName = null)
		{
			if (layoutName.NullOrEmpty()) {
				layoutDict.Clear();
				return true;
			}
			if (activity == layoutName) {
				activity = layoutDict.Keys.FirstOrDefault();
				if (activity.NullOrEmpty()) {
					NewLayout();
				}
			}
			return layoutDict.Remove(layoutName);
		}

		public static bool RenameLayout(string oldName, string newName)
		{
			if (oldName.NullOrEmpty() || newName.NullOrEmpty()) {
				return false;
			}
			if (activity == oldName) {
				activity = layoutDict.Keys.FirstOrDefault();
				if (activity.NullOrEmpty()) {
					NewLayout();
				}
			}
			if (layoutDict.TryGetValue(oldName, out Layout layout)) {
				layoutDict.Remove(oldName);
				layout.name = newName;
				layoutDict.Add(newName, layout);
				return true;
			}
			return false;
		}

		private static string defaultLayoutName = "Layout";
		private static int layoutCount;
	}
}
