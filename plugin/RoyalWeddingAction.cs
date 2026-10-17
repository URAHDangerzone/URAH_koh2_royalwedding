using Logic;

namespace URAH_RoyalWedding
{
    using Action = Logic.Action;
    using Character = Logic.Character;
    using Kingdom = Logic.Kingdom;
    using RoyalFamily = Logic.RoyalFamily;

    /// <summary>
    /// Code for the URAH_RoyalWeddingAction kingdom action (URAH_RoyalWedding/Defs). Cost, duration,
    /// success chance and messages come from the def; this adds the checks, the wedding on success and
    /// the cooldown after a failure. The game finds action classes with Type.GetType("Logic." + key),
    /// which never looks into a plugin assembly, so <see cref="ActionCreatePatch"/> creates this class.
    ///
    /// Multiplayer: the action runs on the host (a guest's click arrives as an ExecuteActionEvent and is
    /// validated again there). A host without the plugin creates a plain Action for the def, which does
    /// not know the urah_royal_wedding var the def's condition asks for, so it refuses before taking
    /// gold. Guests only show the action while the host's heartbeat (<see cref="Heartbeat"/>) is fresh.
    /// </summary>
    public class RoyalWeddingAction : Action
    {
        public const string DefKey = "URAH_RoyalWeddingAction";
        public const string CooldownTimer = "urah_royal_wedding_cooldown";

        public RoyalWeddingAction(Kingdom owner, Def def)
            : base(owner, def)
        {
        }

        public new static Action Create(Object owner, Def def)
        {
            return new RoyalWeddingAction(owner as Kingdom, def);
        }

        public override string Validate(bool quick_out = false)
        {
            if (is_active)
            {
                return base.Validate(quick_out);
            }
            Kingdom kingdom = own_kingdom;
            RoyalFamily family = kingdom?.royalFamily;
            Character king = family?.Sovereign;
            if (king == null || !king.IsAlive())
            {
                return "no_king";
            }
            if (family.Spouse != null && !family.Spouse.IsDead())
            {
                return "married";
            }
            if (!king.CanMarry() || kingdom == game.religions?.catholic?.hq_kingdom)
            {
                return "cannot_marry";
            }
            if (!kingdom.IsAuthority() && !Heartbeat.IsHostAlive(kingdom))
            {
                return "host_without_plugin";
            }
            if (king.prison_kingdom != null)
            {
                return "_king_imprisoned";
            }
            if (king.GetArmy()?.battle != null)
            {
                return "_king_in_battle";
            }
            if (kingdom.court.Find(c => c?.FindStatus<SearchingForSpouseStatus>()?.character == king) != null)
            {
                return "_searching";
            }
            if (Timer.Find(kingdom, CooldownTimer) != null)
            {
                return "_cooldown";
            }
            if (!CheckCost(null))
            {
                return "_cost";
            }
            return base.Validate(quick_out);
        }

        public override void Run()
        {
            Kingdom kingdom = own_kingdom;
            Character king = kingdom?.royalFamily?.Sovereign;
            Character spouse = kingdom?.royalFamily?.Spouse;
            if (king == null || !king.IsAlive() || (spouse != null && !spouse.IsDead()))
            {
                return;
            }
            // the same bride the game's own random marriage (RoyalFamily.CheckForMarrage) creates
            CharacterFactory.CreateQueen(kingdom, king);
        }

        public override bool ApplyOutcome(OutcomeDef outcome)
        {
            if (outcome.key == "fail" && own_kingdom != null)
            {
                Timer.Start(own_kingdom, CooldownTimer, def.field.GetFloat("fail_cooldown"), restart: true);
            }
            return base.ApplyOutcome(outcome);
        }

        public override Value GetVar(string key, IVars vars = null, bool as_value = true)
        {
            switch (key)
            {
            case "urah_royal_wedding":
                return true;
            case "court_knights":
                return CourtKnights();
            default:
                return base.GetVar(key, vars, as_value);
            }
        }

        /// <summary>Living members of the royal court, not counting the king himself.</summary>
        private int CourtKnights()
        {
            Kingdom kingdom = own_kingdom;
            if (kingdom?.court == null)
            {
                return 0;
            }
            Character king = kingdom.royalFamily?.Sovereign;
            int n = 0;
            foreach (Character c in kingdom.court)
            {
                if (c != null && !c.IsDead() && c != king)
                {
                    n++;
                }
            }
            return n;
        }
    }
}
