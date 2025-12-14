using RimWorld;
using Verse;

namespace VSEWW
{
    public class Hediff_Boom : HediffWithComps
    {
        public override void Notify_PawnKilled()
        {
            if (pawn.Faction != Faction.OfPlayer && pawn.Map != null)
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 2.9f, DamageDefOf.Bomb, pawn);

            base.Notify_PawnKilled();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (pawn.Faction != Faction.OfPlayer && pawn.Map != null)
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 2.9f, DamageDefOf.Bomb, pawn);

            base.Notify_PawnDied(dinfo, culprit);
        }
    }
}