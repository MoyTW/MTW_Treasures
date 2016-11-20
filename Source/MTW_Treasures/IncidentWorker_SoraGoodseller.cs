using RimWorld;
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
            if (!this.TryResolveParms(parms)) { return false; }

            var sora = TreasuresUtils.SoraGoodseller(parms.faction);
            if (sora.Dead && sora.Spawned) { return false; }

            List<Pawn> list = base.SpawnPawns(parms);
            list.Add(sora);
            GenSpawn.Spawn(sora, CellFinder.RandomClosewalkCellNear(parms.spawnCenter, 5));

            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(sora, out chillSpot);
            LordJob_VisitColony lordJob = new LordJob_VisitColony(parms.faction, chillSpot);
            LordMaker.MakeNewLord(parms.faction, lordJob, list);

            this.TryConvertOnePawnToSmallTrader(new List<Pawn> { sora }, parms.faction);

            Pawn factionLeader = list.Find((Pawn x) => parms.faction.leader == x);
            string label;
            string letterText;

            string text = "SingleVisitorArrivesTraderInfo".Translate();
            string text2 = (factionLeader == null) ? string.Empty : "SingleVisitorArrivesLeaderInfo".Translate();
            label = "LetterLabelSingleVisitorArrives".Translate();
            letterText = "SingleVisitorArrives".Translate(new object[]
            {
                sora.story.adulthood.title.ToLower(),
                parms.faction.Name,
                sora.Name,
                text,
                text2
            });
            letterText = letterText.AdjustedFor(sora);

            Find.LetterStack.ReceiveLetter(label, letterText, LetterType.Good, sora);
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
