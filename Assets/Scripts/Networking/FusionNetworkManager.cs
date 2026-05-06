using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionNetworkManager Instance { get; private set; }

    [SerializeField] private NetworkRunner    runnerPrefab;
    [SerializeField] private int              defaultPlayerCount = 30;

    private NetworkRunner _runner;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void StartGame(GameMode mode, string sessionName, int playerCount = 0)
    {
        if (playerCount <= 0) playerCount = defaultPlayerCount;

        _runner = Instantiate(runnerPrefab);
        _runner.AddCallbacks(this);
        _runner.ProvideInput = true;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode    = mode,
            SessionName = sessionName,
            PlayerCount = playerCount,
            Scene       = SceneRef.FromIndex(1),
        });

        if (!result.Ok)
            Debug.LogError($"[Fusion] StartGame failed: {result.ShutdownReason}");
    }

    // --- INetworkRunnerCallbacks ---

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        TeamManager.Instance?.AssignAndSpawnPlayer(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        => ReconnectManager.Instance?.HandleDisconnect(player);

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData
        {
            MoveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            LookDelta     = new Vector2(Input.GetAxis("Mouse X"),       Input.GetAxis("Mouse Y")),
            Fire          = Input.GetButton("Fire1"),
            Aim           = Input.GetButton("Fire2"),
            Jump          = Input.GetButtonDown("Jump"),
            Reload        = Input.GetKeyDown(KeyCode.R),
            Sprint        = Input.GetKey(KeyCode.LeftShift),
            Grapple       = Input.GetKeyDown(KeyCode.E),
            SwitchWeapon  = Input.GetKeyDown(KeyCode.Tab),
            UseBandage    = Input.GetKeyDown(KeyCode.H),
            UseMedkit     = Input.GetKeyDown(KeyCode.J),
            Ping          = Input.GetKeyDown(KeyCode.Z),
        };
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason reason) { }
    public void OnConnectedToServer(NetworkRunner r) { }
    public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] token) { }
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessage msg) { }
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> sessions) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, ReliableKey k, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey k, float progress) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2     MoveDirection;
    public Vector2     LookDelta;
    public NetworkBool Fire;
    public NetworkBool Aim;
    public NetworkBool Jump;
    public NetworkBool Reload;
    public NetworkBool Sprint;
    public NetworkBool Grapple;
    public NetworkBool SwitchWeapon;
    public NetworkBool UseBandage;
    public NetworkBool UseMedkit;
    public NetworkBool Ping;
}
