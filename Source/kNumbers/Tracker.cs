using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace kNumbers
{
	class Tracker : MonoBehaviour
	{
		private static Settings _settings;
		public static void Initialize(ref Settings settings)
		{
			if (Current.Root_Play == null) {
				return;
			}
			if (Current.Root_Play.gameObject.GetComponent<Tracker>() != null) {
				return;
			}
			if (Current.Root_Play.gameObject.AddComponent<Tracker>() == null) {
				Log.Error("Current.Root_Play.gameObject.AddComponent<Settings>() == null");
				return;
			}
			_settings = settings;
		}

		void OnDestroy()
		{
			PersistentDataManager.SaveFrom(ref _settings);
		}
	}
}
