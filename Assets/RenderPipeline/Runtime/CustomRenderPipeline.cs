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
    private CameraRenderer m_renderer = new ForwardRenderer();
    private PipelineSettings m_pipelineSettings = new();

    public CustomRenderPipeline(CommonPipelineSettings commonPipelineSettings, ShadowSettings shadowSettings,
        PostFxSettings postFxSettings)
    {
        m_pipelineSettings.commonPipelineSettings = commonPipelineSettings;
        m_pipelineSettings.shadowSettings = shadowSettings;
        m_pipelineSettings.postFxSettings = postFxSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = commonPipelineSettings.useSrpBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_renderer.Render(context, camera, m_pipelineSettings);
        }
    }
}