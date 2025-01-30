# Gearbreaker

A multiplayer boss fight game made in Godot for the 2025 Boss Rush Jam

## Running in Godot

### Multiplayer Setup

1. Go to "Debug" -> "Customize Run Instances" at the top of the Godot editor
2. Toggle Enable Multiple Instances
3. Set number of players (2-4) for multiple instances and set "Main Run Args" to "-host".. don't include the quotes
4. In the "Instance Configuration" on the second line, click "Enabled" in the Override Main Args" column.
5. Click "OK" at the bottom.
6. Rebuild the Project and Run
   Note: When in game the host must join first and there can only be 1 host.
