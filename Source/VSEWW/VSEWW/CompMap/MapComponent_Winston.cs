using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class MapComponent_Winston : MapComponent
    {
        public Vector2 counterPos;
        private bool counterDraggable = true;
        public Window_WaveCounter waveCounter = null;

        public NextRaidInfo nextRaidInfo;

        public int currentWave = 1;
        public float currentPoints = 0;
        private float modifierChance = 0;
        public bool nextRaidSendAllies = false;
        public float nextRaidMultiplyPoints = 1f;
        public int waveDelay = 0;

        public IntVec3 dropSpot = IntVec3.Invalid;

        private int tickUntilStatCheck = 0;
        private List<Pawn> statPawns = new List<Pawn>();
        const int checkEachXTicks = 2000;

        const int winstonTick = 50;
        private int nextTick = 100;

        private bool canReceiveWave = false;

        private bool preNewVersion = true;

        private bool ShouldRegenerateRaid
        {
            get
            {
                if (preNewVersion)
                {
                    preNewVersion = false;
                    Log.Warning("[VSEWW] pre new version");
                    return true;
                }

                if (nextRaidInfo == null)
                {
                    Log.Warning("[VSEWW] nextRaidInfo == null");
                    return true;
                }

                if (nextRaidInfo.parms.raidStrategy == null)
                {
                    Log.Warning("[VSEWW] raidStrategy == null");
                    return true;
                }

                if (nextRaidInfo.parms.faction == null)
                {
                    Log.Warning("[VSEWW] faction == null");
                    return true;
                }

                if (nextRaidInfo.totalPawnsBefore == 0)
                {
                    Log.Warning("[VSEWW] totalPawnsBefore == 0");
                    return true;
                }
                return false;
            }
        }

        public MapComponent_Winston(Map map) : base(map) { }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref currentWave, "currentWave");
            Scribe_Values.Look(ref currentPoints, "currentPoints");
            Scribe_Values.Look(ref modifierChance, "modifierChance");
            Scribe_Values.Look(ref nextRaidSendAllies, "nextRaidSendAllies");
            Scribe_Values.Look(ref nextRaidMultiplyPoints, "nextRaidMultiplyPoints");
            Scribe_Values.Look(ref waveDelay, "waveDelay");          
            Scribe_Values.Look(ref tickUntilStatCheck, "tickUntilStatCheck", 0);
            Scribe_Values.Look(ref counterDraggable, "counterDraggable");
            Scribe_Values.Look(ref counterPos, "counterPos");
            Scribe_Values.Look(ref preNewVersion, "preNewVersion", true);

            Scribe_Collections.Look(ref statPawns, "statPawns", LookMode.Reference);

            Scribe_Deep.Look(ref nextRaidInfo, "nextRaidInfo");
        }

        public override void FinalizeInit()
        {
            var mapParent = map.Parent;
            canReceiveWave = map.Biome.defName != "OuterSpaceBiome" && (mapParent == null || mapParent.def.canBePlayerHome) && map.ParentFaction == Faction.OfPlayer;
            nextTick = Find.TickManager.TicksGame + winstonTick;
        }

        public override void MapComponentTick()
        {
            var ticksGame = Find.TickManager.TicksGame;
            // Not ticking, cannot receive waves or if there is a window absorbing inputs
            if (ticksGame < nextTick || !canReceiveWave || Find.WindowStack.AnyWindowAbsorbingAllInput)
                return;
            // Increment next tick
            nextTick += winstonTick;

            var storyteller = Find.Storyteller;
            // If winston selected and not peaceful
            if (storyteller.def.defName == "VSE_WinstonWave" && storyteller.difficultyDef != InternalDefOf.Peaceful)
            {
                // If stats increase enabled
                if (WinstonMod.settings.enableStatIncrease)
                {
                    // Check
                    if (tickUntilStatCheck <= 0)
                    {
                        // Add
                        AddStatHediff();
                        tickUntilStatCheck = checkEachXTicks;
                    }
                    tickUntilStatCheck--;
                }
                // If next raid isn't set, or is bugged
                if (ShouldRegenerateRaid)
                {
                    nextRaidInfo = GetNextWave();
                    waveCounter?.UpdateWindow();
                    waveCounter?.WaveTip();
                }
                else
                {
                    // If raid isn't sent, but should be
                    if (!nextRaidInfo.sent && nextRaidInfo.atTick <= ticksGame)
                    {
                        StartRaid(ticksGame);
                    }
                    else if (nextRaidInfo.RaidOver())
                    {
                        // Show rewards window
                        Find.WindowStack.Add(new Window_ChooseReward(currentWave, FourthRewardChance(false), map));
                        
                    }
                    else if (nextRaidInfo.sent && Find.TickManager.TicksGame - nextRaidInfo.sentAt >= WinstonMod.settings.timeToDefeatWave * GenDate.TicksPerDay)
                    {
                        PrepareNextWave();
                    }
                }

                if (waveCounter != null)
                {
                    counterDraggable = waveCounter.draggable;
                    counterPos = new Vector2(UI.screenWidth - waveCounter.windowRect.xMax, waveCounter.windowRect.y);
                }
                // Manage counter visibility
                var currentMap = Find.CurrentMap;
                if (waveCounter == null && currentMap == map)
                {
                    waveCounter = new Window_WaveCounter(this, counterDraggable, counterPos);
                    Find.WindowStack.Add(waveCounter);
                    waveCounter.UpdateWindow();
                    waveCounter.WaveTip();
                }
                else if (waveCounter != null && currentMap != map)
                {
                    RemoveCounter();
                }
            }
            // If winston ins't selected or it's peaceful
            else
            {
                // Delay next raid
                if (nextRaidInfo != null)
                    nextRaidInfo.atTick++;
                // Remove all stats increase hediffs
                if (statPawns.NullOrEmpty())
                {
                    RemoveStatHediff();
                    tickUntilStatCheck = 0; // Instant stat back if switch storyteller
                }
                // Remove the counter
                if (waveCounter != null)
                    RemoveCounter();
            }
        }

        public float FourthRewardChance(bool now)
        {
            float ticksInAdvance = nextRaidInfo.atTick - (now ? Find.TickManager.TicksGame : nextRaidInfo.sentAt);
            float ticksInBetween = nextRaidInfo.atTick - nextRaidInfo.generatedAt;

            if (ticksInAdvance > 0) { // Sent early
                
                return ticksInAdvance / ticksInBetween;
            }
           
            return 0f;
        }

        /// <summary>
        /// Call functions and prepare the next wave
        /// </summary>
        internal void PrepareNextWave()
        {
            currentWave++;
            nextRaidInfo.StopIncidentModifiers();
            nextRaidInfo = GetNextWave();
            waveCounter?.UpdateWindow();
            waveCounter?.WaveTip();
        }

        /// <summary>
        /// Generate either a normal or a boss wave depending on the current wave number
        /// </summary>
        public NextRaidInfo GetNextWave()
        {
            var nextRaidInfo = new NextRaidInfo();
            nextRaidInfo.Init(currentWave, GetNextWavePoint(), map);
            nextRaidInfo.atTick += waveDelay * 60000;

            return nextRaidInfo;
        }

        /// <summary>
        /// Calculate new wave raid points
        /// </summary>
        internal float GetNextWavePoint()
        {
            //base points balanced over 280% threat scale option
            float basePoints = Find.Storyteller.difficulty.threatScale * 100f / 2.8f;
            if (WinstonMod.settings.linearThreatScale)
            {
                currentPoints = basePoints * (1 + currentWave <= 20 ?
                    WinstonMod.settings.pointMultiplierBefore * currentWave :
                    WinstonMod.settings.pointMultiplierBefore * 20 +
                    WinstonMod.settings.pointMultiplierAfter * (currentWave - 20));
            }
            else
            {
                currentPoints = basePoints * (currentWave <= 20 ? 
                    (float)Math.Pow(WinstonMod.settings.pointMultiplierBefore, currentWave) :
                    (float)Math.Pow(WinstonMod.settings.pointMultiplierBefore, 20) *
                    (float)Math.Pow(WinstonMod.settings.pointMultiplierAfter, currentWave - 20));
            }


                // Get point for this wave
                var point = (WinstonMod.settings.enableMaxPoint ? Mathf.Min(currentPoints, WinstonMod.settings.maxPoints) : currentPoints) * nextRaidMultiplyPoints;
            nextRaidMultiplyPoints = 1f;

            return point;
        }

        /// <summary>
        /// Start the raid
        /// </summary>
        internal void StartRaid(int ticks)
        {
            // Send raid
            nextRaidInfo.SendRaid(map, ticks);
            if (waveDelay != 0)
                waveDelay = 0;
            waveCounter?.UpdateWindow();
            // Send allies if necessary
            if (nextRaidSendAllies)
            {
                nextRaidSendAllies = false;
                // Create parms
                var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                parms.target = map;
                parms.faction = Find.FactionManager.RandomAlliedFaction();
                parms.points = Math.Min(nextRaidInfo.parms.points * 2, WinstonMod.settings.maxPoints);
                // Send raid
                IncidentDefOf.RaidFriendly.Worker.TryExecute(parms);
            }
        }

        /// <summary>
        /// Add hediff increasing stats to pawns
        /// </summary>
        internal void AddStatHediff()
        {
            if (statPawns == null)
                statPawns = new List<Pawn>();

            var pawns = map.mapPawns.FreeColonistsSpawned;
            pawns.AddRange(map.mapPawns.SlavesAndPrisonersOfColonySpawned);

            for (int i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn.Faction == Faction.OfPlayer && pawn.RaceProps.Humanlike && !statPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(WHediffDefOf.VESWW_IncreasedStats))
                {
                    pawn.health.AddHediff(WHediffDefOf.VESWW_IncreasedStats);
                    statPawns.Add(pawn);
                }
            }
        }

        /// <summary>
        /// Remove hediff increasing stats to pawns
        /// </summary>
        internal void RemoveStatHediff()
        {
            if (statPawns.NullOrEmpty())
                return;

            for (int i = 0; i < statPawns.Count; i++)
            {
                var pawn = statPawns[i];
                if (pawn.health?.hediffSet?.GetFirstHediffOfDef(WHediffDefOf.VESWW_IncreasedStats) is Hediff hediff)
                    pawn.health.RemoveHediff(hediff);
            }

            statPawns.Clear();
        }

        /// <summary>
        /// Register new drop spot
        /// </summary>
        internal void RegisterDropSpot(IntVec3 spot) => dropSpot = spot;

        /// <summary>
        /// Remove ounter from screen
        /// </summary>
        internal void RemoveCounter()
        {
            counterPos = new Vector2(UI.screenWidth - waveCounter.windowRect.xMax, waveCounter.windowRect.y);
            counterDraggable = waveCounter.draggable;
            waveCounter.Close();
            waveCounter = null;
        }
    }
}