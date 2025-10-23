# SWAT Game Plugin

A tactical team-based game mode plugin for Unturned servers using the Rocket framework. Players register as either SWAT or TERRORIST teams, spawn at designated positions per map, and compete in objective-based matches.

## Features
* **Team Registration System** - Players can register as SWAT or TERRORIST with persistent allegiance tracking
* **Per-Map Spawn Positions** - Set custom spawn positions and rotations for each team on different maps
* **SWAT Vehicle Spawning** - Register and spawn team vehicles at designated locations
* **Player Name Tags** - Automatic team tag prefixes ([SWAT] or [TERRORIST]) applied to player names
* **XML Database Storage** - All allegiances, positions, and vehicle data saved in easy-to-edit XML files
* **Rich Text Formatting** - Supports custom colors and rich text formatting in messages
* **Console & Player Commands** - Full command support from both in-game and server console

## Current Version: v0.1.0

This is an early alpha release. Core registration and spawn management features are implemented. Match system and team balancing features are planned for future releases.

---

## Commands
* `/SWATRegister <Allegiance> [Steam64ID]` - Register yourself or another player to SWAT or TERRORIST team. Aliases: `/sregister`, `/steam`
* `/SWATUnregister [Steam64ID]` - Unregister yourself or another player from their team. Aliases: `/sunregister`
* `/SWATPosition [Allegiance]` - Register your current position as a spawn point for your team (or specified team). Aliases: `/sposition`. Positions are saved per map and per allegiance.
* `/SWATVehicle <vehicleID>` - Register a SWAT vehicle spawn at your current position. Aliases: `/svehicle`. Example: `/svehicle 93`. Vehicle spawns are saved per map.
* `swat` - Test command to verify plugin is loaded.

---

## Permissions
To grant access to commands, add the following permissions to your `Rocket/Permissions.config.xml`:

```xml
<!-- Core SWAT permissions -->
<Permission Cooldown="0">swat.register</Permission>
<Permission Cooldown="0">swat.unregister</Permission>
<Permission Cooldown="0">swat.position</Permission>
<Permission Cooldown="0">swat.vehicle</Permission>

<!-- Test permission (optional) -->
<Permission Cooldown="0">swat.test</Permission>
```

---

## Configuration
The plugin creates `SWATConfiguration.xml` in the plugin directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SWATConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>yellow</MessageColor>
  <MessageIconUrl>https://cdn-icons-png.flaticon.com/512/387/387456.png</MessageIconUrl>
</SWATConfiguration>
```

---

## Database Files

### Allegiance.xml
Stores player team registrations:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Allegiance>
  <AllegianceData>
    <Steam64ID>76561198012345678</Steam64ID>
    <Team>SWAT</Team>
  </AllegianceData>
</Allegiance>
```

### PerMapInfos.xml
Stores spawn positions and vehicle data per map:

```xml
<?xml version="1.0" encoding="utf-8"?>
<PerMapInfos>
  <Map id="PEI">
    <Allegiance team="SWAT">
      <PlayerInfo>
        <Steam64ID>76561198012345678</Steam64ID>
        <Position>
          <x>100.5</x>
          <y>50.0</y>
          <z>200.3</z>
        </Position>
        <Rotation>
          <x>0</x>
          <y>90</y>
          <z>0</z>
        </Rotation>
      </PlayerInfo>
    </Allegiance>
    <SwatVehicleInfos>
      <VehicleID>93</VehicleID>
      <SpawnPosition>
        <x>105.2</x>
        <y>50.0</y>
        <z>205.7</z>
      </SpawnPosition>
      <SpawnRotation>
        <x>0</x>
        <y>180</y>
        <z>0</z>
      </SpawnRotation>
    </SwatVehicleInfos>
  </Map>
</PerMapInfos>
```

---

## Translations
Default translations are included. You can customize them in the plugin's `SWAT.translation.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="SwatTestMessage" Value="[[b]]SWAT Plugin[[/b]] is working! Welcome to the team, operative!" />
  <Translation Id="PlayerAlreadyRegistered" Value="Player {0} is already registered to team [[b]]{1}[[/b]]. Use /sunregister to unregister first." />
  <Translation Id="PlayerRegisteredToTeam" Value="Player {0} has been registered to team [[b]]{1}[[/b]]!" />
  <Translation Id="PlayerUnregisteredFromTeam" Value="Player {0} has been unregistered from team [[b]]{1}[[/b]]!" />
  <Translation Id="CommandRegisterPositionSaved" Value="Position registered for player [[b]]{0}[[/b]] for team [[b]]{1}[[/b]] on map [[b]]{2}[[/b]]!" />
  <Translation Id="CommandRegisterSWATVehicleSaved" Value="SWAT vehicle [[b]]{0}[[/b]] spawn registered on map [[b]]{1}[[/b]]!" />
</Translations>
```

> **Note:** Use `[[b]]text[[/b]]` for bold formatting in translations.

---

## Planned Features (v0.2.0+)

### Match System
* **`/SWATStart`** - Initialize match with the following features:
  * Teleport all registered players to their team spawn positions
  * Spawn SWAT team vehicle at designated location
  * Give each player their personal kit using integrated Kits plugin: `/kit <Steam64ID> <Steam64ID>`
  * Display live team member counts on HUD (e.g., SWAT: 2/2, TERRORISTS: 2/2)
  * Track match state and announce victory when one team is eliminated
  * Optional 30-minute build phase for Terrorist team

* **`/SWATCancel`** - Cancel ongoing match and reset game state

### Team Balancing
* **`/SWATShuffle`** - Automatically distribute all online players into random teams
  * Unregisters all existing allegiances
  * Randomly assigns SWAT/TERRORIST teams
  * Balances team sizes

### Match Validation
* Prevent match start if any registered player lacks a spawn position for their allegiance
* Block team changes during active matches
* Implement win conditions based on team elimination

### Integration with Kits Plugin
* Seamless integration with [RestoreMonarchy's Kits plugin](https://github.com/RestoreMonarchyPlugins/Kits)
* Per-player kit assignment using Steam64ID as kit name
* Automatic kit distribution on match start

---

## Requirements

* Unturned dedicated server
* [RocketMod](https://github.com/RocketMod/Rocket.Unturned) (RestoreMonarchy fork recommended)
* .NET Framework 4.8
* (Optional) [Kits Plugin](https://github.com/RestoreMonarchyPlugins/Kits) for future match features

---

## Installation

1. Download the latest release from the Releases page
2. Extract `SWAT.dll` to your server's `Rocket/Plugins` folder
3. Restart your server
4. Configure permissions in `Rocket/Permissions.config.xml`
5. Customize settings in `Rocket/Plugins/SWAT/SWATConfiguration.xml`

---

## Support

For bug reports, feature requests, or questions:
* Open an issue on GitHub
* Check existing issues before creating new ones

---

## License

This project is developed for the Unturned modding community. Please respect the Unturned EULA and RocketMod licensing when using this plugin.

---

## Credits

Built with [RocketMod Framework](https://github.com/RocketMod/Rocket.Unturned)