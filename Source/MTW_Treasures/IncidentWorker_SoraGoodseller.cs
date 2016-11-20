﻿using RimWorld;
using Verse;
using Verse.AI.Group;

using System.Collections.Generic;
using System.Linq;


namespace MTW_Treasures
{
    // Literally copy-pased decompiled IncidentWorker_VisitorGroup with minimal changes!
    // TODO: Actually make it use Sora Goodseller and don't stock duplicates!
    public class IncidentWorker_SoraGoodseller : IncidentWorker_NeutralGroup
    {
        public static readonly TraderKindDef SoraTraderKindDef =
            DefDatabase<TraderKindDef>.GetNamed("Visitor_Treasures_Standard");

        public override bool TryExecute(IncidentParms parms)
        {
            if (!this.TryResolveParms(parms))
            {
                return false;
            }
            List<Pawn> list = base.SpawnPawns(parms);
            if (list.Count == 0)
            {
                return false;
            }
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out chillSpot);
            LordJob_VisitColony lordJob = new LordJob_VisitColony(parms.faction, chillSpot);
            LordMaker.MakeNewLord(parms.faction, lordJob, list);

            bool flag = false;
            flag = this.TryConvertOnePawnToSmallTrader(list, parms.faction);

            Pawn pawn = list.Find((Pawn x) => parms.faction.leader == x);
            string label;
            string text3;
            if (list.Count == 1)
            {
                string text = (!flag) ? string.Empty : "SingleVisitorArrivesTraderInfo".Translate();
                string text2 = (pawn == null) ? string.Empty : "SingleVisitorArrivesLeaderInfo".Translate();
                label = "LetterLabelSingleVisitorArrives".Translate();
                text3 = "SingleVisitorArrives".Translate(new object[]
                {
                    list[0].story.adulthood.title.ToLower(),
                    parms.faction.Name,
                    list[0].Name,
                    text,
                    text2
                });
                text3 = text3.AdjustedFor(list[0]);
            }
            else
            {
                string text4 = (!flag) ? string.Empty : "GroupVisitorsArriveTraderInfo".Translate();
                string text5 = (pawn == null) ? string.Empty : "GroupVisitorsArriveLeaderInfo".Translate(new object[]
                {
                    pawn.LabelShort
                });
                label = "LetterLabelGroupVisitorsArrive".Translate();
                text3 = "GroupVisitorsArrive".Translate(new object[]
                {
                    parms.faction.Name,
                    text4,
                    text5
                });
            }
            Find.LetterStack.ReceiveLetter(label, text3, LetterType.Good, list[0], null);
            return true;
        }

        private bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Faction faction)
        {
            Pawn pawn = pawns.RandomElement<Pawn>();
            Lord lord = pawn.GetLord();
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            TraderKindDef traderKindDef = IncidentWorker_SoraGoodseller.SoraTraderKindDef;
            pawn.trader.traderKind = traderKindDef;
            pawn.inventory.DestroyAll(DestroyMode.Vanish);
            foreach (Thing current in TraderStockGenerator.GenerateTraderThings(traderKindDef))
            {
                Pawn pawn2 = current as Pawn;
                if (pawn2 != null)
                {
                    if (pawn2.Faction != pawn.Faction)
                    {
                        pawn2.SetFaction(pawn.Faction, null);
                    }
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, 5);
                    GenSpawn.Spawn(pawn2, loc);
                    lord.AddPawn(pawn2);
                }
                else if (!pawn.inventory.container.TryAdd(current))
                {
                    current.Destroy(DestroyMode.Vanish);
                }
            }
            if (!pawn.inventory.container.Any((Thing x) => x.def.IsNutritionGivingIngestible && x.def.ingestible.preferability >= FoodPreferability.MealAwful))
            {
                PawnInventoryGenerator.GiveRandomFood(pawn);
            }
            return true;
        }
    }
}
