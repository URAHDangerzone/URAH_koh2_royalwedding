using HarmonyLib;
using Logic;

namespace URAH_RoyalWedding
{
    using Action = Logic.Action;
    using Object = Logic.Object;

    /// <summary>
    /// The game finds the C# class of an action def with Type.GetType("Logic." + key) in
    /// Def.ResolveObjType, which never looks into a plugin assembly, so the wedding def resolves to the
    /// plain Action of its KingdomAction template. This creates the plugin's class for it instead.
    /// Never patch Def.ResolveObjType itself: Type.GetType without an assembly searches the calling
    /// assembly, and a patched method no longer runs from Assembly-CSharp-firstpass, so every other
    /// def would lose its class too (the campaign then fails to load).
    /// </summary>
    [HarmonyPatch(typeof(Action), nameof(Action.Create))]
    internal static class ActionCreatePatch
    {
        private static bool Prefix(Object owner, Action.Def def, ref Action __result)
        {
            if (def?.field?.key != RoyalWeddingAction.DefKey || !(owner is Logic.Kingdom kingdom))
            {
                return true;
            }
            __result = new RoyalWeddingAction(kingdom, def);
            return false;
        }
    }

    /// <summary>
    /// Kingdom.OnTimer warns "Unhandled timer" for every timer it has no case for. The wedding's
    /// cooldown and heartbeat timers need no handling: they only exist to be found or to expire.
    /// </summary>
    [HarmonyPatch(typeof(Logic.Kingdom), nameof(Logic.Kingdom.OnTimer))]
    internal static class KingdomTimerPatch
    {
        private static bool Prefix(Timer timer)
        {
            return timer.name != RoyalWeddingAction.CooldownTimer && timer.name != Heartbeat.TimerName;
        }
    }
}
