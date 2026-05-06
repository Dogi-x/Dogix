using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Health / Armor")]
    [SerializeField] private Slider           healthBar;
    [SerializeField] private TextMeshProUGUI  healthText;
    [SerializeField] private Slider           armorBar;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI  ammoText;
    [SerializeField] private TextMeshProUGUI  reserveText;
    [SerializeField] private TextMeshProUGUI  weaponNameText;

    [Header("Zone")]
    [SerializeField] private TextMeshProUGUI  zoneText;

    [Header("Team counts")]
    [SerializeField] private TextMeshProUGUI  attackersText;
    [SerializeField] private TextMeshProUGUI  defendersText;

    [Header("Feedback")]
    [SerializeField] private Image            hitMarkerImage;
    [SerializeField] private float            hitMarkerDuration = 0.15f;
    [SerializeField] private Image            screenFlashImage;

    [Header("Healing bar")]
    [SerializeField] private Slider           healProgressBar;
    [SerializeField] private GameObject       healBarRoot;

    private Coroutine _hitMarkerRoutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (hitMarkerImage  != null) hitMarkerImage.enabled  = false;
        if (screenFlashImage != null) screenFlashImage.enabled = false;
        if (healBarRoot      != null) healBarRoot.SetActive(false);
    }

    public void UpdateHealth(float hp, float maxHp)
    {
        if (healthBar  != null) healthBar.value  = hp / maxHp;
        if (healthText != null) healthText.text  = Mathf.CeilToInt(hp).ToString();
    }

    public void UpdateArmor(float armorHp, float maxArmorHp)
    {
        if (armorBar != null)
            armorBar.value = maxArmorHp > 0 ? armorHp / maxArmorHp : 0f;
    }

    public void UpdateAmmo(int current, int reserve, string weaponName)
    {
        if (ammoText      != null) ammoText.text      = current.ToString();
        if (reserveText   != null) reserveText.text   = reserve.ToString();
        if (weaponNameText != null) weaponNameText.text = weaponName;
    }

    public void UpdateZone(int stageIndex, float damageRate)
    {
        if (zoneText != null)
            zoneText.text = $"Zone {stageIndex + 1}  |  {damageRate:F0}%/s";
    }

    public void UpdateTeamCounts(int attackers, int defenders)
    {
        if (attackersText != null) attackersText.text = $"⚔ {attackers}";
        if (defendersText != null) defendersText.text = $"🛡 {defenders}";
    }

    public void ShowHitMarker()
    {
        if (_hitMarkerRoutine != null) StopCoroutine(_hitMarkerRoutine);
        _hitMarkerRoutine = StartCoroutine(HitMarkerRoutine());
    }

    public void TriggerFlash(float intensity, float duration)
        => StartCoroutine(FlashRoutine(intensity, duration));

    public void ShowHealProgress(float t)
    {
        if (healBarRoot  != null) healBarRoot.SetActive(t < 1f);
        if (healProgressBar != null) healProgressBar.value = t;
    }

    private IEnumerator HitMarkerRoutine()
    {
        if (hitMarkerImage != null) hitMarkerImage.enabled = true;
        yield return new WaitForSeconds(hitMarkerDuration);
        if (hitMarkerImage != null) hitMarkerImage.enabled = false;
    }

    private IEnumerator FlashRoutine(float intensity, float duration)
    {
        if (screenFlashImage == null) yield break;
        var color = new Color(1f, 1f, 1f, intensity);
        screenFlashImage.color   = color;
        screenFlashImage.enabled = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed    += Time.deltaTime;
            color.a     = Mathf.Lerp(intensity, 0f, elapsed / duration);
            screenFlashImage.color = color;
            yield return null;
        }
        screenFlashImage.enabled = false;
    }
}
