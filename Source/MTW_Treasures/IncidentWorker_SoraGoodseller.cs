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

        protected override bool CanFireNowSub()
        {
            Pawn sora = TreasuresUtils.TryGetSoraGoodseller();
            return sora == null || !(sora.Dead || sora.Spawned);
        }

        public override bool TryExecute(IncidentParms parms)
        {
            if (!this.TryResolveParms(parms)) { return false; }

            Pawn sora = TreasuresUtils.TryGetSoraGoodseller();
            if (sora == null)
            {
                sora = TreasuresUtils.GenSoraGoodseller(parms.faction);
            }
            else if (sora.Dead || sora.Spawned)
            {
                return false;
            }
            else
            {
                sora.SetFaction(parms.faction);
            }

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
            foreach (Thing current in this.GenerateTraderThings(traderKindDef))
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

        private IEnumerable<Thing> GenerateTraderThings(TraderKindDef traderDef)
        {
            bool flag = false;
            List<StockGenerator>.Enumerator enumerator = traderDef.stockGenerators.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    StockGenerator current = enumerator.Current;
                    IEnumerator<Thing> enumerator2 = current.GenerateThings().GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            Thing current2 = enumerator2.Current;
                            if (!(current is StockGenerator_Treasures) && current2.def.tradeability != Tradeability.Stockable)
                            {
                                Log.Error(string.Concat(new object[]
                                {
                                    traderDef,
                                    " generated carrying ",
                                    current2,
                                    " which has is not Stockable. Ignoring..."
                                }));
                            }
                            else
                            {
                                CompQuality compQuality = current2.TryGetComp<CompQuality>();
                                if (compQuality != null)
                                {
                                    compQuality.SetQuality(QualityUtility.RandomTraderItemQuality(), ArtGenerationContext.Outsider);
                                }
                                if (current2.def.colorGeneratorInTraderStock != null)
                                {
                                    current2.SetColor(current2.def.colorGeneratorInTraderStock.NewRandomizedColor(), true);
                                }
                                if (current2.def.Minifiable)
                                {
                                    int stackCount = current2.stackCount;
                                    current2.stackCount = 1;
                                    MinifiedThing minifiedThing = current2.MakeMinified();
                                    minifiedThing.stackCount = stackCount;
                                }
                                flag = true;
                                yield return current2;
                            }
                        }
                    }
                    finally
                    {
                        if (!flag)
                        {
                            if (enumerator2 != null)
                            {
                                enumerator2.Dispose();
                            }
                        }
                    }
                }
            }
            finally
            {
            }
            yield break;
        }
    }
}
