<div align="center"><h1><img width="600" height="131" alt="68747470733a2f2f70616e2e73616d7979632e6465762f732f56596d4d5845" src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" /></h1></div>

<div class="section">
<div align="center"><h1>Zombie Plague for Swiftly2</h1></div>


<div align="center"><strong>基于 Swiftly2 框架开发的 CS2 僵尸瘟疫插件。</p></div>

<div align="center"><strong>支持多种自定义配置/Supports multiple custom configurations。</p></div>
<div align="center"><strong>支持自定义丧尸种类,多重游戏模式,僵尸瘟疫道具,支持API拓展,音效系统等。</p></div>
  <div align="center"><strong>supports customizable zombie types, multiple game modes, zombie plague items, sound effects system</p></div>
     <div align="center"><strong>supports API expansion.</p></div>
</div>

<div align="center">

<div style="display:flex; align-items:center; gap:6px;">
  <span>技术支持 yumi :</span>
  <a href="https://yumi.chat:3000/">
    <img src="https://yumi.chat:3000/logo.png" width="50">
  </a>
  <span>(最好的AI模型供应商,点击图标进入官网)</span>
</div>

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)

## 视频
https://www.bilibili.com/video/BV1c3cJzrEWn

如果遇到客户端崩溃问题,可使用 https://github.com/H-AN/HZPFixes 降低崩溃概率
</div>

---

插件可以配合以下创意工坊资源使用
```
音效 : 3644652779
丧尸模型 : 3655494223
```

---

<div align="center">
  <a href="./README.md"><img src="https://flagcdn.com/48x36/cn.png" alt="中文" width="48" height="36" /> <strong>中文版</strong></a>  
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="./README.en.md"><img src="https://flagcdn.com/48x36/gb.png" alt="English" width="48" height="36" /> <strong>English</strong></a>
</div>

<hr>

# Han Zombie Plague S2

**僵尸瘟疫插件**  
适用于 Counter-Strike 2 游戏的僵尸瘟疫模式插件。提供丰富的游戏模式、特殊丧尸/人类角色、道具系统、API 支持，让你的服务器充满乐趣与挑战！

## 特色功能总览

- **10 种多样化游戏模式**：从经典感染到特殊角色对抗，支持自由配置。
- **特殊职业系统**：母体丧尸、复仇之神（Nemesis）、暗杀者（Assassin）、幸存者（Survivor）、狙击手（Sniper）、英雄（Hero）等，每个角色有独立属性（血量、速度、重力、伤害、模型、武器）。
- **道具与技能**：T 病毒炸弹（感染范围）、燃烧手雷、照明弹、冰冻弹、传送手雷、防化服（免疫感染）、神模式、无限子弹、无限弹夹、无后坐力。
- **自定义配置**：每个模式独立开关无限子弹、丧尸复活；全局配置击退力、生成点、音效、环境音乐等。
- **玩家互动**：菜单选择丧尸偏好（存数据库）、管理员菜单、击杀伤害 HUD 显示。
- **API 支持**：完整事件系统（感染、角色选择、胜利等），供其他插件扩展自定义逻辑。
- **音效与视觉**：专属音效（感染、道具使用）、玩家外发光、FOV 调整、环境氛围音循环。
- **平衡优化**：击退系统（头部/身体/地面/空中不同倍率）、英雄击退独立配置。

## 游戏模式

插件提供 **10 种经典与创新模式**，每个模式支持独立配置：
- **丧尸复活开关**（ZombieCanReborn）
- **人类无限子弹**（EnableInfiniteClipMode）
- **模式权重**（Weight，用于随机选择模式）

1. **普通感染模式**  
   选择 1 名（可配置数量）母体丧尸开始感染人类。经典逐步扩散玩法。

2. **多重感染模式**  
   选择一半玩家作为母体丧尸，同时开始感染。快速进入高强度混战。

3. **幸存者模式**  
   选择 1 名人类作为幸存者，获得 M249 机枪 + 特殊属性（高血量、高速、低重力、高伤害，可配置），其他玩家变为丧尸，进行单人生存战。

4. **狙击手模式**  
   选择 1 名人类作为狙击手，获得狙击枪（AWP）+ 特殊属性（高血量、高速、低重力、高伤害，可配置），其他玩家变为丧尸，进行精准射击对抗。

5. **军团模式（对抗模式）**  
   一半玩家直接变为丧尸，无感染阶段，人类 vs 丧尸火力厮杀。

6. **瘟疫模式**  
   一半玩家变为丧尸 + 1 名幸存者 + 1 名复仇之神，进行多方大乱斗。

7. **暗杀者模式**  
   选择 1 名丧尸作为暗杀者（远距离隐身、近距离/受击显形，可配置隐身距离），无感染，进行潜行刺杀战。

8. **复仇之神模式**  
   选择 1 名丧尸作为复仇之神（高属性 boss），无感染，单挑全人类。

9. **英雄模式**  
   存活到最后 x 名（可配置数量）人类自动变为英雄，获得超强属性，继续战斗。

10. **狙击手 vs 暗杀者**  
    一半玩家变为丧尸 + 1 名狙击手 + 1 名暗杀者，三方混战对决。

## 配置说明

主要配置文件：`HZPMainCFG.json`。

### 全局配置
| 参数 | 说明 | 示例值 |
|------|------|--------|
| `RoundReadyTime` | 回合准备时间（秒） | 22.0 |
| `RoundTime` | 回合持续时间（分钟） | 4.0 |
| `HumandefaultModel` | 人类默认模型路径 | "characters/models/ctm_st6/ctm_st6_variante.vmdl" |
| `HumanMaxHealth` | 人类最大血量 | 225 |
| `HumanInitialSpeed` | 人类初始速度 | 1.0 |
| `HumanInitialGravity` | 人类初始重力 | 0.8 |
| `EnableDamageHud` | 显示击杀伤害 HUD | true |
| `EnableInfiniteReserveAmmo` | 人类无限备用弹药 | true |
| `EnableWeaponNoRecoil` | 武器无后坐力 | true |
| `HumanSpawnPoints` | 人类生成点（CT/T/DM） | "CT,T,DM" |
| `ZombieSpawnPoints` | 丧尸生成点 | "CT,T,DM" |
| `KnockZombieForce` | 击退丧尸力度 | 250.0 |
| `StunZombieTime` | 击退丧尸眩晕时间（秒） | 0.1 |
| `AmbSound` | 环境氛围音列表（逗号分隔） | "han.zombie.amb.zriot,..." |
| `AmbSoundLoopTime` | 氛围音循环间隔（秒） | 60.0 |
| `AmbSoundVolume` | 氛围音音量 | 0.8 |

### 击退系统（KnockBack）
- `HumanKnockBackHeadMultiply`：头部击退倍率（2.0）
- `HumanKnockBackBodyMultiply`：身体击退倍率（1.0）
- `HumanKnockBackGroundMultiply`：地面击退倍率（1.0）
- `HumanKnockBackAirMultiply`：空中击退倍率（0.5）
- `HumanHeroKnockBackMultiply`：英雄击退倍率（1.0）

### 道具配置
| 道具 | 开关 | 生成给予 | 范围/持续时间 | 伤害/效果 | 音效 |
|------|------|----------|--------------|----------|------|
| T 病毒炸弹 | - | - | 300.0 | 可感染英雄 | "han.zombieplague.grenadedote" |
| 燃烧手雷 | `FireGrenade` | `SpawnGiveFireGrenade` | 300.0 / 5.0s | 500 + 10/s | "han.zombieplague.grenadedote" |
| 燃烧弹 | - | `SpawnGiveIncGrenade` | - | - | - |
| 照明弹 | `LightGrenade` | `SpawnGiveLightGrenade` | 1000.0 / 30.0s | 致盲/强光 | "C4.ExplodeTriggerTrip" |
| 冰冻弹 | `FreezeGrenade` | `SpawnGiveFreezeGrenade` | 300.0 / 10.0s | 冻结 | "han.zombieplague.grenadedote" |
| 传送手雷 | `TelportGrenade` | `SpawnGiveTelportGrenade` | - | 传送 | - |
| T 病毒血清 | - | - | - | 变回人类 | "HealthShot.Pickup" |
| 防化服 | `CanUseScbaSuit` | - | 免疫感染 | - | 拾取/破损音效 |

### 模式配置示例
每个模式有独立 `Enable`（开关）、`Name`（显示名）、`Weight`（随机权重）。

- **普通感染**：`MotherZombieCount`（母体数量，默认 1）
- **幸存者**：`SurvivorHealth`（1000）、`SurvivorSpeed`（3.0）、模型/武器自定义
- **狙击手**：`SniperHealth`（500）、`SniperWeapon`（"weapon_awp"）
- **暗杀者**：`InvisibilityDist`（隐身距离，200.0）
- **英雄**：`HeroCount`（3 名）

完整 JSON 配置见仓库 `configs/` 文件夹。

## 安装指南

1. 下载插件包，解压到 `addons/swiftlys2/plugins/`。
2. 启动服务器。
3. 配置 `HZPMainCFG.json`。
4. 确保依赖：swiftlys2 框架支持。

## 命令列表

- `!zclass` 或 `sw_zclass`：打开丧尸职业选择菜单（玩家偏好,可自由定义指令）
- `!zmenu` 或 `sw_zmenu`：管理员菜单（需权限 `AdminMenuPermission`,不填写为所有人可以使用）

---

## 丧尸职业配置

插件支持丰富的丧尸职业系统，分为两类配置文件：

- **HZPZombieClassCFG.json**：普通丧尸职业列表（ZombieClassList）。  
  这些是常规感染后玩家可能变成的丧尸类型（如红骷髅、白骷髅、异形女王等）。

- **HZPSpecialClassCFG.json**：特殊丧尸职业列表（SpecialClassList）。  
  这些是模式中指定的特殊角色（如母体僵尸、复仇之神、暗杀者等）。

**两个文件格式完全相同**，只是为了区分普通丧尸与特殊丧尸而分开存放。  
每个丧尸职业都包含以下结构：

```json
{
  "Name": "职业名称",          // 必须唯一，用于主配置匹配
  "Enable": true,               // 是否启用此职业
  "PrecacheSoundEvent": "...",  // 预缓存音效事件文件
  "Stats": { ... },             // 属性数值
  "Models": { ... },            // 模型路径
  "Sounds": { ... }             // 各种音效
}
```
---

主配置与丧尸职业匹配机制
在主配置文件 HZPMainCFG.json 中，特殊模式通过名称字段匹配丧尸职业，例如：
```
"Nemesis": {
  "Enable": true,
  "Name": "复仇之神模式",
  "NemesisNames": "复仇之神",   // 必须与 HZPSpecialClassCFG.json 中的 "Name" 完全一致
  "Weight": 50,
  ...
}
```
NemesisNames、AssassinNames、SurvivorNames 等字段的值，必须精确匹配对应配置文件中的 "Name"。
如果名称不匹配、职业未启用或不存在，插件将无法加载该角色，可能导致模式异常。

---

| 参数                | 说明                               | 示例值    | 备注                                 |
|---------------------|------------------------------------|-----------|--------------------------------------|
| Health             | 普通状态最大血量                   | 8000     | -                                    |
| MotherZombieHealth | 作为母体丧尸时的最大血量           | 18000    | 仅在母体模式生效                     |
| Speed              | 移动速度倍率（1.0 = 默认人类速度） | 1.0 ~ 2.5| 越高越快                             |
| Damage             | 近战攻击伤害基值                   | 50.0     | 爪/刀伤害                            |
| Gravity            | 重力缩放（值越小跳越高、落得慢）   | 0.7      | 通常 0.2~1.0                         |
| Fov                | 视野角度（FOV）                    | 110      | 丧尸视角更宽                         |
| EnableRegen        | 是否启用自动回血                   | true     | -                                    |
| HpRegenSec         | 回血间隔时间（秒）                 | 5.0      | -                                    |
| HpRegenHp          | 每次回血量                         | 30       | -                                    |
| ZombieSoundVolume  | 丧尸相关音效音量                   | 1.0      | 0.0~1.0                              |
| IdleInterval       | 闲置音效播放间隔（秒）             | 70.0     | -                                    |
| 参数                  | 说明                          | 示例路径示例                                                                 |
|-----------------------|-------------------------------|-----------------------------------------------------------------------------|
| ModelPath             | 丧尸主体模型路径              | characters/models/voikanaa/feral_ghoul_fonv/feral_ghoul_fonv.vmdl         |
| CustomKinfeModelPath  | 自定义爪子/刀模型路径（可选） | ""（使用默认）                                                              |
| 参数          | 说明                         | 示例音效键                              |
|---------------|------------------------------|-----------------------------------------|
| SoundInfect  | 被感染时播放的声音           | han.human.mandeath                      |
| SoundPain    | 疼痛/受伤音效                | han.hl.zombie.pain                      |
| SoundHurt    | 受伤音效                     | han.zombie.manclassic_hurt              |
| SoundDeath   | 死亡音效                     | han.zombie.manclassic_death             |
| IdleSound    | 闲置/呼吸音效（可多选，逗号分隔） | han.hl.nihilanth.idle,han.hl.nihilanth.idleb |
| RegenSound   | 回血音效                     | han.zombie.state.manheal                |
| BurnSound    | 被燃烧时的音效               | han.zombieplague.zburn                  |
| ExplodeSound | 爆炸/特殊死亡音效            | han.hl.zombie.idle                      |
| HitSound     | 攻击命中敌人音效             | han.zombie.classic_hit                  |
| HitWallSound | 攻击打墙音效                 | han.zombie.classic_hitwall              |
| SwingSound   | 挥爪/攻击挥空音效            | han.zombie.classic_swing                |
| 职业名称     | 普通血量 | 母体血量 | 速度 | 重力 | FOV | 回血间隔/量    | 特色描述                     |
|--------------|----------|----------|------|------|-----|----------------|------------------------------|
| 红骷髅       | 8000    | 18000   | 1.0 | 0.7 | 110 | 5.0s / 30     | 高耐久、自动回血             |
| 白骷髅       | 3000    | 13000   | 1.1 | 0.8 | 110 | 5.0s / 30     | 中等血量、略快速度           |
| frozen       | 5000    | 15000   | 1.7 | 0.7 | 110 | 1.0s / 150    | 高回血速率（已禁用）         |
| 胖子         | 5000    | 15000   | 1.7 | 0.8 | 110 | 1.0s / 150    | 高回血速率（已禁用）         |
| 异形女王     | 2500    | 12500   | 2.0 | 0.2 | 110 | 10.0s / 5     | 极低重力、高速、女性音效     |
| 女科学家丧尸 | 1800    | 12000   | 1.8 | 0.5 | 110 | 10.0s / 5     | 高速、女性音效               |
| 职业名称     | 普通血量 | 母体血量 | 速度 | 重力 | FOV | 回血间隔/量    | 特色描述                     |
|--------------|----------|----------|------|------|-----|----------------|------------------------------|
| 母体僵尸     | 15000   | 20000   | 1.5 | 0.5 | 110 | 1.0s / 50     | 初始感染源、高伤害（150）    |
| 复仇之神     | 30000   | 50000   | 2.0 | 0.3 | 120 | 1.0s / 50     | 终极 boss、超高血量、低重力 |
| 暗杀者       | 15000   | 35000   | 2.5 | 0.4 | 120 | 2.0s / 60     | 超高速、配合隐身机制         |

---

## 音效广播系统（Vox 系统）

插件内置强大的音效广播系统（Vox），用于在游戏关键时刻播放语音播报（如回合开始、倒计时、模式宣布、胜利宣言等），增强沉浸感和氛围。

配置文件：**HZPVoxCFG.json**

### Vox 系统结构

VoxList 是一个数组，每个元素代表一组完整的播报语音包（例如 CSOL 男性、CSOL 女性、HL1 男性等）。

每个语音包的结构如下：

```json
{
  "Name": "语音包名称",          // 仅用于显示和识别
  "Enable": true,                 // 是否启用此语音包
  "PrecacheSoundEvent": "...",    // 预缓存的音效事件文件路径（必须填写）
  "RoundMusicVox": "...",         // 回合开始音乐/语音（可多选，逗号分隔）
  "SecRemainVox": "...",          // 剩余 20 秒提醒语音
  "CoundDownVox": "...",          // 倒计时语音（10~1 秒，可多选）
  "ZombieSpawnVox": "...",        // 丧尸生成/出现语音
  "NormalInfectionVox": "...",    // 普通感染模式宣布语音
  "MultiInfectionVox": "...",     // 多重感染模式宣布语音
  "NemesisVox": "...",            // 复仇之神模式宣布语音
  "SurvivorVox": "...",           // 幸存者模式宣布语音
  "SwarmVox": "...",              // 军团模式宣布语音
  "PlagueVox": "...",             // 瘟疫模式宣布语音
  "AssassinVox": "...",           // 暗杀者模式宣布语音
  "SniperVox": "...",             // 狙击手模式宣布语音
  "AVSVox": "...",                // 狙击手 vs 暗杀者模式宣布语音
  "HeroVox": "...",               // 英雄模式宣布语音
  "HumanWinVox": "...",           // 人类胜利语音（可多选）
  "ZombieWinVox": "..."           // 丧尸胜利语音（可多选）
}
```

| 参数              | 触发时机                     | 示例语音键（可多选，逗号分隔） | 备注 |
|-------------------|------------------------------|--------------------------------|------|
| RoundMusicVox    | 回合正式开始（准备时间结束） | han.zombie.round.class_start   | 常用于背景音乐或开场语音 |
| SecRemainVox     | 回合剩余 20 秒时             | han.zombie.round.20secremain   | 提醒玩家时间紧迫 |
| CoundDownVox     | 倒计时 10~1 秒（每秒一个）   | han.zombie.round.mancdone,...  | 支持 10 个独立语音，按顺序播放 |
| ZombieSpawnVox   | 丧尸生成或母体出现时         | han.zombie.round.manzbcome     | 增加紧张感 |
| NormalInfectionVox | 普通感染模式宣布             | han.zombieplague.end.horror    | 模式专属语音 |
| MultiInfectionVox | 多重感染模式宣布             | han.zombieplague.end.horror    | - |
| NemesisVox       | 复仇之神模式宣布             | han.zombieplague.type.nemesis  | - |
| SurvivorVox      | 幸存者模式宣布               | han.zombieplague.type.survivor | - |
| SwarmVox         | 军团模式宣布                 | han.zombieplague.end.horror    | - |
| PlagueVox        | 瘟疫模式宣布                 | han.zombieplague.end.plague    | - |
| AssassinVox      | 暗杀者模式宣布               | han.zombieplague.type.nemesis  | - |
| SniperVox        | 狙击手模式宣布               | han.zombieplague.type.survivor | - |
| AVSVox           | 狙击手 vs 暗杀者模式宣布     | han.zombieplague.type.nemesis  | - |
| HeroVox          | 英雄模式宣布                 | han.zombieplague.type.survivor | - |
| HumanWinVox      | 人类胜利时                   | han.zombie.round.manhmwin      | 可多选随机播放 |
| ZombieWinVox     | 丧尸胜利时                   | han.zombie.round.manzbwin      | 可多选随机播放 |
| 语音包名称     | 风格来源     | 启用状态 | 特色描述                             |
|----------------|--------------|----------|--------------------------------------|
| CSOL男性播报   | CSOL 风格男性 | true    | 经典男性播报，倒计时清晰有力，胜利语音激昂 |
| CSOL女性播报   | CSOL 风格女性 | true    | 女性声线，倒计时温柔但紧张，适合多样氛围 |
| HL1男性播报    | Half-Life 1 男性 | true    | 复古 HL1 播报风格，带有经典感染音和胜利宣言 |
| HL1女性播报    | Half-Life 1 女性 | true    | HL1 女性语音，独特复古氛围，剩余时间提醒特别 |
| 僵尸瘟疫播报   | Zombie Plague 经典 | true    | 混合多种经典语音，胜利语音丰富多样，随机播放效果强 |

**自定义提示**：
- 每个语音包可独立启用/禁用（"Enable": true/false）。
- 同一事件可填写多个语音键（逗号分隔），系统会**随机播放**其中一个，增加多样性。
- 所有语音必须在指定的 PrecacheSoundEvent 文件中预缓存。
- 你可以添加自己的语音包（如日语、韩语、本地化语音），只要音效文件路径正确即可。

**推荐玩法**：
- 服务器可根据活动主题切换语音包（例如万圣节用恐怖风格，节日用欢快风格）。
- 结合不同语音包与模式使用，能让游戏更有主题感（如 HL1 语音包配复古地图，CSOL 语音包配高强度对抗）。

---


## API 支持

完整 API 接口（IHanZombiePlagueAPI），支持事件监听（如感染、胜利）、玩家状态查询、强制设置角色等。详见 [HanZombiePlagueAPI.xml](API/net10.0/HanZombiePlagueAPI.xml) 。 


