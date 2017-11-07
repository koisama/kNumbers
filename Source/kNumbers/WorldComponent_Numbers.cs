using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace kNumbers
{
    class WorldComponent_Numbers : WorldComponent
    {
        public Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>> savedKLists = new Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>>();
        public MainTabWindow_Numbers.pawnType chosenPawnType = new MainTabWindow_Numbers.pawnType();
        public static bool hasData = false;

        public WorldComponent_Numbers(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref chosenPawnType, "chosenPawnType", MainTabWindow_Numbers.pawnType.Colonists);
            foreach (MainTabWindow_Numbers.pawnType type in Enum.GetValues(typeof(MainTabWindow_Numbers.pawnType)))
            {
                List<KListObject> tmpKList;
                savedKLists.TryGetValue(type, out tmpKList);
                Scribe_Collections.Look(ref tmpKList, "klist-" + type, LookMode.Deep);
                savedKLists[type] = tmpKList;
            }
        }
    }
}
