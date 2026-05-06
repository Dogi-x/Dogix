using UnityEngine;
using Fusion;

public class ZoneManager : NetworkBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [SerializeField] private ZoneStage[] stages = new ZoneStage[]
    {
        new ZoneStage { stageName="Map Edge",         radius=500f, duration=90f,  damagePerSecStart=1f,  damagePerSecEnd=1f  },
        new ZoneStage { stageName="Village",           radius=250f, duration=90f,  damagePerSecStart=1f,  damagePerSecEnd=3f  },
        new ZoneStage { stageName="Castle Perimeter",  radius=120f, duration=60f,  damagePerSecStart=3f,  damagePerSecEnd=3f  },
        new ZoneStage { stageName="Inside Castle",     radius=50f,  duration=60f,  damagePerSecStart=3f,  damagePerSecEnd=10f },
        new ZoneStage { stageName="Final Circle",      radius=15f,  duration=60f,  damagePerSecStart=10f, damagePerSecEnd=10f },
    };

    [Networked] public int   CurrentStageIndex { get; private set; }
    [Networked] public float CurrentRadius     { get; private set; }
    [Networked] public Vector3 ZoneCenter      { get; private set; }
    [Networked] public bool  IsActive          { get; private set; }
    [Networked] private float StageElapsed     { get; set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ActivateZone()
    {
        if (!HasStateAuthority) return;
        IsActive           = true;
        CurrentStageIndex  = 0;
        CurrentRadius      = stages[0].radius;
        ZoneCenter         = Vector3.zero;
        StageElapsed       = 0f;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsActive) return;
        if (CurrentStageIndex >= stages.Length) return;

        ZoneStage stage = stages[CurrentStageIndex];
        StageElapsed += Runner.DeltaTime;

        // Lerp radius toward next stage
        if (CurrentStageIndex + 1 < stages.Length)
        {
            float t = Mathf.Clamp01(StageElapsed / stage.duration);
            CurrentRadius = Mathf.Lerp(stage.radius, stages[CurrentStageIndex + 1].radius, t);
        }

        if (StageElapsed >= stage.duration)
        {
            StageElapsed = 0f;
            CurrentStageIndex = Mathf.Min(CurrentStageIndex + 1, stages.Length - 1);
        }
    }

    public float GetCurrentDamageRate()
    {
        if (!IsActive || CurrentStageIndex >= stages.Length) return 0f;
        ZoneStage stage = stages[CurrentStageIndex];
        float t = Mathf.Clamp01(StageElapsed / Mathf.Max(0.001f, stage.duration));
        return Mathf.Lerp(stage.damagePerSecStart, stage.damagePerSecEnd, t);
    }

    public bool IsOutsideZone(Vector3 position)
        => IsActive && Vector3.Distance(position, ZoneCenter) > CurrentRadius;
}
