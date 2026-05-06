using UnityEngine;

public enum WeaponType { AR, SMG, Sniper, Shotgun, Pistol }

[CreateAssetMenu(menuName = "UnderSiege/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public int        id;
    public string     weaponName;
    public WeaponType type;
    public int        inventoryWeight = 20;

    [Header("Damage")]
    public float damage              = 30f;
    public float headshotMultiplier  = 2f;

    [Header("Range")]
    public float          effectiveRange = 80f;
    public float          maxRange       = 200f;
    public AnimationCurve rangeFalloff   = AnimationCurve.Linear(0, 1, 1, 0.5f);

    [Header("Fire")]
    public float fireRate    = 600f;  // rounds per minute
    public bool  isAutomatic = true;
    public int   magazineSize = 30;
    public float reloadTime   = 2.0f;
    public int   ammoPerShot  = 1;    // >1 for shotgun

    [Header("Recoil")]
    public Vector2 recoilPattern   = new Vector2(0.05f, 0.1f);
    public float   recoilRandomness = 0.02f;

    [Header("ADS")]
    public float adsTimeSeconds = 0.2f;
    public float hipfireSpread  = 3f;
    public float adsSpread      = 0.5f;

    [Header("Attachments")]
    public bool acceptsScope = true;
    public bool acceptsMag   = true;
    public bool acceptsGrip  = true;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
}
