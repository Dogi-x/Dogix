using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }

    [SerializeField] private List<Transform> attackerSpawns;
    [SerializeField] private List<Transform> defenderSpawns;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private readonly Dictionary<PlayerRef, TeamType> _playerTeams = new();
    private int _attackerIdx;
    private int _defenderIdx;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AssignAndSpawnPlayer(PlayerRef playerRef)
    {
        if (!HasStateAuthority) return;

        TeamType team = DetermineTeam();
        _playerTeams[playerRef] = team;

        Transform spawn = team == TeamType.Defender
            ? defenderSpawns[_defenderIdx++ % defenderSpawns.Count]
            : attackerSpawns[_attackerIdx++ % attackerSpawns.Count];

        var obj = Runner.Spawn(playerPrefab, spawn.position, spawn.rotation, playerRef);
        obj.GetComponent<PlayerHealth>()?.Initialize(team);
        MatchManager.Instance?.RegisterPlayer(playerRef, team);
    }

    // Defenders capped at ~40% of roster — they hold position advantage
    private TeamType DetermineTeam()
    {
        int defs  = 0;
        foreach (var kv in _playerTeams)
            if (kv.Value == TeamType.Defender) defs++;

        float ratio = (float)defs / Mathf.Max(1, _playerTeams.Count);
        return ratio < 0.40f ? TeamType.Defender : TeamType.Attacker;
    }

    public TeamType GetTeam(PlayerRef player)
        => _playerTeams.TryGetValue(player, out var t) ? t : TeamType.Attacker;

    public bool SameTeam(PlayerRef a, PlayerRef b)
        => GetTeam(a) == GetTeam(b);
}
