using UnityEngine;
using UnityEngine.Rendering;

public partial class PostFxPass
{
    private enum PassName
    {
        Copy
    }

    private const string c_commandBufferName = "Post FX";

    private CommandBuffer m_commandBuffer = new CommandBuffer()
    {
        name = c_commandBufferName
    };

    private ScriptableRenderContext m_context;
    private Camera m_camera;
    private PostFxSettings m_postFxSettings;
    private static readonly int s_fxSrcId = Shader.PropertyToID("g_PostFxSrc");

    public bool IsActive
    {
        get => m_postFxSettings != null;
    }

    public void Setup(ScriptableRenderContext context, Camera camera, PostFxSettings postFxSettings)
    {
        m_context = context;
        m_camera = camera;
        // Probe don't use fx
        m_postFxSettings = m_camera.cameraType <= CameraType.SceneView ? postFxSettings : null;
        ApplySceneViewState();
    }

    private void Draw(RenderTargetIdentifier from, RenderTargetIdentifier to, PassName passName)
    {
        m_commandBuffer.SetGlobalTexture(s_fxSrcId, from);
        m_commandBuffer.SetRenderTarget(to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        m_commandBuffer.DrawProcedural(Matrix4x4.identity, m_postFxSettings.Material, (int)passName,
            MeshTopology.Triangles, 3);
    }

    public void Render(int srcRtId)
    {
        if (IsActive)
        {
            Draw(srcRtId, BuiltinRenderTextureType.CameraTarget, PassName.Copy);
            m_context.ExecuteCommandBuffer(m_commandBuffer);
            m_commandBuffer.Clear();
        }
    }
}