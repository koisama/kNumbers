using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace kNumbers
{
    class KListObject
    {

        public enum objectType
        {
            Stat,
            Health, //can wait
            Need,
            Skill,
            Control //can wait
        }

        public objectType oType;
        public string label { get; }
        public object displayObject { get; }
        public float minWidthDesired { get; } = 120f;

        private static Texture2D    passionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor", true),
                                    passionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor", true),
                                    SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.25f)),
                                    SkillBarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.07f)),
                                    BarInstantMarkerTex = BarInstantMarkerTex = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarker", true);

        private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

        private static MethodInfo mGetSkillDescription = typeof(SkillUI).GetMethod("GetSkillDescription", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, new[] { typeof(SkillRecord) }, null);

        private static FieldInfo needThreshPercent = typeof(Need).GetField("threshPercents", BindingFlags.NonPublic | BindingFlags.Instance);

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

                case objectType.Stat:
                    minWidthDesired = 80f;
                    break;

                case objectType.Need:
                    minWidthDesired = 120f;
                    break;
            }

        }

        private void DrawSkill(Rect rect, Pawn ownerPawn)
        {
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
                Widgets.FillableBar(rect3, (float)skill.level / 20f, SkillBarFillTex, SkillBarBgTex, false);
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
                label = skill.level.ToStringCached();
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
            //TODO: rebuild using code in DrawOnGUI
            Need need = ownerPawn.needs.TryGetNeed((NeedDef)displayObject);

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
            Widgets.FillableBar(rect3, need.CurLevel);
            Widgets.FillableBarChangeArrows(rect3, need.GUIChangeArrow);
            List<float> threshPercents = (List<float>)needThreshPercent.GetValue(need);
            if (threshPercents != null)
            {
                for (int i = 0; i < threshPercents.Count; i++)
                {
                    needDrawBarThreshold(rect3, threshPercents[i], need.CurLevel);
                }
            }
            float curInstantLevel = need.CurInstantLevel;
            if (curInstantLevel >= 0f)
            {
                needDrawBarInstantMarkerAt(rect3, curInstantLevel);
            }
            Text.Font = GameFont.Small;
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

        public void Draw(Rect rect, Pawn ownerPawn)
        {
            switch (oType)
            {
                case objectType.Stat:
                    //TODO: add tooltip with explanation (like what's being displayed in info window)
                    //TODO: refactor to DrawSkill
                    Text.Anchor = TextAnchor.MiddleCenter;
                    string statValue = ((StatDef)displayObject).ValueToString(ownerPawn.GetStatValue((StatDef)displayObject, true));
                    Widgets.Label(rect, statValue);
                    if (Mouse.IsOver(rect))
                    {
                        GUI.DrawTexture(rect, TexUI.HighlightTex);
                    }
                    break;

                case objectType.Skill:
                    DrawSkill(rect, ownerPawn);
                    break;

                case objectType.Need:
                    DrawNeed(rect, ownerPawn);
                    break;
            }


        }


    }
}
