# Camera Panning Test Guide

## CRITICAL FIX APPLIED ✅

**MAJOR ISSUE RESOLVED:** The camera panning system was updating `targetPosition` but applying movement to the wrong transform!

### What Was Fixed:
- **For Cinemachine setups**: CameraManager now moves the virtual camera's transform instead of its own transform
- **For regular camera setups**: CameraManager now moves the actual camera's transform
- **Added GetTargetTransform() method**: Automatically detects the correct transform to move based on your setup
- **Enhanced debugging**: Detailed logging shows which transform is being moved and when

## Current Status
All known camera panning issues have been addressed:

1. ✅ **InputSystem_Actions.inputactions** moved to `Assets/Resources/` folder
2. ✅ **Input Action Asset Loading** fixed with proper resource loading
3. ✅ **Action Map Enablement** corrected to enable entire InputActionAsset
4. ✅ **Camera Detection** enhanced with 6-tier fallback system
5. ✅ **Transform Movement** fixed to move the correct camera transform
6. ✅ **Enhanced Debugging** added comprehensive logging throughout the system

## Testing Steps

### Step 1: Verify Console Output
When you start Play Mode, check the Unity Console for these messages:

**Expected Initialization Messages:**
```
Camera: OnEnable called
Camera: InputActions asset found: InputSystem_Actions
Camera: InputActions enabled
Camera: Action mapping results - Move: Found, Zoom: Found, MiddleClick: Found, MousePosition: Found
Camera: Middle click action bound successfully
Camera: Mouse position action found successfully
Camera: Active camera test after OnEnable: [CameraName]
```

**If you see errors instead:**
- `Camera: InputActions asset is null` - The input actions failed to load
- `Camera: Could not find UI/MiddleClick action` - Action mapping issue
- `Camera: Active camera test after OnEnable: NONE FOUND` - No camera detected

### Step 2: Test Middle Mouse Button Panning
1. Start Play Mode
2. Hold down the **middle mouse button** in the Game view
3. **Expected Console Output:**
   ```
   Camera: OnMiddleMousePressed triggered
   Camera: Using camera for mouse panning: [CameraName]
   Camera: Started middle mouse panning at mouse position: (x, y), world position: (x, y, z)
   ```
4. **Move the mouse while holding middle button**
5. **Release the middle mouse button**
6. **Expected Console Output:**
   ```
   Camera: Stopped middle mouse panning
   ```

### Step 3: Manual Movement Test (NEW)
If mouse panning still doesn't work, you can test the movement system directly:
1. In Play Mode, open the Unity Console
2. Type: `CameraManager.Instance.DebugTestMovement(new Vector3(5, 5, 0))`
3. **Expected Result:** The camera should move 5 units right and up
4. **Expected Console Output:**
   ```
   === MOVEMENT DEBUG TEST - Offset: (5.0, 5.0, 0.0) ===
   Camera: Moved [CameraName] from (oldX, oldY, oldZ) to (newX, newY, newZ)
   === END MOVEMENT TEST ===
   ```

### Step 4: Test WASD Movement
1. In Play Mode, press **W, A, S, D** keys
2. The camera should move smoothly in the corresponding directions
3. Check if there are any movement-related errors in the console

### Step 5: Test Zoom (Mouse Wheel)
1. In Play Mode, scroll the **mouse wheel up/down**
2. The camera should zoom in/out
3. Verify zoom stays within minZoom/maxZoom bounds

## Troubleshooting Common Issues

### Issue: "InputActions asset is null"
**Solution:**
1. Check that `InputSystem_Actions.inputactions` exists in `Assets/Resources/`
2. In Unity, select the CameraManager GameObject
3. In the Inspector, manually drag the InputSystem_Actions asset to the "Input Actions" field

### Issue: "Could not find UI/MiddleClick action"
**Solution:**
1. Open `Assets/Resources/InputSystem_Actions.inputactions` in Unity
2. Verify the UI action map contains:
   - MiddleClick action
   - Point action
   - ScrollWheel action
3. Check that MiddleClick is bound to `<Mouse>/middleButton`

### Issue: "No active camera found"
**Solution:**
1. Ensure your scene has at least one active Camera
2. If using Cinemachine, ensure the CinemachineBrain component is on the main camera
3. Tag your main camera with "MainCamera" tag
4. In CameraManager Inspector, manually assign the "Virtual Camera" field

### Issue: Mouse panning starts but doesn't move camera
**Possible Causes:**
1. Camera movement bounds are too restrictive
2. Pan speed is set too low
3. Camera is constrained by another script

**Solution:**
1. Increase `mousePanSpeed` and `mousePanSensitivity` values
2. Check `minBounds` and `maxBounds` are reasonable (e.g., -50 to 50)
3. Temporarily disable other camera scripts

## Debug Console Commands

If issues persist, you can add these temporary debug lines to test specific components:

```csharp
// In Update() method, add:
if (Input.GetKeyDown(KeyCode.F1))
{
    Camera cam = GetActiveCamera();
    Debug.Log($"Manual camera test: {(cam != null ? cam.name : "NULL")}");
}

if (Input.GetKeyDown(KeyCode.F2))
{
    Debug.Log($"InputActions status: {(inputActions != null ? "Loaded" : "NULL")}");
    Debug.Log($"MiddleClick action: {(middleClickAction != null ? "Found" : "NULL")}");
}
```

## Advanced Debugging

### Enable Detailed Input System Logging
1. Open Edit → Project Settings → Input System Package
2. Enable "Generate C# Class" if not already enabled
3. Set Debug Mode to "Log All Actions"

### Camera Detection Debugging
The enhanced `GetActiveCamera()` method tries 6 different approaches:
1. Cinemachine Brain camera
2. Direct CinemachineCamera component
3. Parent/child camera lookup
4. Camera.main
5. Any camera in scene (FindFirstObjectByType)
6. CameraManager's own camera component

If all 6 fail, there's likely a deeper scene setup issue.

## Final Notes

- **Test in Play Mode only** - Input System actions are not active in Edit Mode
- **Check Game View focus** - Input might not register if Scene View is focused
- **Middle mouse sensitivity varies** - Some mice have different middle button implementations
- **WASD should work even if mouse panning fails** - They use different input methods

## Contact Information

If these tests still show issues, please provide:
1. Complete console log from Unity (especially during Play Mode start)
2. Screenshot of CameraManager Inspector settings
3. Confirmation of which specific test step fails
4. Unity version and Input System package version

Last Updated: [Current Date]
