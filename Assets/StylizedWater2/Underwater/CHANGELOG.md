1.0.1

Added:
- Static SetCurrentWaterLevel and SetCurrentMaterial functions to the UnderwaterRenderer class. 
  * This can be used to switch to other water bodies via triggers or other game logic
- UnderwaterRenderer.EnableRendering static boolean. Can be toggled to disable rendering in specific cases.
- Directional Caustics support for versions older than Unity 2020.2
- Option to enable high accuracy directional caustics (using either the Depth Normals prepass, or reconstructing it from the depth texture)
    
Fixed:
- Underwater surface not matching up perfectly if ambient skybox lighting is used with an intensity higher than x1
- Corrected default settings for particle system prefabs
- Enabling volume settings blending, requiring to toggle the component for it to take effect.
- Rendering also taking effect when editing a prefab

Changed:
- Render feature can longer be auto-added when in playmode

1.0.0
Initial release