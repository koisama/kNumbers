﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections;

namespace kNumbers
{
    [StaticConstructorOnStartup]
    public class KListObject : IExposable
    {

        public enum objectType
        {
            Stat,
            Health, //can wait
            Need,
            Skill,
            Gear,   //weapon and all apparel
            ControlPrisonerGetsFood,
            ControlMedicalCare,
            ControlPrisonerInteraction,
            CurrentJob,
            AnimalMilkFullness,
            AnimalWoolGrowth,
            Age,
            MentalState,
            Capacity
        }

        public objectType oType;
        public string label;
        public object displayObject;
        public float minWidthDesired = 120f;

        private static Texture2D    passionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor", true),
                                    passionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor", true),
                                    SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.25f)),
                                    SkillBarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.07f)),
                                    BarInstantMarkerTex = BarInstantMarkerTex = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarker", true);

        public static Texture2D[] careTextures = new Texture2D[]
        {
            ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare", true),
            ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoMeds", true),
            ThingDefOf.HerbalMedicine.uiIcon,
            ThingDefOf.Medicine.uiIcon,
            ThingDefOf.GlitterworldMedicine.uiIcon
        };

        private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);
        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static MethodInfo mGetSkillDescription = typeof(SkillUI).GetMethod("GetSkillDescription", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, new[] { typeof(SkillRecord) }, null);

        private static FieldInfo needThreshPercent = typeof(Need).GetField("threshPercents", BindingFlags.NonPublic | BindingFlags.Instance);

        public void ExposeData()
        {
            
            Scribe_Values.Look<objectType>(ref oType, "oType");
            Scribe_Values.Look<float>(ref minWidthDesired, "minWidthDesired");
            Scribe_Values.Look<string>(ref this.label, "label");

            switch (oType)
            {
                case objectType.Stat:
                    StatDef tempObjectS = (StatDef)displayObject;
                    Scribe_Defs.Look(ref tempObjectS, "displayObject");
                    displayObject = tempObjectS;
                    break;

                case objectType.Skill:
                    SkillDef tempObjectK = (SkillDef)displayObject;
                    Scribe_Defs.Look(ref tempObjectK, "displayObject");
                    displayObject = tempObjectK;
                    break;

                case objectType.Need:
                    NeedDef tempObjectN = (NeedDef)displayObject;
                    Scribe_Defs.Look(ref tempObjectN, "displayObject");
                    displayObject = tempObjectN;
                    break;

                case objectType.Capacity:
                    PawnCapacityDef tempObjectCap = (PawnCapacityDef)displayObject;
                    Scribe_Defs.Look(ref tempObjectCap, "displayObject");
                    displayObject = tempObjectCap;
                    break;

            }

        }

        //Scribe wants it
        public KListObject()
        {
            oType = objectType.Stat;
            label = " - ";
            displayObject = null;
        }

        public KListObject(objectType type, string defName, object dObject)
        {
            this.oType = type;
            this.label = defName;
            this.displayObject = dObject;

            switch (oType)
            {
                case objectType.Skill:
                    minWidthDesired = 120f;
                    break;

                case objectType.Age:
                case objectType.AnimalMilkFullness:
                case objectType.AnimalWoolGrowth:
                case objectType.Stat:
                    minWidthDesired = 80f;
                    break;

                case objectType.Capacity:
                    minWidthDesired = 60f;
                    break;

                case objectType.Need:
                    minWidthDesired = 110f;
                    break;

                case objectType.Gear:
                    minWidthDesired = 210f;
                    break;

                case objectType.ControlPrisonerGetsFood:
                    minWidthDesired = 40f;
                    break;

                case objectType.MentalState:
                case objectType.ControlPrisonerInteraction:
                    minWidthDesired = 160f;
                    break;

                case objectType.ControlMedicalCare:
                    minWidthDesired = 100f;
                    break;

                case objectType.CurrentJob:
                    minWidthDesired = 260f;
                    break;
            }

        }

        private void DrawSkill(Rect rect, Pawn ownerPawn)
        {
            if (ownerPawn.RaceProps.IsMechanoid) return;
            if (ownerPawn.RaceProps.Animal) return;

            SkillRecord skill = ownerPawn.skills.GetSkill((SkillDef)displayObject);
            GUI.BeginGroup(rect);
            Rect position = new Rect(3f, 3f, 24f, 24f);
            if (skill.passion > Passion.None)
            {
                Texture2D image = (skill.passion != Passion.Major) ? passionMinorIcon : passionMajorIcon;
                GUI.DrawTexture(position, image);
            }
            if (!skill.TotallyDisabled)
            {
                Rect rect3 = new Rect(position.xMax, 0f, rect.width - position.xMax, rect.height);
                Widgets.FillableBar(rect3, (float)skill.Level / 20f, SkillBarFillTex, SkillBarBgTex, false);
            }
            Rect rect4 = new Rect(position.xMax + 4f, 0f, 999f, rect.height);
            rect4.yMin += 3f;
            string label;
            if (skill.TotallyDisabled)
            {
                GUI.color = DisabledSkillColor;
                label = "-";
            }
            else
            {
                label = skill.Level.ToStringCached();
            }
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Widgets.Label(rect4, label);
            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            GUI.EndGroup();
            TooltipHandler.TipRegion(rect, new TipSignal((string)mGetSkillDescription.Invoke(null, new[] { skill }), skill.def.GetHashCode() * 397945));
        }


        private void DrawNeed(Rect rect, Pawn ownerPawn)
        {
            if (ownerPawn.RaceProps.IsMechanoid) return;
            if (ownerPawn.needs == null) return;
            //TODO: rebuild using code in DrawOnGUI
            Need need = ownerPawn.needs.TryGetNeed((NeedDef)displayObject);
            if (need == null) return;

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            TooltipHandler.TipRegion(rect, new TipSignal(() => need.GetTipString(), rect.GetHashCode()));
            float num2 = 14f;
            float num3 = num2 + 15f;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }

            

            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Widgets.FillableBar(rect3, need.CurLevelPercentage);
            Widgets.FillableBarChangeArrows(rect3, need.GUIChangeArrow);
            List<float> threshPercents = (List<float>)needThreshPercent.GetValue(need);
            if (threshPercents != null)
            {
                for (int i = 0; i < threshPercents.Count; i++)
                {
                    needDrawBarThreshold(rect3, threshPercents[i], need.CurLevelPercentage);
                }
            }
            float curInstantLevel = need.CurInstantLevelPercentage;
            if (curInstantLevel >= 0f)
            {
                needDrawBarInstantMarkerAt(rect3, curInstantLevel);
            }   
            Text.Font = GameFont.Small;
        }

        private void DrawGear(Rect rect, ThingWithComps ownerPawn)
        {
            GUI.BeginGroup(rect);
            float x = 0;
            float gWidth = 28f;
            float gHeight = 28f;
            Pawn p1 = (ownerPawn is Pawn) ? (ownerPawn as Pawn) : (ownerPawn as Corpse).InnerPawn;
            if (p1.RaceProps.Animal) return;
            if (p1.equipment != null)
            foreach(ThingWithComps thing in p1.equipment.AllEquipmentListForReading)
                {
                    Rect rect2 = new Rect(x, 0, gWidth, gHeight);
                    DrawThing(rect2, thing, p1);
                    x += gWidth;
                }

            if (p1.apparel != null)
            foreach (Apparel thing in from ap in p1.apparel.WornApparel
                                            orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                            select ap)
                {
                    Rect rect2 = new Rect(x, 0, gWidth, gHeight);
                    DrawThing(rect2, thing, p1);
                    x += gWidth;
                }
            GUI.EndGroup();
        }

        private void DrawThing(Rect rect, Thing thing, Pawn selPawn)
        {
            
            if (Mouse.IsOver(rect))
            {
                GUI.color = HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (Widgets.ButtonInvisible(rect) && Event.current.button == 1)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("ThingInfo".Translate(), delegate
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(thing));
                }, MenuOptionPriority.Default, null, null));
                if (selPawn.IsColonistPlayerControlled)
                {
                    Action action = null;
                    ThingWithComps eq = thing as ThingWithComps;
                    Apparel ap = thing as Apparel;
                    if (ap != null)
                    {
                        action = delegate
						{
							Apparel unused;
							selPawn.apparel.TryDrop(ap, out unused, selPawn.Position, true);
                        };
                    }
                    else if (eq != null && selPawn.equipment.AllEquipmentListForReading.Contains(eq))
                    {
                        action = delegate
						{
							ThingWithComps unused;
							selPawn.equipment.TryDropEquipment(eq, out unused, selPawn.Position, true);
                        };
                    }
                    else if (!thing.def.destroyOnDrop)
                    {
                        action = delegate
						{
							Thing unused;
							selPawn.inventory.innerContainer.TryDrop(thing, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out unused);
                        };
                    }   
                    list.Add(new FloatMenuOption("DropThing".Translate(), action, MenuOptionPriority.Default, null, null));
                }
                FloatMenu window = new FloatMenu(list, thing.LabelCap, false);
                Find.WindowStack.Add(window);
            }
            GUI.BeginGroup(rect);
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(3f, 3f, 27f, 27f), thing);
            }
            GUI.EndGroup();
            TooltipHandler.TipRegion(rect, new TipSignal(thing.LabelCap));
        }

        

        public void needDrawBarThreshold(Rect barRect, float threshPct, float curLevel)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < curLevel)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        public void needDrawBarInstantMarkerAt(Rect barRect, float pct)
        {
            float num = 12f;
            if (barRect.width < 150f)
            {
                num /= 2f;
            }
            Vector2 vector = new Vector2(barRect.x + barRect.width * pct, barRect.y + barRect.height);
            Rect position = new Rect(vector.x - num / 2f, vector.y, num, num);
            GUI.DrawTexture(position, BarInstantMarkerTex);
        }

        //from MedicalInfo
        public static void MedicalCareSetter(Rect rect, ref MedicalCareCategory medCare)
        {
            float iconSize = rect.width / 5f;
            float iconHeightOffset = (rect.height - iconSize) / 2;
            Rect rect2 = new Rect(rect.x, rect.y + iconHeightOffset, iconSize, iconSize);
            for (int i = 0; i < 5; i++)
            {
                MedicalCareCategory mc = (MedicalCareCategory)i;
                Widgets.DrawHighlightIfMouseover(rect2);
                GUI.DrawTexture(rect2, careTextures[i]);
                if (Widgets.ButtonInvisible(rect2))
                {
                    medCare = mc;
                }
                if (medCare == mc)
                {
                    GUI.DrawTexture(rect2, Widgets.CheckboxOnTex);
                }
                TooltipHandler.TipRegion(rect2, () => mc.GetLabel(), 632165 + i * 17);
                rect2.x += rect2.width;
            }
        }

        public void Draw(Rect rect, ThingWithComps ownerPawn)
        {
            Text.Font = GameFont.Small;
            string value = "-";

            switch (oType)
            {
                case objectType.Stat:
                    Text.Anchor = TextAnchor.MiddleCenter;
                    StatDef stat = (StatDef)displayObject;
                    string statValue = (stat.ValueToString(ownerPawn.GetStatValue((StatDef)displayObject, true)));
                    Widgets.Label(rect, statValue);
                    if (Mouse.IsOver(rect))
                    {
                        GUI.DrawTexture(rect, TexUI.HighlightTex);
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(stat.LabelCap);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(stat.description);
                    TooltipHandler.TipRegion(rect, new TipSignal(stringBuilder.ToString(), rect.GetHashCode()));
                    break;

                case objectType.Skill:
                    if ((ownerPawn is Pawn) && (ownerPawn as Pawn).RaceProps.Humanlike) DrawSkill(rect, ownerPawn as Pawn);
                    break;

                case objectType.Need:
                    if (ownerPawn is Pawn) DrawNeed(rect, ownerPawn as Pawn);
                    break;

                case objectType.Capacity:
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (ownerPawn is Pawn)
                    {
                        Pawn p = (Pawn)ownerPawn;
                        PawnCapacityDef cap = (PawnCapacityDef)displayObject;

                        Pair<string, Color> effLabel = HealthCardUtility.GetEfficiencyLabel(p, cap);
                        string pawnCapTip = HealthCardUtility.GetPawnCapacityTip(p, cap);

                        
                        // I stole this one line from Fluffy's Medical Tab. THANKS FLUFFY!
                        string capValue = (p.health.capacities.GetLevel(cap) * 100f).ToString("F0") + "%";
                        GUI.color = effLabel.Second;
                        Widgets.Label(rect, capValue);
                        GUI.color = Color.white;

                        if (Mouse.IsOver(rect))
                        {
                            GUI.DrawTexture(rect, TexUI.HighlightTex);
                        }

                        StringBuilder stringBuilder2 = new StringBuilder();
                        stringBuilder2.AppendLine(cap.LabelCap);
                        stringBuilder2.AppendLine();
                        stringBuilder2.AppendLine(cap.description);
                        TooltipHandler.TipRegion(rect, new TipSignal(stringBuilder2.ToString(), rect.GetHashCode()));
                    }
                    break;

                case objectType.MentalState:
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (ownerPawn is Pawn && (ownerPawn as Pawn).MentalState != null) { 
                        string ms = ((ownerPawn as Pawn).MentalState.InspectLine);
                        Widgets.Label(rect, ms);
                        if (Mouse.IsOver(rect))
                        {
                            GUI.DrawTexture(rect, TexUI.HighlightTex);
                        }
                    }
                    Text.Font = GameFont.Medium;
                    break;

                case objectType.Age:
                    Text.Anchor = TextAnchor.MiddleCenter;
                    string ageValue = ((ownerPawn as Pawn).ageTracker.AgeBiologicalYears.ToString());
                    Widgets.Label(rect, ageValue);
                    if (Mouse.IsOver(rect))
                    {
                        GUI.DrawTexture(rect, TexUI.HighlightTex);
                    }
                    break;

                case objectType.Gear:
                    DrawGear(rect, ownerPawn);
                    break;

                case objectType.ControlPrisonerGetsFood:
                    if (ownerPawn is Pawn)
                    {
                        if (Mouse.IsOver(rect))
                        {
                            GUI.DrawTexture(rect, TexUI.HighlightTex);
                        }
                        bool getsFood = (ownerPawn as Pawn).guest.GetsFood;
                        Widgets.CheckboxLabeled(new Rect(rect.x + 8f, rect.y + 3f, 27f, 27f), "", ref getsFood, false);
                        (ownerPawn as Pawn).guest.GetsFood = getsFood;
                    }
                    break;

                case objectType.ControlPrisonerInteraction:
                    if (ownerPawn is Pawn)
		            {
			            if (Mouse.IsOver(rect)) { GUI.DrawTexture(rect, TexUI.HighlightTex); }
			            float x = 8f;

			            GUI.BeginGroup(rect);

			            foreach (var prisonerInteractionMode in DefDatabase<PrisonerInteractionModeDef>.AllDefs)
			            {
				            if (Widgets.RadioButton(new Vector2(x, 3f), (ownerPawn as Pawn).guest.interactionMode == prisonerInteractionMode)) { (ownerPawn as Pawn).guest.interactionMode = prisonerInteractionMode; }
				            TooltipHandler.TipRegion(new Rect(x, 0f, 30f, 30f), new TipSignal(prisonerInteractionMode.GetLabel()));
				            x += 30f;
			            }

			            GUI.EndGroup();
		            }
		            break;

                case objectType.ControlMedicalCare:
                    if (ownerPawn is Pawn) MedicalCareSetter(rect, ref (ownerPawn as Pawn).playerSettings.medCare);
                    break;

                case objectType.AnimalMilkFullness:
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (ownerPawn is Pawn && ((Pawn)ownerPawn).ageTracker.CurLifeStage.milkable)
                    {
                        var comp = ownerPawn.AllComps.OfType<CompMilkable>().FirstOrDefault();
                        if(comp != null)
                        value = comp.Fullness.ToStringPercent();
                    }

                    Widgets.Label(rect, value);
                    break;

                case objectType.AnimalWoolGrowth:
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (ownerPawn is Pawn && ((Pawn)ownerPawn).ageTracker.CurLifeStage.shearable)
                    {
                        var comp = ownerPawn.AllComps.OfType<CompShearable>().FirstOrDefault();
                        if (comp != null)
                            value = comp.Fullness.ToStringPercent();
                    }

                    Widgets.Label(rect, value);
                    break;

                case objectType.CurrentJob:
                    if(ownerPawn is Pawn && ((Pawn)ownerPawn).jobs.curDriver != null )
                    {
                        string text = ((Pawn)ownerPawn).jobs.curDriver.GetReport();
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Rect tRect = new Rect(rect.xMin + 2, rect.yMin + 3, rect.width - 2, rect.height);
                        GenText.SetTextSizeToFit(text, tRect);

                        if (Text.Font == GameFont.Tiny)
                            Widgets.Label(tRect, text);
                        else
                        {
                            Rect sRect = new Rect(rect.xMin + 2, rect.yMin, rect.width - 2, rect.height);
                            Widgets.Label(sRect, text);
                        }

                        if (Mouse.IsOver(rect))
                        {
                            GUI.DrawTexture(rect, TexUI.HighlightTex);
                        }
                    }
                    break;
            }


        }

        
    }
}
