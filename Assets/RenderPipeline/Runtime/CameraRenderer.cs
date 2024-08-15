using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext m_context;
    private Camera m_camera;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        m_context = context;
        m_camera = camera;

        DrawVisibleGeometry();

        Submit();
    }

    private void Submit()
    {
        m_context.Submit();
    }

    private void DrawVisibleGeometry()
    {
        m_context.DrawSkybox(m_camera);
    }
}
