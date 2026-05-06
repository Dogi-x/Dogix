using UnityEngine;
using Fusion;

[RequireComponent(typeof(CharacterController), typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed   = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight  = 1.5f;
    [SerializeField] private float gravity     = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float cameraPitchMin   = -60f;
    [SerializeField] private float cameraPitchMax   =  70f;

    [Header("ADS")]
    [SerializeField] private float aimAssistStrength = 0.25f;

    [Networked] public NetworkBool IsAiming    { get; private set; }
    [Networked] public NetworkBool IsSprinting { get; private set; }
    [Networked] private float NetworkYaw   { get; set; }
    [Networked] private float NetworkPitch { get; set; }

    private CharacterController _cc;
    private Vector3 _velocity;
    private float   _yaw;
    private float   _pitch;

    private PlayerHealth    _health;
    private PlayerInventory _inventory;
    private GrappleController _grapple;
    private HealingSystem   _healing;

    // Maximum speed multiplier allowed before anti-cheat flags
    private const float SPEED_MARGIN = 1.25f;

    public override void Spawned()
    {
        _cc        = GetComponent<CharacterController>();
        _health    = GetComponent<PlayerHealth>();
        _inventory = GetComponent<PlayerInventory>();
        _grapple   = GetComponent<GrappleController>();
        _healing   = GetComponent<HealingSystem>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (_health != null && (_health.IsKnocked || _health.IsDead)) return;

        if (GetInput(out NetworkInputData input))
        {
            HandleMovement(input);
            HandleAiming(input);
            HandleWeapon(input);
            HandleGrapple(input);
            HandleHealing(input);
            HandlePing(input);
        }

        HandleZoneDamage();
    }

    private void HandleMovement(NetworkInputData input)
    {
        Vector3 move = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);
        move = transform.TransformDirection(move);

        bool sprinting = input.Sprint && !IsAiming && move.magnitude > 0.1f;
        IsSprinting = sprinting;
        float speed  = sprinting ? sprintSpeed : moveSpeed;

        if (_cc.isGrounded)
        {
            _velocity.y = -2f;
            if (input.Jump) _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        _velocity.y += gravity * Runner.DeltaTime;

        Vector3 horizontalMove = move * speed * Runner.DeltaTime;
        float maxHoriz = speed * SPEED_MARGIN * Runner.DeltaTime;
        if (horizontalMove.magnitude > maxHoriz)
            horizontalMove = horizontalMove.normalized * maxHoriz;

        _cc.Move(horizontalMove + Vector3.up * _velocity.y * Runner.DeltaTime);

        // Look
        _yaw   += input.LookDelta.x * mouseSensitivity;
        _pitch  = Mathf.Clamp(_pitch - input.LookDelta.y * mouseSensitivity, cameraPitchMin, cameraPitchMax);

        transform.rotation = Quaternion.Euler(0, _yaw, 0);
        if (cameraTarget != null) cameraTarget.localRotation = Quaternion.Euler(_pitch, 0, 0);

        NetworkYaw   = _yaw;
        NetworkPitch = _pitch;
    }

    private void HandleAiming(NetworkInputData input)
    {
        IsAiming = input.Aim && !IsSprinting;
    }

    private void HandleWeapon(NetworkInputData input)
    {
        if (_inventory == null) return;
        WeaponBase weapon = _inventory.GetActiveWeapon();
        if (weapon == null) return;

        if (input.Fire && !IsSprinting)
            weapon.TryFire(IsAiming, aimAssistStrength);

        if (input.Reload)
            weapon.TryReload();

        if (input.SwitchWeapon)
            _inventory.SwitchWeapon();
    }

    private void HandleGrapple(NetworkInputData input)
    {
        if (input.Grapple) _grapple?.TryGrapple();
    }

    private void HandleHealing(NetworkInputData input)
    {
        if (input.UseBandage)  _healing?.TryHeal(HealItemType.Bandage);
        if (input.UseMedkit)   _healing?.TryHeal(HealItemType.Medkit);
    }

    private void HandlePing(NetworkInputData input)
    {
        if (input.Ping) GetComponent<PingSystem>()?.SendPing();
    }

    private void HandleZoneDamage()
    {
        if (ZoneManager.Instance == null) return;
        if (!ZoneManager.Instance.IsOutsideZone(transform.position)) return;

        float damage = ZoneManager.Instance.GetCurrentDamageRate() * Runner.DeltaTime;
        _health?.TakeDamage(damage, DamageSource.Zone);
    }

    public override void Render()
    {
        if (HasStateAuthority) return;
        transform.rotation = Quaternion.Euler(0, NetworkYaw, 0);
        if (cameraTarget != null) cameraTarget.localRotation = Quaternion.Euler(NetworkPitch, 0, 0);
    }
}
