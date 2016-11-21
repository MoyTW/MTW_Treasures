using RimWorld;
using Verse;
using Verse.AI.Group;

using System.Collections.Generic;
using System.Linq;


namespace MTW_Treasures
{
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

            while (enumerator.MoveNext())
            {
                StockGenerator currentGenerator = enumerator.Current;
                IEnumerator<Thing> generatedThings = currentGenerator.GenerateThings().GetEnumerator();
                try
                {
                    while (generatedThings.MoveNext())
                    {
                        Thing generatedThing = generatedThings.Current;
                        if (!(currentGenerator is StockGenerator_Treasures) &&
                            generatedThing.def.tradeability != Tradeability.Stockable)
                        {
                            Log.Error(string.Concat(new object[]
                            {
                                    traderDef,
                                    " generated carrying ",
                                    generatedThing,
                                    " which has is not Stockable. Ignoring..."
                            }));
                        }
                        else
                        {
                            CompQuality compQuality = generatedThing.TryGetComp<CompQuality>();
                            if (compQuality != null)
                            {
                                compQuality.SetQuality(QualityUtility.RandomTraderItemQuality(),
                                    ArtGenerationContext.Outsider);
                            }
                            if (generatedThing.def.colorGeneratorInTraderStock != null)
                            {
                                var color = generatedThing.def.colorGeneratorInTraderStock.NewRandomizedColor();
                                generatedThing.SetColor(color, true);
                            }
                            if (generatedThing.def.Minifiable)
                            {
                                int stackCount = generatedThing.stackCount;
                                generatedThing.stackCount = 1;
                                MinifiedThing minifiedThing = generatedThing.MakeMinified();
                                minifiedThing.stackCount = stackCount;
                                generatedThing = minifiedThing;
                            }
                            flag = true;
                            yield return generatedThing;
                        }
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        if (generatedThings != null)
                        {
                            generatedThings.Dispose();
                        }
                    }
                }
            }

            yield break;
        }
    }
}
