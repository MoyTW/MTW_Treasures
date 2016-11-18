using System;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace MTW_Treasures
{
    enum BannerState
    {
        Unused=1,
        WieldedSafe,
        WieldedDanger,
        DroppedDanger
    }

    class Treasure_HumaktsRavenBanner : Apparel
    {
        private Pawn lastBearer;
        private BannerState bannerState;

        private bool EnemiesPresent
        {
            get
            {
                int numHostiles = Find.AttackTargetsCache.TargetsHostileToColony
                    .Count((IAttackTarget x) => !x.ThreatDisabled());
                return numHostiles > 0;
            }
        }

        private void KillFormerBearer()
        {
            if (this.lastBearer == null)
            {
                Log.Warning("Humakt's Raven Banner tried to kill a null bearer!");
                return;
            }
            if (this.lastBearer.corpse != null)
            {
                Log.Warning("Humakt's Raven Banner can't kill bearer " + lastBearer.Label + ", bearer is already dead!");
                this.lastBearer = null;
                return;
            }

            Messages.Message("Once hoisted, Humakt's Raven Banner cannot be laid down without its due in death!",
                this.lastBearer, MessageSound.Negative);

            int cuts = 0;
            while (this.lastBearer.corpse == null && cuts < 100)
            {
                BodyPartDamageInfo value = new BodyPartDamageInfo(null, new BodyPartDepth?(BodyPartDepth.Inside));
                var dinfo = new DamageInfo(DamageDefOf.ExecutionCut, 9999, this, value, null);
                this.lastBearer.TakeDamage(dinfo);
                cuts++;
            }

            if (this.lastBearer.corpse == null)
            {
                Log.Error("Humakt's Raven Banner could not execute " + this.lastBearer.Label + "!");
            }

            this.lastBearer = null;
        }

        private void fromUnusedState()
        {
            if (this.wearer != null)
            {
                this.lastBearer = this.wearer;

                if (this.EnemiesPresent)
                {
                    this.bannerState = BannerState.WieldedDanger;
                }
                else
                {
                    this.bannerState = BannerState.WieldedSafe;
                }
            }
        }

        private void fromWieldedSafeState()
        {
            if (this.wearer == null)
            {
                this.KillFormerBearer();
                this.bannerState = BannerState.Unused;
            }
            else if (this.EnemiesPresent)
            {
                this.bannerState = BannerState.WieldedDanger;
            }
        }

        private void fromWieldedDangerState()
        {
            if (!this.EnemiesPresent)
            {
                this.KillFormerBearer();
                this.bannerState = BannerState.Unused;
            }
            else if (this.wearer == null && this.lastBearer.corpse != null)
            {
                this.KillFormerBearer();
                this.bannerState = BannerState.Unused;
            }
            else if (this.wearer == null)
            {
                this.lastBearer = null;
                this.bannerState = BannerState.DroppedDanger;
            }
        }

        private void fromDroppedDangerState()
        {
            if (!this.EnemiesPresent)
            {
                this.bannerState = BannerState.Unused;
            }
            else if (this.wearer != null)
            {
                this.lastBearer = this.wearer;
                this.bannerState = BannerState.WieldedDanger;
            }
        }

        public override void Tick()
        {
            base.Tick();

            switch (this.bannerState)
            {
                case BannerState.Unused: this.fromUnusedState(); break;
                case BannerState.WieldedSafe: this.fromWieldedSafeState(); break;
                case BannerState.WieldedDanger: this.fromWieldedDangerState(); break;
                case BannerState.DroppedDanger: this.fromDroppedDangerState(); break;
                default: throw new ArgumentOutOfRangeException("State " + this.bannerState + " is not a valid state!");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<Pawn>(ref this.lastBearer, "bearer", false);
            Scribe_Values.LookValue<BannerState>(ref this.bannerState, "bannerState", BannerState.Unused);
        }
    }
}
