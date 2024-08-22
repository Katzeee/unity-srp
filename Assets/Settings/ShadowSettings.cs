using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)] public float maxDistance = 100f;
    // fade at max distance
    [Range(0.001f, 1f)] public float fadeDistance = 0.1f;
    // fade at last cascade
    [Range(0.001f, 1f)] public float fadeCascade = 0.1f;

    public enum TextureSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048
    }

    [System.Serializable]
    public struct SDirectionalLightShadow
    {
        public TextureSize textureSize;
        [Range(1, 4)] public int cascadeCount;
        // [Range(0, 1f)] public float mixRatio;
    }

    public SDirectionalLightShadow dirLightShadow = new SDirectionalLightShadow
    {
        textureSize = TextureSize._2048,
        cascadeCount = 4,
        // mixRatio = 0.8f
    };
}