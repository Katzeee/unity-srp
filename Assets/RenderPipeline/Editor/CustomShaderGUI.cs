using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor m_materialEditor;
    private Object[] m_materials;
    private MaterialProperty[] m_properties;
    private bool m_showPresets = true;

    private bool Clipping
    {
        set => SetProperty("_Clipping", value ? 1.0f : 0.0f);
    }

    private bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1.0f : 0.0f);
    }

    private bool PremulAlpha
    {
        set => SetProperty("_PremulAlpha", value ? 1.0f : 0.0f);
    }

    private BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    private BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    private RenderQueue RenderQueueNum
    {
        set
        {
            foreach (Material material in m_materials)
            {
                material.renderQueue = (int)value;
            }
        }
    }

    private enum ShadowClipMode
    {
        Off,
        Clip,
        Dither,
    }

    private ShadowClipMode m_shadowClipMode
    {
        set
        {
            if (SetProperty("_ShadowClipMode", (float)value))
            {
                SetKeyword("_SHADOW_CLIP", value == ShadowClipMode.Clip);
                SetKeyword("_SHADOW_DITHER", value == ShadowClipMode.Dither);
            }
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        m_materialEditor = materialEditor;
        m_materials = materialEditor.targets;
        m_properties = properties;
        m_showPresets = EditorGUILayout.Foldout(m_showPresets, "Presets", true);
        if (m_showPresets)
        {
            OpaquePreset();
            FadePreset();
            TransparentPreset();
            ShowDebugInfo();
        }
    }

    bool SetKeyword(string name, bool value)
    {
        return true;
    }

    bool SetProperty(string name, float value)
    {
        var property = FindProperty(name, m_properties, false);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }

        return false;
    }

    bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            m_materialEditor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            ZWrite = true;
            Clipping = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            RenderQueueNum = RenderQueue.Geometry;
        }
    }

    // with premul alpha false
    void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            ZWrite = false;
            Clipping = false;
            PremulAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            RenderQueueNum = RenderQueue.Transparent;
        }
    }

    // with premul alpha true
    void TransparentPreset()
    {
        if (PresetButton("Transparent"))
        {
            ZWrite = false;
            Clipping = false;
            PremulAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            RenderQueueNum = RenderQueue.Transparent;
        }
    }

    void ShowDebugInfo()
    {
        StringBuilder builder = new();
        if (PresetButton("Show Debug Info"))
        {
            foreach (var material in m_materials)
            {
                builder.AppendFormat("{0} ", material.name);
            }

            Debug.Log(builder.ToString());
            builder.Clear();

            foreach (var property in m_properties)
            {
                builder.AppendFormat("{0} ", property.name);
            }

            Debug.Log(builder.ToString());
        }
    }
}