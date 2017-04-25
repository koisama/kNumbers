using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace kNumbers
{
	public class Settings : IExposable
	{
		public static bool LayoutIsValid;
		private static LayoutCollection layouts = new LayoutCollection();
		public static List<Layout> Layouts
		{
			get => layouts?.ToList();
		}
		private static string activity;
		public static Layout ActivityLayout
		{
			get {
				if (activity.NullOrEmpty()) {
					return null;
				}
				return layouts.TryGetValue(activity, out Layout layout) ? layout : null;
			}

			set => activity = value?.name;
		}

		public virtual void ExposeData()
		{
			if (!(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars))
				return;

			var tmp = new List<Layout>(layouts);

			Scribe_Values.LookValue(ref activity, "activity");
			Scribe_Collections.LookList(ref tmp, "layouts", LookMode.Deep);

			#region postload
			if (Scribe.mode == LoadSaveMode.LoadingVars) {
				layouts = new LayoutCollection(tmp);
				postload();
			}
			#endregion

		}

		private static void postload()
		{
#if DEBUG
			{
				var strings = layouts?.Select(l => l.name).ToArray();
				if (strings.NullOrEmpty()) {
					strings = new[] { "null" };
				}
				Log.Message($"postLoad. activity: '{activity}', Layouts: {string.Join(", ", strings)}");
			}
#endif
			if (layouts == null || layouts.Count == 0 || (layouts[0]?.name).NullOrEmpty()) {
				Log.Error("invalid layouts.");
				LayoutIsValid = false;
				layouts = new LayoutCollection();
				return;
			}
			LayoutIsValid = true;
			if (activity.NullOrEmpty()) {
				activity = layouts.FirstOrDefault()?.name;
			}
			ActivityLayout = layouts.Where(l => {
				return l.name == activity;
			}).SingleOrDefault();
		}

		public static void NewLayout(string layoutName = null)
		{
			if (layoutName.NullOrEmpty()) {
				do {
					layoutName = string.Format("{0} {1}", defaultLayoutName, ++layoutCount);
				} while (layouts.Contains(layoutName));
			} else if (layouts.Contains(layoutName)) {
				Log.Error("Name Is In Use");
				return;
			}
			layouts.Add(new Layout { name = layoutName });
			activity = layoutName;
		}

		public static bool DeleteLayout(string layoutName = null)
		{
			if (layoutName.NullOrEmpty()) {
				layouts.Clear();
				return true;
			}
			if (activity == layoutName) {
				activity = layouts.FirstOrDefault()?.name;
				if (activity.NullOrEmpty()) {
					NewLayout();
				}
			}
			return layouts.Remove(layoutName);
		}

		public static bool RenameLayout(string oldName, string newName)
		{
			if (oldName.NullOrEmpty() || newName.NullOrEmpty()) {
				return false;
			}
			if (activity == oldName) {
				activity = layouts.FirstOrDefault()?.name;
				if (activity.NullOrEmpty()) {
					NewLayout();
				}
			}
			if (layouts.Contains(oldName)) {
				layouts.ChangeItemKey(layouts[oldName], newName);
				return true;
			}

			return false;
		}

		private static string defaultLayoutName = "Layout"; //FIX: translate
		private static int layoutCount;

	}

	public class Layout : Settings
	{
		public string name;
		public MainTabWindow_Numbers.pawnType chosenPawnType;
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
			if (!(Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars))
				return;

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

	public class LayoutCollection : KeyedCollection<string, Layout>
	{
		public LayoutCollection(IEnumerable<Layout> layouts = null) : base(null, 0)
		{
			if (layouts == null) {
				return;
			}
			foreach (var item in layouts) {
				Add(item);
			}
		}

		protected override string GetKeyForItem(Layout item)
		{
			return item.name;
		}

		internal new void ChangeItemKey(Layout item, string newKey)
		{
			item.name = newKey;
			base.ChangeItemKey(item, newKey);
		}

		public ICollection<string> Keys
		{
			get {
				if (Dictionary != null) {
					return Dictionary.Keys;
				} else {
					return new Collection<string>(this.Select(GetKeyForItem).ToArray());
				}
			}
		}

		public bool TryGetValue(string key, out Layout value)
		{
			if (Dictionary != null) {
				return Dictionary.TryGetValue(key, out value);
			} else if (key == null) {
				throw new ArgumentNullException("key");
			}
			value = this.SingleOrDefault(x => x.name == key);
			return value != null;
		}

	}


}
