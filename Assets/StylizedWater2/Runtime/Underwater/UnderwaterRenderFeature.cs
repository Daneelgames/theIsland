//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System;
using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    public class UnderwaterRenderFeature : ScriptableRendererFeature
    {
        //Shared resources, ensures they're included in a build when the render feature is in use
        [SerializeField]
        #if !SWS_DEV
        [HideInInspector]
        #endif
        public UnderwaterResources resources;

        [Serializable]
        public class Settings
        {
            [Tooltip("Enable support for split-screen. This has the performance implication of the effect rendering for a camera, even if its not touching the water, but another one is!")]
            public bool splitScreenSupport = false;
            [Tooltip("If there are a lot of water meshes in the scene, culling them based on visibility may yield better performance. Tradeoff between CPU load and draw calls")]
            public bool frustrumCulling;
            
            [Header("Quality/Performance")]
            public bool allowBlur = true;
            public bool allowDistortion = true;
            
            [Space]
            
            [Tooltip("Limit caustics only to parts of a surface where sun light hits it")]
            public bool directionalCaustics;
            [Tooltip("(Requires Unity 2020.2+) Use the depth normals texture created from the Depth Normals pre-pass." +
                     "\n\nThis can negatively impact performance if the game isn't already optimized for draw calls!" +
                     "\n\nIf disabled, normals will be reconstructed from the depth texture")]
            public bool accurateDirectionalCaustics = false;
            
            [Space]
            
            [Tooltip("If enabled, anything above the water will not receive fog. Disable for improved performance")]
            public bool excludeAboveWater = true;
           
            public enum DistortionMode
            {
                ScreenSpace,
                CameraSpace
            }
            [Tooltip("Screen-space mode is faster, but distortion will appear to move with the camera\n\n" +
                     "Camera-space mode looks better, but requires more calculations")]
            public DistortionMode distortionMode = DistortionMode.CameraSpace;
            [Tooltip("Attempts to create a glass-like appearance by refracting the scene geometry behind the water line. Note this does not refract the water surface behind it")]
            public bool waterlineRefraction = true;
        }
        public Settings settings = new Settings();
        
        private WaterDepthPrePass depthPrePass;
        private UnderwaterMaskPass maskPass;
        private UnderwaterLinePass waterLinePass;
        private UnderwaterShadingPass underwaterShadingPass;
        private UnderwaterPost underwaterPostPass;
        
        public struct MaterialSettings
        {
            public bool unlit;
            public bool translucency;
        }
        public MaterialSettings materialSettings;
        
        public const string UNLIT_KEYWORD = "_UNLIT";
        public const string TRANSLUCENCY_KEYWORD = "_TRANSLUCENCY";
        public const string DEPTH_PREPASS_ENABLED_KEYWORD = "_DEPTH_PREPASS_ENABLED";
        public const string DEPTH_NORMALS_KEYWORD = "_REQUIRE_DEPTH_NORMALS";
        public const string SOURCE_DEPTH_NORMALS_KEYWORD = "_SOURCE_DEPTH_NORMALS";
        public const string REFRACTION_KEYWORD = "_REFRACTION";
        
        private void Reset() //Note: editor-only
        {
            if (!resources) resources = UnderwaterResources.Find();
            
            #if UNITY_IOS || UNITY_TVOS || UNITY_ANDROID || UNITY_SWITCH
            //Recommended fastest settings
            settings.excludeAboveWater = false;
            settings.directionalCaustics = false;
            settings.accurateDirectionalCaustics = false;
            settings.allowBlur = false;
            settings.allowDistortion = false;
            settings.distortionMode = DistortionMode.ScreenSpace;
            settings.waterlineRefraction = false;
            #endif
        }

        public override void Create()
        {
            #if UNITY_EDITOR
            if (!resources) resources = UnderwaterResources.Find();
            #endif
            
            depthPrePass = new WaterDepthPrePass();
            depthPrePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            
            maskPass = new UnderwaterMaskPass(this);
            maskPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            
            waterLinePass = new UnderwaterLinePass(this);
            waterLinePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            underwaterShadingPass = new UnderwaterShadingPass(this);
            underwaterShadingPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            
            underwaterPostPass = new UnderwaterPost(this);
            underwaterPostPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        private bool cameraIntersecting;

        private bool RequiresPostProcessingPass(UnderwaterRenderer renderer)
        {
            return (renderer.enableBlur && settings.allowBlur) || (renderer.enableDistortion && settings.allowDistortion);
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!resources || UnderwaterRenderer.Instance == null || UnderwaterRenderer.Instance.waterMaterial == null) return;

            if (!UnderwaterRenderer.EnableRendering) return;
            
            #if UNITY_EDITOR
            //Skip rendering if editing a prefab
            #if UNITY_2021_2_OR_NEWER
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            #else
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            #endif
            #endif

            //Camera stacking and depth-based post processing is essentially non-functional.
            //All effects render twice to the screen, causing double brightness. Next to fog causing overlay objects to appear transparent
            //Best option is to not render anything for overlay cameras
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay || renderingData.cameraData.camera.cameraType == CameraType.Reflection) return;

#if UNITY_EDITOR
            //Skip if post-processing is disabled in scene-view
            if (renderingData.cameraData.cameraType == CameraType.SceneView && UnityEditor.SceneView.lastActiveSceneView && !UnityEditor.SceneView.lastActiveSceneView.sceneViewState.showImageEffects) return;
#endif

            cameraIntersecting = UnderwaterRenderer.Instance.CameraIntersectingWater(renderingData.cameraData.camera);

            if (cameraIntersecting || settings.splitScreenSupport)
            {
                materialSettings.unlit = UnderwaterRenderer.Instance.waterMaterial.IsKeywordEnabled(UNLIT_KEYWORD);
                materialSettings.translucency = UnderwaterRenderer.Instance.waterMaterial.IsKeywordEnabled(TRANSLUCENCY_KEYWORD);
                
                maskPass.Setup();
                renderer.EnqueuePass(maskPass);

                if (settings.excludeAboveWater && renderingData.cameraData.camera.orthographic == false)
                {
                    depthPrePass.Setup(settings.frustrumCulling);
                    renderer.EnqueuePass(depthPrePass);
                }

                underwaterShadingPass.Setup(renderer);
                renderer.EnqueuePass(underwaterShadingPass);
                
                if (RequiresPostProcessingPass(UnderwaterRenderer.Instance))
                {
                    underwaterPostPass.Setup(settings, renderer);
                    renderer.EnqueuePass(underwaterPostPass);
                }
                
                waterLinePass.Setup();
                renderer.EnqueuePass(waterLinePass);
            }
        }

        public static Material CreateMaterial(string profilerTag, Shader shader)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!shader)
            {
                //Debug.LogError("[" + profilerTag + "] Shader could not be found, ensure all files are imported");
                return null;
            }
            #endif
            
            return CoreUtils.CreateEngineMaterial(shader);
        }
    }
}
#endif