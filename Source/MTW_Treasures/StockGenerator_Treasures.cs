using RimWorld;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace MTW_Treasures
{
    public class StockGenerator_Treasures : StockGenerator
    {
        private Thing TryMakeForStockSingle(ThingDef thingDef)
        {
            ThingDef stuff = null;
            if (thingDef.MadeFromStuff)
            {
                stuff = (from st in DefDatabase<ThingDef>.AllDefs
                         where st.IsStuff && st.stuffProps.CanMake(thingDef)
                         select st).RandomElementByWeight((ThingDef st) => st.stuffProps.commonality);
            }
            Thing thing = ThingMaker.MakeThing(thingDef, stuff);
            thing.stackCount = 1;
            return thing;
        }

        private bool CanStockTreasure(ThingDef def)
        {
            return Find.ListerThings.ThingsOfDef(def).Count == 0;
        }

        public override IEnumerable<Thing> GenerateThings()
        {
            // Always generates 1 of every treasure at the moment.
            foreach (var def in TreasuresUtils.AllTreasuresDefs.Where(d => this.CanStockTreasure(d)))
            {
                yield return this.TryMakeForStockSingle(def);
            }
            yield break;
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return TreasuresUtils.AllTreasuresDefs.Contains(thingDef);
        }
    }
}
