using UnityEngine;
using Fusion;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Networked, Capacity(50)] private NetworkLinkedList<PlayerRef> Attackers { get; }
    [Networked, Capacity(50)] private NetworkLinkedList<PlayerRef> Defenders { get; }
    [Networked] public int AliveAttackers { get; private set; }
    [Networked] public int AliveDefenders { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterPlayer(PlayerRef player, TeamType team)
    {
        if (!HasStateAuthority) return;
        if (team == TeamType.Attacker) { Attackers.Add(player); AliveAttackers++; }
        else { Defenders.Add(player); AliveDefenders++; }
    }

    public void PlayerDied(PlayerRef player, TeamType team)
    {
        if (!HasStateAuthority) return;

        if (team == TeamType.Attacker)
        {
            AliveAttackers = Mathf.Max(0, AliveAttackers - 1);
            if (AliveAttackers == 0) GameManager.Instance?.NotifyTeamWiped(TeamType.Attacker);
        }
        else
        {
            AliveDefenders = Mathf.Max(0, AliveDefenders - 1);
            if (AliveDefenders == 0) GameManager.Instance?.NotifyTeamWiped(TeamType.Defender);
        }
    }
}
