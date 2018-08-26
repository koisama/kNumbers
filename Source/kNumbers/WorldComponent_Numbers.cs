using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace kNumbers
{
    class WorldComponent_Numbers : WorldComponent
    {
        public Dictionary<MainTabWindow_Numbers.PawnType, List<KListObject>> savedKLists = new Dictionary<MainTabWindow_Numbers.PawnType, List<KListObject>>();
        public MainTabWindow_Numbers.PawnType chosenPawnType = new MainTabWindow_Numbers.PawnType();
        public static bool hasData = false;

        public WorldComponent_Numbers(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref chosenPawnType, "chosenPawnType", MainTabWindow_Numbers.PawnType.Colonists);
            foreach (MainTabWindow_Numbers.PawnType type in Enum.GetValues(typeof(MainTabWindow_Numbers.PawnType)))
            {
                savedKLists.TryGetValue(type, out List<KListObject> tmpKList);
                Scribe_Collections.Look(ref tmpKList, "klist-" + type, LookMode.Deep);
                savedKLists[type] = tmpKList;
            }
        }
    }
}
