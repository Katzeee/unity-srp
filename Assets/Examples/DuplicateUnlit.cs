using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DuplicateUnlit : MonoBehaviour
{
    private static readonly int s_baseColorId = Shader.PropertyToID("_BaseColor");
    private Mesh m_mesh;
    private Material m_material;
    private const int c_numberOfInstances = 1023;
    private Matrix4x4[] m_modelMatrix = new Matrix4x4[c_numberOfInstances];
    private Vector4[] m_modelColor = new Vector4[c_numberOfInstances];
    private MaterialPropertyBlock m_materialPropertyBlock;
    
    void Awake()
    {
        m_materialPropertyBlock = new();
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_material = GetComponent<MeshRenderer>().material;
        for (int i = 0; i < c_numberOfInstances; i++)
        {
            m_modelMatrix[i] = Matrix4x4.TRS(transform.position + Random.insideUnitSphere * 10f,
                Quaternion.Euler(Random.value, Random.value, Random.value), Vector3.one * Random.Range(0.5f, 1.5f));
            m_modelColor[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }
        m_materialPropertyBlock.SetVectorArray(s_baseColorId, m_modelColor);
    }

    private void Update()
    {
        Graphics.DrawMeshInstanced(m_mesh, 0, m_material, m_modelMatrix, c_numberOfInstances, m_materialPropertyBlock);
    }
}
