using RimWorld;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace MTW_Treasures
{
    public class StockGenerator_Treasures : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings()
        {
            // Always generates 1 of every treasure at the moment.
            foreach (var def in TreasuresUtils.AllTreasuresDefs)
            {
                if (def.tradeability != Tradeability.Stockable)
                {
                    break;
                }
                else
                {
                    IEnumerator<Thing> enumerator = StockGeneratorUtility.TryMakeForStock(def, 1).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Thing current = enumerator.Current;
                        yield return current;
                    }
                }
            }
            yield break;
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return TreasuresUtils.AllTreasuresDefs.Contains(thingDef);
        }
    }
}
