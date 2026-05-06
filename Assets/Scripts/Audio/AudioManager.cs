using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips")]
    [SerializeField] private AudioClip alarmClip;
    [SerializeField] private AudioClip gateOpenClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip passageClip;
    [SerializeField] private AudioClip hitBulletClip;
    [SerializeField] private AudioClip hitZoneClip;

    [Header("Ping Clips")]
    [SerializeField] private AudioClip pingEnemyClip;
    [SerializeField] private AudioClip pingLootClip;
    [SerializeField] private AudioClip pingDangerClip;

    [Header("Pool")]
    [SerializeField] private int   poolSize           = 16;
    [SerializeField] private float maxHearingDistance = 150f;

    private AudioSource       _uiSource;     // 2D, for UI / local sounds
    private Queue<AudioSource> _pool = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _uiSource        = gameObject.AddComponent<AudioSource>();
        _uiSource.spatialBlend = 0f;

        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend  = 1f;
            src.maxDistance   = maxHearingDistance;
            src.rolloffMode   = AudioRolloffMode.Linear;
            src.dopplerLevel  = 0f;
            _pool.Enqueue(src);
        }
    }

    // Play a spatial (3-D) sound at world position
    public void PlayAt(AudioClip clip, Vector3 position)
    {
        if (clip == null || _pool.Count == 0) return;
        var src = _pool.Dequeue();
        src.transform.position = position;
        src.clip = clip;
        src.Play();
        StartCoroutine(ReturnToPool(src, clip.length + 0.1f));
    }

    public void PlayAlarm()                  => _uiSource.PlayOneShot(alarmClip);
    public void PlayGateOpen()               => PlayAt(gateOpenClip, Vector3.zero);
    public void PlayExplosionAt(Vector3 pos) => PlayAt(explosionClip, pos);
    public void PlayPassageSound()           => _uiSource.PlayOneShot(passageClip);

    public void PlayHitSound(DamageSource source)
    {
        AudioClip clip = source == DamageSource.Zone ? hitZoneClip : hitBulletClip;
        _uiSource.PlayOneShot(clip, 0.8f);
    }

    public void PlayPingSound(PingType type)
    {
        AudioClip clip = type switch
        {
            PingType.Enemy  => pingEnemyClip,
            PingType.Loot   => pingLootClip,
            _               => pingDangerClip,
        };
        _uiSource.PlayOneShot(clip, 0.6f);
    }

    private IEnumerator ReturnToPool(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        _pool.Enqueue(src);
    }
}
