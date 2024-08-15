using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CustomRpAsset", menuName = "Rendering/CreateCustomRp")]
public class RpAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}
