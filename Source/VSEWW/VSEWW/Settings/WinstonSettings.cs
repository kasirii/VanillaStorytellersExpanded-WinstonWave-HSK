using System.Collections.Generic;
using Verse;

namespace VSEWW
{
    public class WinstonSettings : ModSettings
    {
        public bool enableMaxPoint = true;
        public bool enableGraceDelay = true;
        public bool earliestRaidCheck = true;
        public float dayMultiplier = 1f;
        public bool linearThreatScale = false;
        public bool noWaterRaid = true;
        public int maxPoints = 60000;
        public float timeBeforeFirstWave = 5f;
        public float timeBetweenWaves = 3.5f;
        public float timeToDefeatWave = 3f;
        public float pointMultiplierBefore = 1.2f;
        public float pointMultiplierAfter = 1.071f;
        public bool enableStatIncrease = true;
        public bool drawBackground = false;
        public bool mysteryMod = false;
        public bool randomRewardMod = false;
        public bool hideToggleDraggable = false;
        public bool showPawnList = true;
        public bool dropSlagChunk = true;
        public bool rewardSettingsChanged = false;


        public List<string> modifierDefs = new List<string>();
        public List<string> rewardDefs = new List<string>();
        public List<string> excludedFactionDefs = new List<string>();
        public List<string> excludedStrategyDefs = new List<string>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableMaxPoint, "enableMaxPoint", true);
            Scribe_Values.Look(ref enableGraceDelay, "enableGraceDelay", true);
            Scribe_Values.Look(ref earliestRaidCheck, "earliestRaidCheck", true);
            Scribe_Values.Look(ref dayMultiplier, "dayMultiplier", 1f);
            Scribe_Values.Look(ref linearThreatScale, "linearThreatScale", false);
            Scribe_Values.Look(ref noWaterRaid, "noWaterRaid", true);
            Scribe_Values.Look(ref maxPoints, "maxPoints", 60000);
            Scribe_Values.Look(ref timeBeforeFirstWave, "timeBeforeFirstWave", 5f);
            Scribe_Values.Look(ref timeBetweenWaves, "timeBetweenWaves", 3.5f);
            Scribe_Values.Look(ref timeToDefeatWave, "timeToDefeatWave", 3f);
            Scribe_Values.Look(ref pointMultiplierBefore, "pointMultiplierBefore", 1.2f);
            Scribe_Values.Look(ref pointMultiplierAfter, "pointMultiplierAfter", 1.071f);
            Scribe_Values.Look(ref enableStatIncrease, "enableStatIncrease", true);
            Scribe_Values.Look(ref drawBackground, "drawBackground", false);
            Scribe_Values.Look(ref mysteryMod, "mysteryMod", false);
            Scribe_Values.Look(ref randomRewardMod, "randomRewardMod", false);
            Scribe_Values.Look(ref hideToggleDraggable, "hideToggleDraggable", false);
            Scribe_Values.Look(ref showPawnList, "showPawnList", true);
            Scribe_Values.Look(ref dropSlagChunk, "dropSlagChunk", true);
            Scribe_Collections.Look(ref modifierDefs, "modifierDefs", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref rewardDefs, "rewardDefs", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref excludedFactionDefs, "excludedFactionDefs", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref excludedStrategyDefs, "excludedStrategyDefs", LookMode.Value, new List<string>());
        }
    }
}