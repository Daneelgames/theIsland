//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedWater2
{
    [AddComponentMenu("Stylized Water 2/Underwater Renderer")]
    [ExecuteInEditMode]
    public class UnderwaterRenderer : MonoBehaviour
    {
        public static string Version = "1.0.1";
        public static string MinBaseVersion = "1.1.3";
        
        public static UnderwaterRenderer Instance;
        /// <summary>
        /// If false, underwater rendering is disabled entirely
        /// </summary>
        public static bool EnableRendering = true;

        [HideInInspector]
        //If this is throwing an error either the Stylized Water 2 asset is not installed, or an outdated version is used (see MinBaseVersion)
        public WaveParameters waveParameters = new WaveParameters();
        
        public enum WaterLevelSource
        {
            FixedValue,
            Transform
        }
        
        [Tooltip("Configure what should be used to set the base water level. Either a fixed value, or based on a transform's world-space Y-position")]
        public WaterLevelSource waterLevelSource;
        [Tooltip("The base water level, this value is important and required for correct rendering. As such, underwater rendering does not work with rivers or other non-flat water")]
        public float waterLevel;
        [Tooltip("This transform's Y-position is used as the base water level, this value is important and required for correct rendering. As such, underwater rendering does not work with rivers or other non-flat water")]
        public Transform waterLevelTransform;
        public float CurrentWaterLevel
        {
            get
            {
                if (waterLevelSource == WaterLevelSource.Transform && waterLevelTransform) return waterLevelTransform.position.y;

                return waterLevel;
            }
        }
        
        [Tooltip("The water material used in the environment. This is used to copy its colors and wave settings, so everything is in sync")]
        public Material waterMaterial;
        [Tooltip("Only enable if the material's wave parameters are being changed in realtime, this has some performance overhead.\n\nIn edit-mode, the wave parameters are always fetched, so changes are directly visible")]
        public bool dynamicMaterial;

        [SerializeField]
        private UnderwaterSettings settings;

        //[Header("Fog")]
        [Tooltip("Control the fog settings through a Volume component. \"Underwater Settings\" must be present on any volume profile")]
        public bool useVolumeBlending;
        [Min(0f)]
        public float verticalDensity = 1f;
        [Min(0f)]
        public float verticalDepth = 18f;
        public float horizontalDensity = 8f;
        [Tooltip("Pushes the fog this many units away from the camera, resulting in clear water")]
        public float startDistance = 0f;
        [Min(0f)]
        public float fogBrightness = 1f;
        [Min(0f)]
        public float subsurfaceStrength = 1f;
        
        //[Header("Waterline")]
        [Tooltip("Pushes the lens effect this many units away from the camera. The camera's Near Clip value is added to this.")]
        [Min(0f)]
        public float offset = 1f;
        [Range(0.1f, 0.7f)]
        public float waterLineThickness = 0.4f;
        
        //[Header("Effects")]
        [Tooltip("Enables blurring based on fog density. This emulates light scattering in murky water")]
        public bool enableBlur;
        [Tooltip("Distorts the underwater image in screen-space using a noise texture")]
        public bool enableDistortion;

        public const string Keyword = "UNDERWATER_ENABLED"; //multi_compile (global)
        
        private static int _WaterLevel = Shader.PropertyToID("_WaterLevel");
        private static int _ClipOffset = Shader.PropertyToID("_ClipOffset");
        private static int _StartDistance = Shader.PropertyToID("_StartDistance");
        private static int _VerticalDensity = Shader.PropertyToID("_VerticalDensity");
        private static int _VerticalDepth = Shader.PropertyToID("_VerticalDepth");
        private static int _HorizontalDensity = Shader.PropertyToID("_HorizontalDensity");
        private static int _UnderwaterFogBrightness = Shader.PropertyToID("_UnderwaterFogBrightness");
        private static int _UnderwaterSubsurfaceStrength = Shader.PropertyToID("_UnderwaterSubsurfaceStrength");

        private void Update()
        {
            if (!EnableRendering) return;
            
            if (dynamicMaterial || Application.isPlaying == false) UpdateMaterialParameters();
            
            if(useVolumeBlending || Application.isPlaying == false) UpdateProperties();
            
            if(useVolumeBlending && !settings) GetVolumeSettings();
        }

        /// <summary>
        /// Passes the fog parameters, water level and offset value to shader land. Call this whenever changing these values through script!
        /// </summary>
        public void UpdateProperties()
        {
            Shader.SetGlobalFloat(_WaterLevel, CurrentWaterLevel);
            Shader.SetGlobalFloat(_ClipOffset, offset);

            Shader.SetGlobalFloat(_StartDistance, useVolumeBlending && settings ? settings.startDistance.value : startDistance);
            Shader.SetGlobalFloat(_VerticalDensity, (useVolumeBlending && settings ? settings.verticalDensity.value : verticalDensity) * 0.01f);
            Shader.SetGlobalFloat(_VerticalDepth, useVolumeBlending && settings ? settings.verticalDepth.value : verticalDepth);
            Shader.SetGlobalFloat(_HorizontalDensity, (useVolumeBlending && settings ? settings.horizontalDensity.value  : horizontalDensity) * 0.01f);
            Shader.SetGlobalFloat(_UnderwaterFogBrightness, (useVolumeBlending && settings ? settings.fogBrightness.value  : fogBrightness));
            Shader.SetGlobalFloat(_UnderwaterSubsurfaceStrength, (useVolumeBlending && settings ? settings.subsurfaceStrength.value  : subsurfaceStrength));
        }

        private static int SourceShallowColorID = Shader.PropertyToID("_ShallowColor");
        private static int SourceDeepColorID = Shader.PropertyToID("_BaseColor");
        private static int DestShallowColorID = Shader.PropertyToID("_WaterShallowColor");
        private static int DestDeepColorID = Shader.PropertyToID("_WaterDeepColor");
        
        private static int CausticsTexID = Shader.PropertyToID("_CausticsTex");
        private static int CausticsTilingID = Shader.PropertyToID("_CausticsTiling");
        private static int CausticsBrightnessID = Shader.PropertyToID("_CausticsBrightness");
        private static int CausticsSpeedID = Shader.PropertyToID("_CausticsSpeed");
        
        private static int _TranslucencyParams = Shader.PropertyToID("_TranslucencyParams");
        
        /// <summary>
        /// Fetches the water material's wave parameters and sends this to the underwater effects. if "dynamicMaterial" is enabled, this is performed every frame
        /// Call this function when changing the material
        /// </summary>
        public void UpdateMaterialParameters()
        {
            if (waterMaterial)
            {
                Shader.SetGlobalColor(DestShallowColorID, waterMaterial.GetColor(SourceShallowColorID));
                Shader.SetGlobalColor(DestDeepColorID, waterMaterial.GetColor(SourceDeepColorID));

                Shader.SetGlobalTexture(CausticsTexID, waterMaterial.GetTexture(CausticsTexID));
                Shader.SetGlobalFloat(CausticsTilingID, waterMaterial.GetFloat(CausticsTilingID));
                Shader.SetGlobalFloat(CausticsBrightnessID, waterMaterial.GetFloat(CausticsBrightnessID));
                Shader.SetGlobalFloat(CausticsSpeedID, waterMaterial.GetFloat(CausticsSpeedID));
                
                Shader.SetGlobalVector(_TranslucencyParams, waterMaterial.GetVector(_TranslucencyParams));

                //Debug.Log("Updating water material parameters");
                waveParameters.Update(waterMaterial);
                waveParameters.SetAsGlobal();
            }
        }

        /// <summary>
        /// Configures the assigned water material to render as double-sided, which is required for underwater rendering
        /// </summary>
        public void SetMaterialCulling()
        {
            if (!waterMaterial) return;
            
            waterMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(waterMaterial);
            #endif
            
            Debug.Log("Culling was set to double-sided on " + waterMaterial.name + ". Required for underwater rendering");
        }

        private void Reset()
        {
            gameObject.name = "Underwater Renderer";
            
            //Component first added, fetch the water level for easy setup
            if (WaterObject.Instances.Count == 1)
            {
                waterLevel = WaterObject.Instances[0].transform.position.y;
                waterMaterial = WaterObject.Instances[0].material;
            }
        }

        private void OnEnable()
        {
            Instance = this;
            
            #if UNITY_EDITOR && URP
            if (Application.isPlaying == false)
            {
                if (!PipelineUtilities.RenderFeatureAdded<UnderwaterRenderFeature>())
                {
                    Debug.LogError("The \"Underwater Render Feature\" hasn't been added to the render pipeline. Check the inspector for setup instructions", this);
                    UnityEditor.EditorGUIUtility.PingObject(this);
                }
            }
            #endif

            GetVolumeSettings();
            
            UpdateProperties();
            UpdateMaterialParameters();
            
#if URP
            RenderPipelineManager.beginCameraRendering += TriggerForCamera;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
#endif

#if UNITY_EDITOR
            if(!Application.isPlaying) EditorApplication.update += Update;
#endif
        }

        public void GetVolumeSettings()
        {
            settings = VolumeManager.instance.stack.GetComponent<UnderwaterSettings>();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= Update;
#endif
            Instance = null;
            
            UnderwaterUtilities.ToggleUnderwaterKeyword(false);
        }

        private void OnDestroy()
        {
            UnderwaterUtilities.ToggleUnderwaterKeyword(false);
        }
        
        /// <summary>
        /// If no Underwater Renderer instance is present, this does nothing. The waterLevelSource parameter will be set to "FixedValue"
        /// </summary>
        /// <param name="transform">The world-space position's Y-value will be used</param>
        public static void SetCurrentWaterLevel(Transform transform)
        {
            if (!Instance) return;

            Instance.waterLevelSource = WaterLevelSource.Transform;
           
            Instance.waterLevelTransform = transform;
            Instance.UpdateProperties();
        }

        /// <summary>
        /// If no Underwater Renderer instance is present, this does nothing. The waterLevelSource will be set to "FixedValue"
        /// </summary>
        /// <param name="height">Water level height in world-space</param>
        public static void SetCurrentWaterLevel(float height)
        {
            if (!Instance) return;

            Instance.waterLevelSource = WaterLevelSource.FixedValue;

            Instance.waterLevel = height;
            Instance.UpdateProperties();
        }

        /// <summary>
        /// Configure the water material used for underwater rendering. This affects the water line behaviour and overall appearance.
        /// </summary>
        /// <param name="material"></param>
        public static void SetCurrentWaterMaterial(Material material)
        {
            if (!Instance) return;

            Instance.waterMaterial = material;
            Instance.UpdateMaterialParameters();
            Instance.UpdateProperties();
        }

        /// <summary>
        /// Checks if the bottom of the camera's near-clip plane is below the maximum possible water level
        /// Does not account for rotation on the Z-axis!
        /// </summary>
        /// <param name="targetCamera"></param>
        /// <returns></returns>
        public bool CameraIntersectingWater(Camera targetCamera)
        {
            //Note: Does not account for rotation on Z-axis, should check for both left/right corners of the plane

            //Check if bottom of near plane touches water level,
            return ((UnderwaterUtilities.GetNearPlaneBottomPosition(targetCamera, offset).y) - (waveParameters.height)) <= CurrentWaterLevel;
        }

#if URP
        private void TriggerForCamera(ScriptableRenderContext content, Camera currentCamera)
        {
            //Little caveat, this must be done on a per-camera basis. The render passes are shared by all cameras using the same renderer.
            //Set the keyword before rendering, before any passes execute (including underwater rendering)
            UnderwaterUtilities.ToggleUnderwaterKeyword(CameraIntersectingWater(currentCamera));
        }

        private void OnEndCameraRendering(ScriptableRenderContext content, Camera currentCamera)
        {
            //Disable if necessary for whatever camera comes next, it may not be underwater
            UnderwaterUtilities.ToggleUnderwaterKeyword(false);
        }
#endif
    }
}