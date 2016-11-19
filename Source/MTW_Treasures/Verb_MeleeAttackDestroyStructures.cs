using Verse;
using RimWorld;

namespace MTW_Treasures
{
    class Verb_MeleeAttackDestroyStructures : Verb_MeleeAttack
    {
        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                Thing target = this.currentTarget.Thing;
                if (!(target is Pawn))
                {
                    target.Destroy(DestroyMode.Kill);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
