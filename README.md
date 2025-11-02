# SWAT Game Plugin for Unturned

A competitive team-based game plugin for Unturned servers that pits SWAT forces against Terrorists in tactical combat scenarios.

## 🎮 Overview

The SWAT Game Plugin is a RocketMod plugin that creates an asymmetric tactical combat gamemode for Unturned. **Terrorists** fortify a position (like a city or compound) during a build phase, while **SWAT forces** wait at their spawn with their vehicle. Once the build phase ends, SWAT must assault the terrorist position and eliminate all hostiles. Last team standing wins.

### How It Works

1. **Terrorists** spawn at their base location (e.g., a city like Waikoloa)
2. Terrorists get a **build phase** (configurable, e.g., 30 minutes) to prepare defenses, build bases, set traps, etc.
3. **SWAT team** spawns at a different location with their vehicle and waits during the build phase
4. When the build phase ends, **SWAT assaults** - they drive to the terrorist location and breach
5. **Terrorists defend** using their fortifications, guerrilla tactics, or any strategy they choose
6. **Last team standing wins**

### Key Features

- **Asymmetric Gameplay**: Defenders (Terrorists) vs Attackers (SWAT)
- **Build Phase**: Terrorists fortify while SWAT waits
- **Custom Spawn Points**: Separate spawn locations for each team on each map
- **SWAT Vehicle**: SWAT gets a vehicle to transport to the assault location
- **Kit System**: Automated kit distribution based on team
- **Flexible Terrorist Strategy**: Build bases, use guerrilla tactics, or any defensive approach
- **Win Condition**: Last team standing wins

## 📋 Requirements

- **Unturned Server** (tested on latest version)
- **RocketMod** (Rocket.Unturned)
- **RestoreMonarchy Kits Plugin** (recommended for kit functionality)

## 🛠️ Installation

1. Download the latest release of the SWAT plugin
2. Place the plugin DLL in your server's `Rocket/Plugins` directory
3. Restart your server to generate the configuration files
4. Configure the plugin as needed (see Configuration section)

## ⚙️ Configuration

The plugin generates a configuration file in `Rocket/Plugins/SWAT/SWAT.configuration.xml`:

```xml
<SWATConfiguration>
  <MessageColor>yellow</MessageColor>
  <MessageIconUrl>https://cdn-icons-png.flaticon.com/512/387/387456.png</MessageIconUrl>
</SWATConfiguration>
```

### Database Files

The plugin creates three XML database files in `Rocket/Plugins/SWAT/`:

- **Allegiance.xml**: Stores player team assignments
- **KitInfo.xml**: Stores kit assignments per team
- **PerMapInfos.xml**: Stores spawn positions and vehicle info for each map

## 🎯 Player Setup (How to Join a Game)

### Step 1: Register to a Team

Players must first register themselves to either the SWAT or TERRORIST team:

```
/sregister SWAT
```
or
```
/sregister TERRORIST
```

**Aliases**: `/steam`

**Note**: Admins can register other players by adding their name or Steam64ID:
```
/sregister SWAT PlayerName
```

### Step 2: Register Spawn Position

Each player needs to set their spawn position on the current map. Stand where you want to spawn and run:

```
/sposition
```

This saves your current position as your team's spawn point on the active map.

**Optional**: Specify a team if not already registered:
```
/sposition SWAT
```

### Step 3: Set Your Kit

Each player sets their own kit for their team:

```
/setkit <kitname>
```

Example:
```
/setkit SWATRifleman
```

**Note**: Each player can can choose a kit per Team. If a player has not set a kit, the plugin automatically looks for a kit with their username. Kits are given on combat start.

**Aliases**: `/skit`

### Step 4: Configure SWAT Vehicle Spawn

Someone needs to configure where the SWAT vehicle will spawn on each map (only needs to be done once per map). Stand at the desired location and run:

```
/svehicle <vehicleID>
```

Example:
```
/svehicle 92
```

### Step 5: Ready to Play!

Once setup is complete:
1. All players registered to teams (`/sregister`)
2. All players set their spawn positions (`/sposition`)
3. All players set their kits (`/setkit`)
4. SWAT vehicle spawn configured (`/svehicle`)

An admin can start the game using `/start`.

## 🎮 Commands Reference

### Player Commands

| Command | Syntax | Description | Permission |
|---------|--------|-------------|------------|
| `/sregister` | `<SWAT\|TERRORIST> [PlayerName\|Steam64ID]` | Register yourself or another player to a team | `swat.register` |
| `/steam` | `<SWAT\|TERRORIST> [PlayerName\|Steam64ID]` | Alias for `/sregister` | `swat.register` |
| `/sunregister` | `[PlayerName\|Steam64ID]` | Remove yourself or another player from their team | `swat.unregister` |
| `/sposition` | `[SWAT\|TERRORIST]` | Register your current position as spawn point | Player only |
| `/svehicle` | `<vehicleID>` | Register SWAT vehicle spawn location at your position | `swat.vehicle` |
| `/setkit` | `<kitname> [SWAT\|TERRORIST] [PlayerName\|Steam64ID]` | Set your kit or another player's kit | `swat.setkit` |
| `/skit` | `<kitname> [SWAT\|TERRORIST] [PlayerName\|Steam64ID]` | Alias for `/setkit` | `swat.setkit` |
| `/showteams` | - | Display all team members currently online | `swat.showteams` |
| `/teams` | - | Alias for `/showteams` | `swat.showteams` |
| `/status` | - | Check current game status | `swat.status` |
| `/info` | - | Alias for `/status` | `swat.status` |
| `/clearinventory` | - | Clear your own inventory | `swat.clearinventory` |
| `/cinv` | - | Alias for `/clearinventory` | `swat.clearinventory` |

### Admin Commands - Team Management

| Command | Syntax | Description | Permission |
|---------|--------|-------------|------------|
| `/shuffle` | - | Randomly assign all online players to teams | `swat.shuffle` |

### Admin Commands - Game Control

| Command | Syntax | Description | Permission |
|---------|--------|-------------|------------|
| `/start` | `[buildtime]` | Start the SWAT game (buildtime in minutes, default 0) | `swat.start` |
| `/cancel` | - | Cancel the current game | `swat.cancel` |
| `/skip` | - | Skip the build phase and start combat immediately | `swat.skip` |

### Admin Commands - Utilities

| Command | Syntax | Description | Permission |
|---------|--------|-------------|------------|
| `/clear` | `<all\|items\|vehicles\|buildings\|inventory>` | Clear map entities | `clear` |

**Clear Options**:
- `all` or `a` - Remove all items, vehicles, and buildings
- `items` or `i` - Remove all dropped items
- `vehicles` or `v` - Remove all vehicles
- `buildings` or `b` - Remove all structures and barricades
- `inventory` or `inv` - Clear your inventory

## 🚀 Starting a SWAT Game

### Prerequisites Checklist

Before starting a game, ensure:

- ✅ At least 1 player registered to **SWAT** team
- ✅ At least 1 player registered to **TERRORIST** team
- ✅ All registered players have set their spawn positions on the current map
- ✅ All players have set their kits
- ✅ SWAT vehicle spawn location configured on the current map

### Game Flow

#### 1. **Game Start**

Admin starts the game with a build phase time (in minutes):
```
/start 30
```
(30 minutes build phase - adjust as needed)

Or start immediately without build phase:
```
/start
```

#### 2. **Preparing Phase**
- All players are teleported to their spawn positions
- **Terrorists** spawn at their base/city location (e.g., Waikoloa)
- **SWAT** spawns at their staging area with their vehicle
- Teams and game info are announced

#### 3. **Build Phase** (Terrorists fortify, SWAT waits)
- **Terrorists**: Build defenses, fortify positions, set up bases, prepare traps
- **SWAT**: Wait at spawn with their vehicle (cannot attack yet)
- Timer counts down (e.g., 30 minutes)
- Admin can skip early with `/skip` if both teams are ready

#### 4. **Combat Phase Begins**
- Build phase ends
- **All players** are teleported back to their spawn positions
- **All inventories are cleared**
- **Kits are automatically given** to all players
- **SWAT**: Takes the vehicle and drives to the terrorist location
- **Terrorists**: Defend their position using fortifications and tactics
- **Combat begins!**

#### 5. **Victory**
- When all members of one team are eliminated, the other team wins
- Game ends automatically
- Victory is announced

## 📝 Example Setup Workflow

### For Server Admins (First Time Setup)

1. **Install the plugin** and restart the server

2. **Assign players to teams**:
   ```
   /sregister SWAT PlayerOne
   /sregister SWAT PlayerTwo
   /sregister TERRORIST PlayerThree
   /sregister TERRORIST PlayerFour
   ```

   Or use shuffle:
   ```
   /shuffle
   ```

3. **Have each player set their spawn position**:
   - **Terrorists** go to their base/city location (e.g., Waikoloa)
   - Each terrorist runs: `/sposition`
   - **SWAT** goes to their staging area (different location from terrorists)
   - Each SWAT member runs: `/sposition`

4. **Have each player set their kit**:
   - Each player runs: `/setkit <kitname>`
   - Example: `/setkit SWATRifleman` or `/setkit TerroristAK`

5. **Configure SWAT vehicle spawn** (only needs to be done once per map):
   - Anyone can do this step
   - Stand at the **SWAT spawn location** where the vehicle should appear
   - Run: `/svehicle 92` (92 = vehicle ID, e.g., APC)

6. **Start the game with build phase**:
   ```
   /start 30
   ```
   (Terrorists get 30 minutes to build, SWAT waits)

### For Players

1. **Join a team**: `/sregister SWAT` or `/sregister TERRORIST`

2. **Set spawn point**: 
   - **If TERRORIST**: Go to your base location (e.g., a city to defend)
   - **If SWAT**: Go to your staging area (different from terrorist location)
   - Run: `/sposition`

3. **Set your kit**: `/setkit <kitname>` (e.g., `/setkit SWATRifleman`)

4. **Configure vehicle** (if not done yet): Someone on SWAT needs to run `/svehicle <ID>` at the SWAT spawn once per map

5. **Check teams**: `/showteams` to see who's on each team

6. **Wait for admin** to start the game with `/start 30` (or other build time)

7. **Play your role**:
   - **Terrorists**: Build defenses during build phase, then defend!
   - **SWAT**: Wait during build phase, then assault when it ends!