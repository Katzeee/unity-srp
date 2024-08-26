using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
#if UNITY_EDITOR
    private string SampleName { get; set; }

    private static readonly string[] s_unsupportedShaderIds = new string[]
    {
        "Always",
        "ForwardBase",
        "PrepassBase",
        "Vertex",
        "VertexLMRGBM",
        "VertexLM"
    };

    private static Shader s_errorShader = Shader.Find("Hidden/InternalErrorShader");

    private void DrawUnsupportedShader()
    {
        var drawingSettings =
            new DrawingSettings(new ShaderTagId(s_unsupportedShaderIds[0]), new SortingSettings(m_camera));
        drawingSettings.overrideShader = s_errorShader;
        for (int i = 1; i < s_unsupportedShaderIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, new ShaderTagId(s_unsupportedShaderIds[i]));
        }

        var filteringSettings = FilteringSettings.defaultValue;
        m_context.DrawRenderers(m_cullingRes, ref drawingSettings, ref filteringSettings);
    }

    private void DrawGizmosBeforeFx()
    {
        if (Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
        }
    }

    private void DrawGizmosAfterFx()
    {
        if (Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
        }
    }

    private void PrepareForSceneWindow()
    {
        if (m_camera.cameraType == CameraType.SceneView)
        {
            // draw ui in scene window
            ScriptableRenderContext.EmitWorldGeometryForSceneView(m_camera);
        }
    }

    private void PrepareCommandBuffer()
    {
        Profiler.BeginSample("Editor Only");
        m_commandBuffer.name = SampleName = m_camera.name;
        Profiler.EndSample();
    }
#else
    const string SampleName = "Camera Renderer";

    private void DrawUnsupportedShader() {}
    private void DrawGizmos() {}
    private void PrepareForSceneWindow() {}
    private void PrepareCommandBuffer() {}
#endif
}