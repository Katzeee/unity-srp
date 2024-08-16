using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    public Color baseColor = Color.white;
    private static readonly int s_baseColorId = Shader.PropertyToID("_BaseColor");
    
    void Awake()
    {
        MaterialPropertyBlock materialPropertyBlock = new(); 
        materialPropertyBlock.SetColor(s_baseColorId, baseColor);
        GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
    }
}
