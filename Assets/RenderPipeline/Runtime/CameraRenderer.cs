using UnityEngine;
using UnityEngine.Rendering;

public class PipelineSettings
{
    public CommonPipelineSettings commonPipelineSettings;
    public ShadowSettings shadowSettings;
    public PostFxSettings postFxSettings;
}

public abstract partial class CameraRenderer
{
    protected ScriptableRenderContext m_context;
    protected Camera m_camera;
    protected CullingResults m_cullingRes;
    protected CommandBuffer m_commandBuffer = new();
    protected PipelineSettings m_pipelineSettings;
    protected static readonly int s_frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");
    protected bool m_useHdr;

    protected static string[] s_supportedRenderIds = new[]
    {
        "SRPDefaultUnlit",
        "CustomLit",
    };

    protected virtual bool BeforeRender()
    {
        PrepareProfileTag();
        PrepareForSceneWindow();
        return Cull(m_pipelineSettings.shadowSettings.maxDistance);
    }

    protected virtual void OnRender()
    {
    }

    protected virtual void AfterRender()
    {
    }

    public void Render(ScriptableRenderContext context, Camera camera, PipelineSettings pipelineSettings)
    {
        m_context = context;
        m_camera = camera;
        m_pipelineSettings = pipelineSettings;
        m_useHdr = m_camera.allowHDR && m_pipelineSettings.commonPipelineSettings.useHdr;

        if (BeforeRender())
        {
            OnRender();
            AfterRender();
        }
    }

    private bool Cull(float maxShadowDistance)
    {
        ScriptableCullingParameters p;
        if (m_camera.TryGetCullingParameters(out p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, m_camera.farClipPlane);
            m_cullingRes = m_context.Cull(ref p);
            return true;
        }

        return false;
    }

    protected void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();
    }

    protected void Submit()
    {
        m_commandBuffer.EndSample(SampleName);
        ExecuteBuffer();
        m_context.Submit();
    }
}