using System;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace MTW_Treasures
{
    class Treasure_BoxOfPerpetualFlour : Building
    {
        private static readonly ThingDef flourDef = DefDatabase<ThingDef>.GetNamed("PerpetualFlour");
        private static readonly int flourStackLimit = flourDef.stackLimit;

        // Specifies behaviour; should really be in XML
        private const int storedFlourMax = 75;
        private const int numPawnsToFeed = 2;

        // Calculate how much flour it should generate
        private const float avgMealsPerPawnPerDay = 2.0f;
        private const int flourPerMeal = 10;
        private const float flourPerDay = numPawnsToFeed * avgMealsPerPawnPerDay * flourPerMeal;
        // May have slight rounding error here
        private static readonly int flourSpawnTicks = (int)(GenDate.TicksPerDay / flourPerDay);

        private int storedFlour;

        private void addFlour()
        {
            if (this.storedFlour < Treasure_BoxOfPerpetualFlour.storedFlourMax)
            {
                this.storedFlour++;
            }

            var flourInCell = Find.ThingGrid.ThingsListAtFast(this.Position)
                    .Where(t => t.def == flourDef).FirstOrDefault();

            if (flourInCell != null)
            {
                var transferQuantity = Mathf.Min(this.storedFlour,
                    Treasure_BoxOfPerpetualFlour.flourStackLimit - flourInCell.stackCount);
                if (transferQuantity > 0)
                {
                    flourInCell.stackCount += transferQuantity;
                    this.storedFlour -= transferQuantity;
                }
            }
            else if (this.storedFlour >= Treasure_BoxOfPerpetualFlour.flourStackLimit)
            {
                Thing thing = ThingMaker.MakeThing(Treasure_BoxOfPerpetualFlour.flourDef);
                thing.stackCount = Treasure_BoxOfPerpetualFlour.flourStackLimit;
                GenSpawn.Spawn(thing, this.Position);
                this.storedFlour -= Treasure_BoxOfPerpetualFlour.flourStackLimit;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % Treasure_BoxOfPerpetualFlour.flourSpawnTicks == 0)
            {
                this.addFlour();
            }
        }

        public override string GetInspectString()
        {
            var inspectString = new StringBuilder(base.GetInspectString());
            inspectString.AppendFormat("Current flour: {0}/{1}", this.storedFlour,
                Treasure_BoxOfPerpetualFlour.storedFlourMax);
            return inspectString.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.storedFlour, "storedFlour", storedFlourMax);
        }
    }
}
