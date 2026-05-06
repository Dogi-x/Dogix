using UnityEngine;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    [SerializeField] private Camera          minimapCamera;
    [SerializeField] private RectTransform   zoneCircleUI;
    [SerializeField] private float           mapWorldSize  = 1000f;
    [SerializeField] private float           minimapUISize = 200f;
    [SerializeField] private GameObject      pingIconPrefab;
    [SerializeField] private Transform       pingContainer;

    private Transform _localPlayer;

    public void Initialize(Transform localPlayer) => _localPlayer = localPlayer;

    private void LateUpdate()
    {
        if (_localPlayer != null && minimapCamera != null)
        {
            minimapCamera.transform.position = new Vector3(
                _localPlayer.position.x,
                minimapCamera.transform.position.y,
                _localPlayer.position.z);
        }
        UpdateZoneCircle();
    }

    private void UpdateZoneCircle()
    {
        if (ZoneManager.Instance == null || zoneCircleUI == null) return;

        float worldRadius = ZoneManager.Instance.CurrentRadius;
        float uiDiameter  = (worldRadius / mapWorldSize) * minimapUISize * 2f;
        zoneCircleUI.sizeDelta = new Vector2(uiDiameter, uiDiameter);

        Vector3 center   = ZoneManager.Instance.ZoneCenter;
        zoneCircleUI.anchoredPosition = new Vector2(
            (center.x / mapWorldSize) * minimapUISize,
            (center.z / mapWorldSize) * minimapUISize);
    }

    public void ShowPing(Vector3 worldPos, PingType type)
    {
        if (pingIconPrefab == null || pingContainer == null) return;
        var icon = Instantiate(pingIconPrefab, pingContainer);
        icon.GetComponent<RectTransform>().anchoredPosition = WorldToMinimap(worldPos);
        icon.GetComponent<PingIcon>()?.Setup(type);
        Destroy(icon, 10f);
    }

    private Vector2 WorldToMinimap(Vector3 world)
        => new Vector2(
            (world.x / mapWorldSize) * minimapUISize,
            (world.z / mapWorldSize) * minimapUISize);
}
