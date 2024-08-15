using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer m_renderer = new CameraRenderer();
    private bool m_useDynamicBatching = true;
    private bool m_useGpuInstacing = true;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGpuInstacing, bool useSrpBatcher)
    {
        m_useDynamicBatching = useDynamicBatching;
        m_useGpuInstacing = useGpuInstacing; 
        GraphicsSettings.useScriptableRenderPipelineBatching = useSrpBatcher;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_renderer.Render(context, camera, m_useDynamicBatching, m_useGpuInstacing); 
        } 
    }
}
