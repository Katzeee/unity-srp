using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer m_renderer = new CameraRenderer();
    private bool m_useDynamicBatching = true;
    private bool m_useGpuInstacing = true;
    private ShadowSettings m_shadowSettings;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGpuInstacing, bool useSrpBatcher, ShadowSettings shadowSettings)
    {
        m_useDynamicBatching = useDynamicBatching;
        m_useGpuInstacing = useGpuInstacing;
        m_shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSrpBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_renderer.Render(context, camera, m_useDynamicBatching, m_useGpuInstacing, m_shadowSettings); 
        } 
    }
}
