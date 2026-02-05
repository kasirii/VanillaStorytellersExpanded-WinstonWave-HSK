using RimWorld.Planet;
using Verse;

namespace VSEWW
{
    internal class WorldComponent_VSEWW : WorldComponent
    {
        public int totalKill = 0;
        public int currentWave;
        public bool nextRaidSendAllies;
        public float nextRaidMultiplyPoints;
        public int remainTicks;
        public bool shouldRestore;

        public WorldComponent_VSEWW(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalKill, "totalKill", 0, true);
            Scribe_Values.Look(ref currentWave, "currentWave");
            Scribe_Values.Look(ref nextRaidSendAllies, "nextRaidSendAllies");
            Scribe_Values.Look(ref nextRaidMultiplyPoints, "nextRaidMultiplyPoints");
            Scribe_Values.Look(ref remainTicks, "remainTicks");
            Scribe_Values.Look(ref shouldRestore, "shouldRestore");
        }

        public void IncrementCounter() => totalKill++;
    }
}