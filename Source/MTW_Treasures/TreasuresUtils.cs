using RimWorld;
using Verse;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

namespace MTW_Treasures
{
    static class TreasuresUtils
    {
        public static readonly List<ThingDef> AllTreasureDefsListForReading =
            DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => TreasuresUtils.IsTreasureDef(def))
            .ToList();

        public static bool IsTreasureDef(ThingDef def)
        {
            return def.GetCompProperties<CompProperties_Treasure>() != null;
        }

        // This is really *kind* of a silly method of checking!
        public static bool IsSoraGoodseller(Pawn p)
        {
            return p != null && p.Name != null && p.Name.ToStringFull == "Sora Goodseller";
        }

        // Will coerce Sora to the Faction in question if Sora already exists!
        public static Pawn TryGetSoraGoodseller()
        {
            Pawn sora = Find.MapPawns.AllPawns.Where(p => IsSoraGoodseller(p)).FirstOrDefault();
            if (sora == null)
            {
                sora = Find.WorldPawns.AllPawnsAlive.Where(p => IsSoraGoodseller(p)).FirstOrDefault();
            }
            return sora;
        }

        // Reference: http://kingofdragonpass.wikia.com/wiki/Sora_Goodseller
        public static Pawn GenSoraGoodseller(Faction faction)
        {
            var tribalTraderDef = DefDatabase<PawnKindDef>.GetNamed("TribalTrader");
            var genRequest = new PawnGenerationRequest(tribalTraderDef, faction: faction, fixedGender: Gender.Female,
                fixedBiologicalAge: 32, fixedChronologicalAge: 32, canGeneratePawnRelations: false,
                forceGenerateNewPawn: true);
            Pawn generated = PawnGenerator.GeneratePawn(genRequest);

            // Set up the history; lot of magic values here.
            generated.Name = new NameTriple("Sora", "Sora", "Goodseller");
            generated.story.crownType = CrownType.Narrow;
            generated.story.hairDef = DefDatabase<HairDef>.GetNamed("Mop");
            generated.story.hairColor = new Color(0.960f, 0.766f, 0.226f, 1.000f);
            generated.story.skinWhiteness = 0.5118284f;
            generated.story.childhood = BackstoryDatabase.allBackstories["FireTender-1318961704"];
            generated.story.adulthood = BackstoryDatabase.allBackstories["LoreKeeper1770777935"];

            // Traits; if skills for some reason need to be filled out they also go here.
            var traits = generated.story.traits;
            traits.allTraits.Clear();
            traits.GainTrait(new Trait(TraitDefOf.Beauty, 2));
            traits.GainTrait(new Trait(TraitDefOf.Industriousness, 2));
            traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamed("Nerves"), 2));

            // Apparel
            var tribalwear = (Apparel)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Apparel_TribalA"),
                DefDatabase<ThingDef>.GetNamed("DevilstrandCloth"));
            tribalwear.GetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            generated.apparel.DestroyAll();
            generated.apparel.Wear(tribalwear);

            return generated;
        }
    }
}
