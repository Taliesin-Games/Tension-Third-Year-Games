
classDiagram
%% ===============================
%% HEART OF RUIN - HIGH LEVEL CLASS DIAGRAM (Unity 6)
%% ===============================

%% === GAME FLOW SYSTEMS ===
```mermaid
class GameManager {
    +StartGame()
    +EndRun()
    +LoadScene(sceneName)
    +ReturnToHub()
}
class SceneManager {
    +LoadAdditive(scene)
    +Unload(scene)
}
class SaveLoadSystem {
    +SaveData()
    +LoadData()
}
class EventManager {
    +Broadcast(eventName)
    +Subscribe(eventName, callback)
}
class AudioManager {
    +PlaySFX(id)
    +PlayBGM(track)
    +FadeTrack(newTrack)
}
class OptionsManager {
    +SaveSettings()
    +LoadSettings()
}
class GraphicsManager {
    +ApplyQualityPreset()
}
class AccessibilityManager {
    +ToggleSubtitles()
    +AdjustTextSize()
}

GameManager --> SceneManager
GameManager --> SaveLoadSystem
GameManager --> EventManager
GameManager --> AudioManager
GameManager --> OptionsManager
OptionsManager --> GraphicsManager
OptionsManager --> AccessibilityManager
```
```mermaid
%% === PLAYER SYSTEMS ===
class PlayerController {
    +Move()
    +Dash()
    +Attack()
}
class ResourceManager {
    +ModifyHealth()
    +ModifyMana()
    +CheckDeath()
}
class PlayerStats {
    +Strength
    +Agility
    +Intelligence
    +ApplyModifiers()
}
class Weapon {
    +Attack()
    +Damage
    +Cooldown
}
class WeaponFramework {
    +EquipWeapon()
    +GenerateWeapon(type)
}
class Inventory {
    +AddItem()
    +RemoveItem()
    +UseItem()
}
class BuffCurseManager {
    +ApplyBuff()
    +ApplyCurse()
    +ExpireEffect()
}

PlayerController --> ResourceManager
PlayerController --> PlayerStats
PlayerController --> Weapon
PlayerController --> BuffCurseManager
Inventory --> WeaponFramework
Inventory --> PlayerController
```

```mermaid
%% === ENEMY & AI SYSTEMS ===
class Enemy {
    +TakeDamage()
    +Attack()
}
class EnemyAI {
    +Patrol()
    +Chase()
    +AttackTarget()
}
class Boss extends Enemy {
    +PhaseTransition()
    +SpecialAttack()
}
class EnemySpawner {
    +SpawnEnemies()
    +DespawnAll()
}

EnemySpawner --> EnemyAI
EnemyAI --> Enemy
Boss <|-- Enemy
```

```mermaid
%% === COMBAT CORE SYSTEMS ===
class CombatSystem {
    +RegisterHit()
    +ApplyDamage()
}
class DamageSystem {
    +CalculateDamage()
    +ApplyResistance()
}
CombatSystem --> DamageSystem
PlayerController --> CombatSystem
Enemy --> CombatSystem
```

```mermaid
%% === DUNGEON SYSTEMS ===
class DungeonGenerator {
    +GenerateLayout(seed)
    +PlaceRooms()
}
class BiomeDefinition {
    +EnvironmentType
    +Modifiers
}
class FloorProgression {
    +AdvanceFloor()
    +ReturnToHub()
}
class SafePointHub {
    +EnterHub()
    +AccessShop()
}
class PermanentUpgradeSystem {
    +UnlockUpgrade()
    +ApplyUpgrade()
}

DungeonGenerator --> BiomeDefinition
GameManager --> FloorProgression
FloorProgression --> SafePointHub
SafePointHub --> PermanentUpgradeSystem
```

```mermaid
%% === UI SYSTEMS ===
class HUD {
    +DisplayHealth()
    +DisplayMana()
    +DisplayTension()
}
class InventoryUI {
    +ShowInventory()
    +EquipItem()
}
class ShopUI {
    +DisplayItems()
    +PurchaseItem()
}
class DialogueUI {
    +ShowDialogue()
    +AdvanceText()
}
class PauseMenu {
    +Open()
    +Close()
}

HUD --> PlayerController
InventoryUI --> Inventory
ShopUI --> SafePointHub
PauseMenu --> OptionsManager
```

```mermaid
%% === MULTIPLAYER & NETWORKING ===
class NetworkManager {
    +HostGame()
    +JoinGame()
    +SyncData()
}
class PartyManager {
    +CreateParty()
    +AddPlayer()
    +RemovePlayer()
}
class FlareSystem {
    +SendFlare()
    +ReceiveFlare()
}
class PseudoParty {
    +AutoJoin()
    +AutoLeave()
}
class LeaderboardSystem {
    +SubmitScore()
    +DisplayLeaderboard()
}
class WorldEventManager {
    +TriggerWorldBoss()
    +ScheduleRaid()
}

NetworkManager --> PartyManager
PartyManager --> PseudoParty
FlareSystem --> PartyManager
NetworkManager --> LeaderboardSystem
NetworkManager --> WorldEventManager
```

```mermaid
%% === TESTING & DEBUG ===
class DeveloperConsole {
    +ExecuteCommand(cmd)
}
class DebugOverlay {
    +ShowStats()
}
class ProceduralDebugTools {
    +VisualiseLayout()
}
class RemoteConsole {
    +StreamLogs()
}

DeveloperConsole --> GameManager
DebugOverlay --> GameManager
ProceduralDebugTools --> DungeonGenerator
RemoteConsole --> DeveloperConsole
```
%% End Diagram
