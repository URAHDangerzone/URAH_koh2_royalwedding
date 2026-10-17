using System;
using BepInEx;
using BepInEx.Logging;
using URAH;

namespace URAH_RoyalWedding
{
    /// <summary>
    /// Royal wedding feast: an unmarried king can pay for a feast at court to find a wife among his own
    /// nobles, with a success chance like the game's other actions. Needs the URAH_RoyalWedding data
    /// mod (the action's def). The patches are only applied while that mod is active (its
    /// bepinex_plugins.txt names this plugin, see ModLink).
    /// </summary>
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "urah.koh2.royalwedding";
        public const string Name = "URAH_RoyalWedding";
        public const string Version = URAH.BuildVersion.Value; // plugin/version.txt

        internal static ManualLogSource Log;

        private ModLink link;
        private bool heartbeatFailed;

        private void Awake()
        {
            Log = Logger;
            link = new ModLink(Guid, typeof(Plugin).Assembly, Log);
            Log.LogInfo($"{Name} {Version} loaded");
        }

        private void Update()
        {
            if (heartbeatFailed || !link.Active)
            {
                return;
            }
            try
            {
                Heartbeat.Update(GameLogic.Get(create: false));
            }
            catch (Exception e)
            {
                Log.LogError($"Royal wedding heartbeat failed: {e}");
                heartbeatFailed = true;
            }
        }
    }
}
