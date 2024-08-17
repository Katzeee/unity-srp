using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CustomRpAsset", menuName = "Rendering/CreateCustomRp")]
public class RpAsset : RenderPipelineAsset
{
    public bool useDynamicBatching = true;
    public bool useGpuInstancing = true;
    public bool useSrpBatcher = true;
    [SerializeField] public ShadowSettings shadowSettings = default;
    
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGpuInstancing, useSrpBatcher, shadowSettings);
    }
}
