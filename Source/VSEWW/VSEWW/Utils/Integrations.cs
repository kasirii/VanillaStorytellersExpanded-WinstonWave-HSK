using Verse;
using RimWorld;
using System;
using System.Reflection;

namespace VSEWW
{
    public static class IgnoranceIntegration
    {
        private static bool initialized;
        private static MethodInfo factionInEligibleTechRangeMethod;

        public static bool IsFactionAllowed(Faction faction)
        {
            if (!initialized)
                TryInit();

            if (factionInEligibleTechRangeMethod == null)
                return true;

            try
            {
                return (bool)factionInEligibleTechRangeMethod.Invoke(null, new object[] { faction });
            }
            catch (Exception e)
            {
                Log.Error($"[VSEWW] Ignorance FactionInEligibleTechRange failed: {e}");
                return true;
            }
        }

        private static void TryInit()
        {
            initialized = true;

            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type ignoranceBaseType =
                        asm.GetType("DIgnoranceIsBliss.Core_Patches.IgnoranceBase");

                    if (ignoranceBaseType == null)
                        continue;

                    factionInEligibleTechRangeMethod =
                        ignoranceBaseType.GetMethod(
                            "FactionInEligibleTechRange",
                            BindingFlags.Public | BindingFlags.Static
                        );

                    if (factionInEligibleTechRangeMethod != null)
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[VSEWW] Ignorance integration init failed: {e}");
            }
        }
    }

    public static class LayeredAtmosphereOrbitIntegration
    {
        public static Type ExtensionType =
            GenTypes.GetTypeInAnyAssembly(
                "LayeredAtmosphereOrbit.LayeredAtmosphereOrbitDefModExtension");

        public static readonly FieldInfo MinArrivalFactionTechLevel =
            ExtensionType?.GetField(
                "minArrivalFactionTechLevel",
                BindingFlags.Instance | BindingFlags.Public);

        public static readonly FieldInfo MaxArrivalFactionTechLevel =
            ExtensionType?.GetField(
                "maxArrivalFactionTechLevel",
                BindingFlags.Instance | BindingFlags.Public);

        public static readonly FieldInfo WhitelistArrivalFactionDef =
            ExtensionType?.GetField(
                "WhitelistArrivalFactionDef",
                BindingFlags.Instance | BindingFlags.Public);

        public static readonly FieldInfo BlacklistArrivalFactionDef =
            ExtensionType?.GetField(
                "BlacklistArrivalFactionDef",
                BindingFlags.Instance | BindingFlags.Public);

        public static bool LAOActive => ExtensionType != null;


    }
}
