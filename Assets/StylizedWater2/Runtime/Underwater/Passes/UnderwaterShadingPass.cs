//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    class UnderwaterShadingPass : ScriptableRenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Shading";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);

        private Material Material;
        private UnderwaterRenderFeature.Settings settings;
        private UnderwaterResources resources;
        private UnderwaterRenderFeature renderFeature;

        private bool reconstructSceneNormals;

        public UnderwaterShadingPass(UnderwaterRenderFeature renderFeature)
        {
            this.renderFeature = renderFeature;
            this.settings = renderFeature.settings;
            this.resources = renderFeature.resources;
            Material = UnderwaterRenderFeature.CreateMaterial(ProfilerTag, renderFeature.resources.underwaterShader);
        }

        private RenderTargetHandle mainTexRT;
        private int mainTexID = Shader.PropertyToID("_MainTex");
        private RenderTargetIdentifier cameraColorTarget;
        
        public void Setup(ScriptableRenderer renderer)
        {
            #if URP_10_0_OR_NEWER
            ConfigureInput(ScriptableRenderPassInput.Depth);
            #endif
            
            if (settings.directionalCaustics)
            {
                #if URP_10_0_OR_NEWER
                if(settings.accurateDirectionalCaustics) 
                {
                    ConfigureInput(ScriptableRenderPassInput.Normal);
                    reconstructSceneNormals = false;
                }
                CoreUtils.SetKeyword(Material, UnderwaterRenderFeature.SOURCE_DEPTH_NORMALS_KEYWORD, settings.accurateDirectionalCaustics);
                #else
                reconstructSceneNormals = true;
                CoreUtils.SetKeyword(Material, UnderwaterRenderFeature.SOURCE_DEPTH_NORMALS_KEYWORD, false);
                #endif
            }
            
            CoreUtils.SetKeyword(Material, UnderwaterRenderFeature.DEPTH_NORMALS_KEYWORD, settings.directionalCaustics);
            CoreUtils.SetKeyword(Material, UnderwaterRenderFeature.UNLIT_KEYWORD, renderFeature.materialSettings.unlit);
            CoreUtils.SetKeyword(Material, UnderwaterRenderFeature.TRANSLUCENCY_KEYWORD, renderFeature.materialSettings.translucency);
            CoreUtils.SetKeyword(Material, UnderwaterRenderFeature.DEPTH_PREPASS_ENABLED_KEYWORD, settings.excludeAboveWater);

            #if !URP_10_0_OR_NEWER
            //otherwise fetched in Execute function, no longer allowed from a ScriptableRenderFeature setup function (target may be not be created yet, or was disposed)
            this.cameraColorTarget = renderer.cameraColorTarget;
            #endif
        }
        
#if URP_9_0_OR_NEWER
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
#else
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
#endif
        {
            #if URP_9_0_OR_NEWER
            RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            #endif
            
            ConfigurePass(cmd, cameraTextureDescriptor);
        }

        public void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //cameraTextureDescriptor.msaaSamples = 1;
            mainTexRT.id = mainTexID;
            
            cmd.GetTemporaryRT(mainTexRT.id, cameraTextureDescriptor);
            cmd.SetGlobalTexture(mainTexRT.id, mainTexID);
            
            if(reconstructSceneNormals) UnderwaterLighting.DepthNormals.Configure(cmd, cameraTextureDescriptor, this.resources);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                #if URP_10_0_OR_NEWER
                //Color target can now only be fetched inside a ScriptableRenderPass
                this.cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
                #endif

                Blit(cmd, cameraColorTarget, mainTexRT.id);

                UnderwaterLighting.PassAmbientLighting(this, cmd, Material);
                UnderwaterLighting.PassMainLight(cmd, renderingData);
                
                if(reconstructSceneNormals) UnderwaterLighting.DepthNormals.Generate(this, cmd, renderingData);

                Blit(cmd, mainTexRT.id, cameraColorTarget, Material, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

#if URP_9_0_OR_NEWER
        public override void OnCameraCleanup(CommandBuffer cmd)
#else
        public override void FrameCleanup(CommandBuffer cmd)
#endif
        {
            cmd.ReleaseTemporaryRT(mainTexID);
            if(reconstructSceneNormals) UnderwaterLighting.DepthNormals.Cleanup(cmd);
        }
    }

}
#endif