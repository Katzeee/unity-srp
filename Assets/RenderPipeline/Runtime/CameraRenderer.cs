using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext m_context;
    private Camera m_camera;
    private CullingResults m_cullingRes;
    private CommandBuffer m_commandBuffer = new();
    private Lighting m_lighting = new();

    private static string[] s_supportedRenderIds = new[]
    {
        "SRPDefaultUnlit",
        "CustomLit",
    };

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGpuInstancing,
        ShadowSettings shadowSettings)
    {
        m_context = context;
        m_camera = camera;
        
        PrepareCommandBuffer();
        PrepareForSceneWindow();
        Cull(shadowSettings.maxDistance);
        m_commandBuffer.BeginSample(SampleName);
        ExecuteBuffer();
        m_lighting.Setup(m_context, m_cullingRes, shadowSettings);
        m_commandBuffer.EndSample(SampleName);
        Setup();
        
        DrawVisibleGeometry(useDynamicBatching, useGpuInstancing);
        DrawUnsupportedShader();
        DrawGizmos();
        m_lighting.CleanUp();
        
        Submit();
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

    // before drawing, clean the framebuffer
    private void Setup()
    {
        m_context.SetupCameraProperties(m_camera);
        CameraClearFlags clearFlags = m_camera.clearFlags;
        m_commandBuffer.ClearRenderTarget(clearFlags <= CameraClearFlags.Depth, clearFlags <= CameraClearFlags.Color,
            clearFlags == CameraClearFlags.Color ? m_camera.backgroundColor.linear : Color.clear);
        // QUESTION: must excute buffer after begin sample?
        m_commandBuffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();
    }

    private void Submit()
    {
        m_commandBuffer.EndSample(SampleName);
        ExecuteBuffer();
        m_context.Submit();
    }

    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGpuInstacing)
    {
        var sortingSettings = new SortingSettings(m_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(new ShaderTagId(s_supportedRenderIds[0]), sortingSettings)
        {
            enableInstancing = useGpuInstacing,
            enableDynamicBatching = useDynamicBatching
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