# UNDER SIEGE — تحت الحصار

Mobile multiplayer castle-siege battle royale built with Unity 2022.3 LTS + Photon Fusion 2.

## Core Concept
Attackers (outside castle) vs Defenders (inside castle). Zone shrinks inward. Last team alive wins.

## Player Counts
| Mode | Attackers | Defenders |
|------|-----------|----------|
| Small | 20 | 15 |
| Medium | 30 | 20 |
| Large | 40 | 30 |
| XL | 50 | 40 |

## Maps
- رجال ألمع 1980 (default)
- جدة، الرياض، العلا، القاهرة، البرازيل، اليابان، أمريكا، أوروبا

## Tech Stack
| Layer | Tech |
|-------|------|
| Engine | Unity 2022.3 LTS |
| Networking | Photon Fusion 2 |
| Platform | iOS / Android |
| Target FPS | 30–60 |

## Project Structure
```
Assets/Scripts/
├── Core/           GameManager, MatchManager
├── Zone/           ZoneManager, ZoneStage
├── Player/         PlayerController, PlayerHealth, PlayerInventory, ArmorSystem
├── Weapons/        WeaponBase, WeaponData, BulletController
├── Combat/         DamageSystem, ExplosiveItem, HealingSystem
├── Siege/          GateController, GrappleController, SecretPassage
├── Loot/           LootItem, LootTable, LootSpawner
├── Teams/          TeamManager
├── Networking/     FusionNetworkManager, AntiCheat, ReconnectManager
├── UI/             HUDController, MinimapController, PingSystem, PingIcon,
│                   ResultsScreen, CameraShake
└── Audio/          AudioManager
```

## Zone Stages
| Stage | Area | Duration | Damage/s |
|-------|------|----------|----------|
| 1 | Map edges | 90s | 1% |
| 2 | Village | 90s | 1% → 3% |
| 3 | Castle perimeter | 60s | 3% |
| 4 | Inside castle | 60s | 3% → 10% |
| 5 | Final circle | 60s | 10% |

## Siege Systems
- **Gates**: 2 main (HP 3000) + 1 secondary (HP 1500) — destructible
- **Ropes**: 3–4 grapple points on walls — 3–5s climb, exposed
- **Secret Passage**: 1 hidden tunnel inside ↔ outside
- **Gate Breach**: 25% chance random open with alarm + 5–10s warning

## Weapons
| Type | Notes |
|------|-------|
| AR | Full-auto, medium range |
| SMG | High fire rate, short range |
| Sniper | High damage, long range |
| Shotgun | Multi-pellet, close range |
| Pistol | Secondary only |

Attachments: Scope · Mag · Grip

## Health & Armor
- HP: 100 — Knocked at 0, teammates can revive within 30s
- No respawn after death
- Armor Lv1: 20% reduction (100 ArmorHP) · Lv2: 35% (150) · Lv3: 50% (200)
- Bandage: +10 HP over 4s | Medkit: +100 HP over 6s (cancelled by movement/damage)

## Anti-Cheat (Server-authoritative)
- Speed validation per tick
- Fire-rate validation
- Damage-value validation (client never trusted for critical numbers)
- Kick after 5 violations
