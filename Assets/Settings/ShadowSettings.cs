using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)] 
    public float maxDistance = 100f;
    
    public enum TextureSize
    {
        _256 = 256, _512 = 512, _1024 = 1024 
    }
    
    [System.Serializable]
    public struct DirectionalLight
    {
        public TextureSize textureSize;
    }

    public DirectionalLight dirLight = new DirectionalLight
    {
        textureSize = TextureSize._1024
    };

}


