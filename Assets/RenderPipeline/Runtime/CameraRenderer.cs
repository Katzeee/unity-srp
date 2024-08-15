using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext m_context;
    private Camera m_camera;
    private CullingResults m_cullingRes;
    private const string c_bufferName = "Render Camera";
    private CommandBuffer m_commandBuffer = new CommandBuffer{ name = c_bufferName };

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        m_context = context;
        m_camera = camera;

        Cull();
        Setup();
        DrawVisibleGeometry();
        Submit();
    }

    private bool Cull()
    {
        ScriptableCullingParameters p;
        if (m_camera.TryGetCullingParameters(out p))
        {
            m_cullingRes = m_context.Cull(ref p);
            return true; 
        }
        return false;
    }

    private void Setup()
    {
        m_context.SetupCameraProperties(m_camera);
        m_commandBuffer.ClearRenderTarget(true, true, Color.clear);
        m_commandBuffer.BeginSample(c_bufferName);
        ExecteBuffer();
    }

    private void ExecteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();
    }

    private void Submit()
    {
        m_commandBuffer.EndSample(c_bufferName);
        ExecteBuffer();
        m_context.Submit();
    }

    private void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(m_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        m_context.DrawRenderers(m_cullingRes, ref drawingSettings, ref filteringSettings);
        m_context.DrawSkybox(m_camera);
    }
}
