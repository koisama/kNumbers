using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace kNumbers
{
    class GameComponent_Numbers : GameComponent
    {

        public Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>> savedKLists = new Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>>(5);
        public MainTabWindow_Numbers.pawnType chosenPawnType = MainTabWindow_Numbers.pawnType.Colonists;


	    public GameComponent_Numbers()
	    {
		    LongEventHandler.ExecuteWhenFinished(() => { if (Current.Game.GetComponent<GameComponent_Numbers>() == null) { Current.Game.components.Add(this); } });
	    }


	    public override void ExposeData()
	    {

			Scribe_Values.Look(ref chosenPawnType, "chosenPawnType", MainTabWindow_Numbers.pawnType.Colonists);
			foreach (MainTabWindow_Numbers.pawnType type in Enum.GetValues(typeof(MainTabWindow_Numbers.pawnType)))
			{
				List<KListObject> tmpKList;
				savedKLists.TryGetValue(type, out tmpKList);
				Scribe_Collections.Look(ref tmpKList, "klist-" + type, LookMode.Deep);
				savedKLists[type] = tmpKList;

				/*foreach(KListObject obj in tmpKList ?? Enumerable.Empty<KListObject>())
				{
					Log.Message("scribe loaded object "+ obj.oType.ToString() + ", " + obj.label + "," + (obj.displayObject == null? "NULL!" : obj.displayObject.ToString()));
				}*/

			}

		}
		
    }
}
