using UnityEngine;
using UnityEngine.Rendering;

public class ForwardRenderer : CameraRenderer
{
    protected Lighting m_lighting = new();
    protected PostFxPass m_postFxPass = new PostFxPass();
    protected ComputerShaderPass m_computePass = new();

    protected override bool BeforeRender()
    {
        if (base.BeforeRender())
        {
            m_commandBuffer.BeginSample(SampleName);
            ExecuteBuffer();
            m_lighting.Setup(m_context, m_cullingRes, m_pipelineSettings.shadowSettings);
            m_commandBuffer.EndSample(SampleName);
            m_postFxPass.Setup(m_context, m_camera, m_pipelineSettings.postFxSettings);
            m_computePass.Setup(m_context, m_camera);

            Setup();
            return true;
        }

        return false;
    }

    protected override void OnRender()
    {
        base.OnRender();

        DrawVisibleGeometry();
        DrawUnsupportedShader();
        DrawGizmosBeforeFx();
        m_computePass.Render(s_frameBufferId);
        // m_postFxPass.Render(s_frameBufferId);
        DrawGizmosAfterFx();
    }

    protected override void AfterRender()
    {
        base.AfterRender();
        m_lighting.CleanUp();
        if (m_postFxPass.IsActive)
        {
            m_commandBuffer.ReleaseTemporaryRT(s_frameBufferId);
        }

        Submit();
    }


    // before drawing, clean the framebuffer
    protected void Setup()
    {
        m_context.SetupCameraProperties(m_camera);
        if (m_postFxPass.IsActive)
        {
            m_commandBuffer.GetTemporaryRT(s_frameBufferId, m_camera.pixelWidth, m_camera.pixelHeight, 32,
                FilterMode.Bilinear, m_useHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            m_commandBuffer.SetRenderTarget(s_frameBufferId, RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store);
        }

        CameraClearFlags clearFlags = m_camera.clearFlags;
        CCommonUtils.ClearFrameBuffer(m_commandBuffer, clearFlags, Color.clear);
        // QUESTION: must excute buffer after begin sample?
        m_commandBuffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    protected void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(m_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(new ShaderTagId(s_supportedRenderIds[0]), sortingSettings)
        {
            enableInstancing = m_pipelineSettings.commonPipelineSettings.useGpuInstancing,
            enableDynamicBatching = m_pipelineSettings.commonPipelineSettings.useDynamicBatching
        };
        for (int i = 1; i < s_supportedRenderIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, new ShaderTagId(s_supportedRenderIds[i]));
        }

        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        // draw opaque
        m_context.DrawRenderers(m_cullingRes, ref drawingSettings, ref filteringSettings);
        // draw skybox
        m_context.DrawSkybox(m_camera);
        // draw transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        m_context.DrawRenderers(m_cullingRes, ref drawingSettings, ref filteringSettings);
    }
}