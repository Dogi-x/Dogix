using UnityEngine;
using Fusion;

public class GrappleController : NetworkBehaviour
{
    [SerializeField] private float      grappleRange   = 20f;
    [SerializeField] private float      climbDuration  = 4f;    // 3-5 seconds exposed
    [SerializeField] private LayerMask  ropePointMask;
    [SerializeField] private LineRenderer ropeRenderer;

    [Networked] public bool    IsClimbing     { get; private set; }
    [Networked] private Vector3 TargetPosition { get; set; }
    [Networked] private TickTimer ClimbTimer  { get; set; }

    private PlayerHealth      _health;
    private CharacterController _cc;
    private Vector3           _startPosition;

    public override void Spawned()
    {
        _health = GetComponent<PlayerHealth>();
        _cc     = GetComponent<CharacterController>();
    }

    public void TryGrapple()
    {
        if (!HasStateAuthority || IsClimbing) return;
        if (_health != null && (_health.IsKnocked || _health.IsDead)) return;

        Collider[] candidates = Physics.OverlapSphere(transform.position, grappleRange, ropePointMask);
        if (candidates.Length == 0) return;

        float   minDist = float.MaxValue;
        Vector3 target  = Vector3.zero;
        foreach (var c in candidates)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < minDist) { minDist = d; target = c.transform.position; }
        }

        IsClimbing     = true;
        TargetPosition = target;
        _startPosition = transform.position;
        ClimbTimer     = TickTimer.CreateFromSeconds(Runner, climbDuration);
        RPC_ShowRope(transform.position, target);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsClimbing) return;

        if (ClimbTimer.Expired(Runner))
        {
            IsClimbing = false;
            RPC_HideRope();
            return;
        }

        float elapsed = climbDuration - (ClimbTimer.RemainingTime(Runner) ?? 0f);
        float t       = Mathf.Clamp01(elapsed / climbDuration);
        Vector3 newPos = Vector3.Lerp(_startPosition, TargetPosition, t);
        _cc?.Move(newPos - transform.position);
    }

    public void Interrupt()
    {
        if (!HasStateAuthority || !IsClimbing) return;
        IsClimbing = false;
        RPC_HideRope();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowRope(Vector3 from, Vector3 to)
    {
        if (ropeRenderer == null) return;
        ropeRenderer.enabled    = true;
        ropeRenderer.SetPosition(0, from);
        ropeRenderer.SetPosition(1, to);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideRope()
    {
        if (ropeRenderer != null) ropeRenderer.enabled = false;
    }
}
