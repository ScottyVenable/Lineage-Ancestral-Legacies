# Camera Controls Guide

## Overview
The CameraManager now supports both keyboard and mouse panning for smooth camera movement throughout your game world.

## Controls

### Keyboard Panning (WASD)
- **W** - Move camera up
- **A** - Move camera left  
- **S** - Move camera down
- **D** - Move camera right
- **Arrow Keys** - Alternative movement controls

### Mouse Panning
- **Middle Mouse Button + Drag** - Pan camera by dragging with the middle mouse button
- **Mouse Wheel** - Zoom in/out

## Configuration Options

### Camera Settings
- **Pan Speed** - Speed of keyboard movement (default: 20)
- **Mouse Pan Speed** - Speed of mouse drag panning (default: 1)
- **Zoom Speed** - Speed of mouse wheel zoom (default: 10)
- **Smooth Movement** - Enable smooth interpolation between positions
- **Smooth Time** - Time for smooth movement interpolation (default: 0.5s)

### Mouse Panning Settings
- **Invert Mouse Pan X** - Reverse horizontal mouse panning direction
- **Invert Mouse Pan Y** - Reverse vertical mouse panning direction (default: true)
- **Mouse Pan Sensitivity** - Multiplier for mouse panning sensitivity (default: 1)

### Boundaries
- **Min Bounds** - Minimum camera position limits
- **Max Bounds** - Maximum camera position limits

## Technical Details

### Input System Integration
The camera controls use Unity's Input System with the following action mappings:
- `Player/Move` - WASD keyboard movement
- `UI/MiddleClick` - Middle mouse button detection
- `UI/Point` - Mouse position tracking
- `UI/ScrollWheel` - Mouse wheel zoom

### Fallback Support
If the Input System is not available, the camera will automatically fall back to Unity's legacy input system.

### Cinemachine Compatibility
The system works with both:
- Standard Unity Cameras
- Cinemachine Virtual Cameras (when Cinemachine package is installed)

## Usage Tips

1. **Smooth vs Instant Movement**: Enable "Smooth Movement" for cinematic camera feel, disable for precise control
2. **Boundary Setup**: Set min/max bounds to prevent camera from going outside your game world
3. **Sensitivity Tuning**: Adjust mouse pan sensitivity based on your game's scale and feel
4. **Y-Axis Inversion**: Most RTS games have inverted Y-axis for mouse panning (moving mouse up pans camera down)

## Public API

### Properties
- `CameraManager.Instance.IsMousePanning` - Check if middle mouse panning is active
- `CameraManager.Instance.CurrentTargetPosition` - Get the current target camera position

### Methods
- `FocusOnTransform(Transform target)` - Instantly move camera to focus on a specific object

## Troubleshooting

### Camera Not Moving
1. Check that the CameraManager component is active
2. Verify Input Action Asset is assigned in the References section
3. Ensure the camera boundaries allow movement in the desired direction

### Mouse Panning Not Working
1. Verify that the Input System package is installed
2. Check that UI/MiddleClick and UI/Point actions exist in your Input Action Asset
3. Make sure the middle mouse button binding is configured correctly
4. **Check the Console** for debug messages when pressing the middle mouse button - you should see "Started middle mouse panning" messages
5. Ensure the entire InputActionAsset is being enabled (not just individual actions)
6. **IMPORTANT**: Make sure the `InputSystem_Actions.inputactions` file is located in the `Assets/Resources/` folder

### Input System Setup
- The Input Action Asset must be in `Assets/Resources/InputSystem_Actions.inputactions` for automatic loading
- Alternatively, assign the Input Action Asset directly in the CameraManager's References section
- The system automatically enables all action maps when the component starts

### Performance
- Mouse panning uses screen-to-world position conversion each frame while active
- Consider adjusting update frequency for lower-end devices if needed

## Recent Fixes Applied
1. **Fixed Input Action Asset Loading**: Moved InputSystem_Actions.inputactions to Resources folder
2. **Fixed Action Map Enablement**: Now properly enables entire action asset instead of individual actions
3. **Improved Debug Logging**: Added detailed logging for troubleshooting mouse panning issues
4. **Fixed Screen-to-World Conversion**: Improved z-coordinate calculation for more accurate mouse positioning
