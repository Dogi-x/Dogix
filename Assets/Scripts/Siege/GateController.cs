using UnityEngine;
using Fusion;
using System.Collections.Generic;

public enum GateType { Main, Secondary }

public class GateController : NetworkBehaviour
{
    private static readonly List<GateController> All = new();

    [SerializeField] private GateType   gateType  = GateType.Main;
    [SerializeField] private float      openChance = 0.25f;
    [SerializeField] private GameObject alarmIndicator;
    [SerializeField] private Animator   gateAnimator;

    [Networked] public float HP          { get; private set; }
    [Networked] public bool  IsOpen      { get; private set; }
    [Networked] public bool  IsDestroyed { get; private set; }
    [Networked] private bool         AlarmActive { get; set; }
    [Networked] private TickTimer    OpenTimer   { get; set; }

    private float MaxHP => gateType == GateType.Main ? 3000f : 1500f;

    public override void Spawned()
    {
        HP = MaxHP;
        All.Add(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
        => All.Remove(this);

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority || IsDestroyed || IsOpen) return;
        HP = Mathf.Max(0f, HP - damage);
        if (HP <= 0f) DestroyGate();
    }

    // Called when combat phase begins — 25% chance to alarm + open
    public void CheckRandomOpen()
    {
        if (!HasStateAuthority || IsDestroyed || IsOpen) return;
        if (UnityEngine.Random.value <= openChance) TriggerAlarmAndOpen();
    }

    private void TriggerAlarmAndOpen()
    {
        AlarmActive = true;
        float delay = UnityEngine.Random.Range(5f, 10f);
        OpenTimer   = TickTimer.CreateFromSeconds(Runner, delay);
        RPC_Alarm();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || IsOpen || IsDestroyed) return;
        if (AlarmActive && OpenTimer.Expired(Runner)) OpenGate();
    }

    private void OpenGate()
    {
        IsOpen      = true;
        AlarmActive = false;
        RPC_Open();
    }

    private void DestroyGate()
    {
        IsDestroyed = true;
        IsOpen      = true;
        RPC_Destroy();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Alarm()
    {
        if (alarmIndicator != null) alarmIndicator.SetActive(true);
        AudioManager.Instance?.PlayAlarm();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Open()
    {
        if (alarmIndicator != null) alarmIndicator.SetActive(false);
        gateAnimator?.SetTrigger("Open");
        AudioManager.Instance?.PlayGateOpen();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Destroy()
    {
        gateAnimator?.SetTrigger("Destroy");
        AudioManager.Instance?.PlayExplosionAt(transform.position);
    }

    // Trigger all gates to roll their random-open chance
    public static void OpenAll()
    {
        foreach (var gate in All) gate.CheckRandomOpen();
    }
}
