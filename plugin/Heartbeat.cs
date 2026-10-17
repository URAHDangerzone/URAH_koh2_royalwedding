using Logic;

namespace URAH_RoyalWedding
{
    using Kingdom = Logic.Kingdom;

    /// <summary>
    /// Tells guests whether the host runs this plugin. The host restarts a timer on every player
    /// kingdom once a minute; timers are synced to guests with their elapsed time (Object.TimersState),
    /// so a guest sees a recently restarted timer only while a host with the plugin keeps it fresh.
    /// A timer left over in a save goes stale within <see cref="MaxAge"/> seconds of loading it on a
    /// host without the plugin, and until then the def's condition still keeps that host from running
    /// the action.
    /// </summary>
    internal static class Heartbeat
    {
        public const string TimerName = "urah_royal_wedding_host";
        private const float Interval = 60f;
        private const float MaxAge = 150f;
        private const float Never = 1e9f;

        private static Time lastBeat = Time.Zero;

        /// <summary>Called every frame; restarts the timers on the host once per interval.</summary>
        public static void Update(Game game)
        {
            if (game?.kingdoms == null || !game.IsAuthority())
            {
                return;
            }
            if (lastBeat != Time.Zero && game.time >= lastBeat && game.time - lastBeat < Interval)
            {
                return;
            }
            lastBeat = game.time;
            foreach (Kingdom kingdom in game.kingdoms)
            {
                if (kingdom != null && kingdom.is_player && kingdom.IsAuthority())
                {
                    Timer.Start(kingdom, TimerName, Never, restart: true);
                }
            }
        }

        public static bool IsHostAlive(Kingdom kingdom)
        {
            Timer timer = Timer.Find(kingdom, TimerName);
            return timer != null && timer.Elapsed() < MaxAge;
        }
    }
}
