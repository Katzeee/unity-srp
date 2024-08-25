using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CustomRpAsset", menuName = "Rendering/CreateCustomRp")]
public class RpAsset : RenderPipelineAsset
{
    [SerializeField] public CommonPipelineSettings commonPipelineSettings = default;
    [SerializeField] public ShadowSettings shadowSettings = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(commonPipelineSettings, shadowSettings);
    }
}