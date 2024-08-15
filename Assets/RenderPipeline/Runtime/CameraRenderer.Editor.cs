using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private partial void DrawUnsupportedShader();
    private partial void DrawGizmos();
    private partial void PrepareForSceneWindow();
    
#if UNITY_EDITOR
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
    
    private partial void DrawUnsupportedShader()
    {
        var drawingSettings = new DrawingSettings(new ShaderTagId(s_unsupportedShaderIds[0]), new SortingSettings(m_camera));
        drawingSettings.overrideShader = s_errorShader;
        for (int i = 1; i < s_unsupportedShaderIds.Length; i++)
        {   
            drawingSettings.SetShaderPassName(i, new ShaderTagId(s_unsupportedShaderIds[i]));
        }

        var filteringSettings = FilteringSettings.defaultValue;
        m_context.DrawRenderers(m_cullingRes, ref drawingSettings, ref filteringSettings);
    }

    private partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
            m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
        }
    }

    private partial void PrepareForSceneWindow()
    {
        if (m_camera.cameraType == CameraType.SceneView)
        {
            // draw ui in scene window
            ScriptableRenderContext.EmitWorldGeometryForSceneView(m_camera);
        }
    }
#endif
}