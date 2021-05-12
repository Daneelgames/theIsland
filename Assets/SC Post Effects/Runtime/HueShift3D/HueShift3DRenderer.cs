#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class HueShift3DRenderer : ScriptableRendererFeature
    {
        class HueShift3DRenderPass : PostEffectRenderer<HueShift3D>
        {
            public HueShift3DRenderPass(EffectBaseSettings settings)
            {
                this.settings = settings;
                shaderName = ShaderNames.HueShift3D;
                ProfilerTag = this.ToString();
            }

            public void Setup(ScriptableRenderer renderer)
            {
                this.cameraColorTarget = GetCameraTarget(renderer);
                this.cameraDepthTarget = GetCameraDepthTarget(renderer);
                volumeSettings = VolumeManager.instance.stack.GetComponent<HueShift3D>();
                
                if(volumeSettings && volumeSettings.IsActive()) renderer.EnqueuePass(this);
            }

            public override void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!volumeSettings) return;

                requiresDepthNormals = volumeSettings.IsActive() && volumeSettings.geoInfluence.value > 0f;

                base.ConfigurePass(cmd, cameraTextureDescriptor);
            }
            
            enum Pass
            {
                ColorSpectrum,
                GradientTexture
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (ShouldRender(renderingData) == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                CopyTargets(cmd, renderingData);

                HueShift3D.isOrtho = renderingData.cameraData.camera.orthographic;

                Material.SetVector("_Params", new Vector4(volumeSettings.speed.value, volumeSettings.size.value, volumeSettings.geoInfluence.value, volumeSettings.intensity.value));
                if(volumeSettings.gradientTex.value) Material.SetTexture("_GradientTex", volumeSettings.gradientTex.value);
                
                FinalBlit(this, context, cmd, mainTexID.id, cameraColorTarget, Material, volumeSettings.colorSource.value == (int)HueShift3D.ColorSource.RGBSpectrum ? (int)Pass.ColorSpectrum : (int)Pass.GradientTexture);
            }
        }

        HueShift3DRenderPass m_ScriptablePass;

        [SerializeField]
        public EffectBaseSettings settings = new EffectBaseSettings();

        public override void Create()
        {
            m_ScriptablePass = new HueShift3DRenderPass(settings);
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