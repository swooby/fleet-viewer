## TODO
 * Sort by Line, Curve, Fleet, [Chaos or Custom] like starship42.com/fleetview  
   Notice they have button to inc/dec rows.  
   Breakpoint keydown and enter VMXXXX file (FleetDB object) and search for "#clay" handlers.
 * Skin imports similar to starship42.com/fleetview
 ```
 $("#import").on("click", function() {
    $("#mask").show(),
    ke(),
    le("action", "import")
 })
 ```

### Preferred RuntimeTransformGizmos' settings
```
  Runtime Editor Application:
    Enable Undo/Redo: True
    Use Custom Camera: True Main Camera
    Use Unity Colliders: True
    XZ Grid Settings:
      Is Visible: True
      Scroll grip up/down: Num keys 0
      Scroll grip up/down (STEP): Num keys 0
    Editor Gizmo System:
      Translation: True
      Rotation: True
      Activate move gizmo: Alpha1
      Activate rotation gizmo: Alpha2
      Activate global transform: O
      Activate local transform: L
      Turn off gizmos: Alpha0
      Toggle pivot: P
    Editor Object Selection:
      Can Select Empty Objects: True
      Can Click-Select: True
      Can Multi-Select: True
      Default: True
      TransparentFX: True
      Ignore Raycast: True
      Water: True
      UI: True
      Default: True
      TransparentFX: True
      Ignore Raycast: True
      Water: True
      UI: True
      Draw Selection Boxes: True
      SELECTION BOX RENDER MODE: From Parent To Bottom
      Append to selection: Num keys 0
      Multi deselect: Num keys 0
      Duplicate selection: Return (Setting to Num keys 0 causes weird unusable lag bug!)
      Delete selection: Delete
    Scene Gizmo:
      Corner: None (Currently no way I know to set Orthogonal projection in VR)
      Lock Perspective: True
    Translation Gizmo:
      Gizmo Base Scale: 3
      Preserve Gizmo Screen Size: True
      No keymappings
    Rotation Gizmo:
      Gizmo Base Scale: 6
      Preserve Gizmo Screen Size: True
      Full Circle X/Y/Z: ?
      No keymappings

Main Camera Settings:
  Depth: 1

GvrLaserPointer Settings:
  Hybrid
  Default Reticle Distance: 0.5
  Draw Debug Rays (for now)
```
