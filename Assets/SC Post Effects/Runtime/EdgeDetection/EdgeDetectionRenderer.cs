#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine;
using UnityEngine.Rendering;

namespace SCPE
{
#if URP
    public class EdgeDetectionRenderer : ScriptableRendererFeature
    {
        
        class EdgeDetectionRenderPass : PostEffectRenderer<EdgeDetection>
        {
            public EdgeDetectionRenderPass(EffectBaseSettings settings)
            {
                this.settings = settings;
                shaderName = ShaderNames.EdgeDetection;
                ProfilerTag = this.ToString();
            }

            public void Setup(ScriptableRenderer renderer)
            {
                this.cameraColorTarget = GetCameraTarget(renderer);
                this.cameraDepthTarget = GetCameraDepthTarget(renderer);
                volumeSettings = VolumeManager.instance.stack.GetComponent<EdgeDetection>();
                
                if(volumeSettings && volumeSettings.IsActive()) renderer.EnqueuePass(this);
            }

            public override void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!volumeSettings) return;

                requiresDepth = true;
                requiresDepthNormals = volumeSettings.IsActive() && (volumeSettings.mode == EdgeDetection.EdgeDetectMode.CrossDepthNormals || volumeSettings.mode == EdgeDetection.EdgeDetectMode.DepthNormals);
                
                base.ConfigurePass(cmd, cameraTextureDescriptor);
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (ShouldRender(renderingData) == false) return;
                
                base.Execute(context, ref renderingData);

                var cmd = CommandBufferPool.Get(ProfilerTag);

                CopyTargets(cmd, renderingData);

                Vector2 sensitivity = new Vector2(volumeSettings.sensitivityDepth.value, volumeSettings.sensitivityNormals.value);
                Material.SetVector("_Sensitivity", sensitivity);
                Material.SetFloat("_BackgroundFade", (volumeSettings.debug.value) ? 1f : 0f);
                Material.SetFloat("_EdgeSize", volumeSettings.edgeSize.value);
                Material.SetFloat("_Exponent", volumeSettings.edgeExp.value);
                Material.SetFloat("_Threshold", volumeSettings.lumThreshold.value);
                Material.SetColor("_EdgeColor", volumeSettings.edgeColor.value);
                Material.SetFloat("_EdgeOpacity", volumeSettings.edgeOpacity.value);

                Material.SetVector("_FadeParams", new Vector4(volumeSettings.startFadeDistance.value, volumeSettings.endFadeDistance.value, (volumeSettings.invertFadeDistance.value) ? 1 : 0, volumeSettings.distanceFade.value ? 1 : 0));

                Material.SetVector("_SobelParams", new Vector4((volumeSettings.sobelThin.value) ? 1 : 0, 0, 0, 0));
                
                FinalBlit(this, context, cmd, mainTexID.id, cameraColorTarget, Material, (int)volumeSettings.mode.value);
            }

            public override void Cleanup(CommandBuffer cmd)
            {
                requiresDepthNormals = false;
                requiresDepth = false;
                
                base.Cleanup(cmd);
            }
        }
        
        EdgeDetectionRenderPass m_ScriptablePass;

        [SerializeField]
        public EffectBaseSettings settings = new EffectBaseSettings();
        
        public override void Create()
        {
            m_ScriptablePass = new EdgeDetectionRenderPass(settings);
            m_ScriptablePass.renderPassEvent = settings.injectionPoint;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_ScriptablePass.CheckForStackedRendering(renderer, renderingData.cameraData);
            m_ScriptablePass.Setup(renderer);
        }
    }
#endif
}
