using UnityEngine;
using Fusion;

public enum GamePhase { Waiting, Loot, Combat, Ended }
public enum TeamType { Attacker, Defender }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Networked] public GamePhase Phase { get; private set; }
    [Networked] private TickTimer LootTimer { get; set; }

    public const float LOOT_PHASE_DURATION = 60f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void Spawned()
    {
        if (!HasStateAuthority) return;
        Phase = GamePhase.Loot;
        LootTimer = TickTimer.CreateFromSeconds(Runner, LOOT_PHASE_DURATION);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (Phase == GamePhase.Loot && LootTimer.Expired(Runner))
        {
            Phase = GamePhase.Combat;
            ZoneManager.Instance?.ActivateZone();
            GateController.OpenAll();
        }
    }

    public void NotifyTeamWiped(TeamType team)
    {
        if (Phase != GamePhase.Combat) return;
        Phase = GamePhase.Ended;
        TeamType winner = team == TeamType.Attacker ? TeamType.Defender : TeamType.Attacker;
        RPC_MatchEnded(winner);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MatchEnded(TeamType winner)
    {
        ResultsScreen.Instance?.Show(winner);
    }

    public bool IsLootPhase  => Phase == GamePhase.Loot;
    public bool IsCombatPhase => Phase == GamePhase.Combat;
}
