using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class CommonPipelineSettings
{
    public bool useDynamicBatching = true;
    public bool useGpuInstancing = true;
    public bool useSrpBatcher = true;
    public bool useHdr = true;
}

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer m_renderer = new CameraRenderer();
    private ShadowSettings m_shadowSettings;
    private CommonPipelineSettings m_commonPipelineSettings;
    private PostFxSettings m_postFxSettings;

    public CustomRenderPipeline(CommonPipelineSettings commonPipelineSettings, ShadowSettings shadowSettings,
        PostFxSettings postFxSettings)
    {
        m_commonPipelineSettings = commonPipelineSettings;
        m_shadowSettings = shadowSettings;
        m_postFxSettings = postFxSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = commonPipelineSettings.useSrpBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_renderer.Render(context, camera, m_commonPipelineSettings, m_shadowSettings, m_postFxSettings);
        }
    }
}