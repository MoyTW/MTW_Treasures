using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace MTW_Treasures
{
    class Treasure_HumaktsRavenBanner : Apparel
    {
        private Pawn bearer;

        private void KillFormerBearer()
        {
            if (this.bearer == null)
            {
                Log.Warning("Humakt's Raven Banner tried to kill a null bearer!");
                return;
            }
            if (this.bearer.corpse != null)
            {
                Log.Warning("Humakt's Raven Banner can't kill bearer " + bearer.Label + ", bearer is already dead!");
                this.bearer = null;
                return;
            }

            Messages.Message("Once hoisted, Humakt's Raven Banner cannot be laid down without its due in death!",
                this.bearer, MessageSound.Negative);

            int cuts = 0;
            while (this.bearer.corpse == null && cuts < 100)
            {
                BodyPartDamageInfo value = new BodyPartDamageInfo(null, new BodyPartDepth?(BodyPartDepth.Inside));
                var dinfo = new DamageInfo(DamageDefOf.ExecutionCut, 9999, this, value, null);
                this.bearer.TakeDamage(dinfo);
                cuts++;
            }

            if (this.bearer.corpse == null)
            {
                Log.Error("Humakt's Raven Banner could not execute " + this.bearer.Label + "!");
            }

            this.bearer = null;
        }

        public override void Tick()
        {
            base.Tick();

            // If banner was picked up
            if (this.wearer != null && this.bearer == null)
            {
                this.bearer = this.wearer;
            }
            // If banner was dropped
            else if (this.wearer == null && this.bearer != null)
            {
                this.KillFormerBearer();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<Pawn>(ref this.bearer, "bearer", false);
        }
    }
}
