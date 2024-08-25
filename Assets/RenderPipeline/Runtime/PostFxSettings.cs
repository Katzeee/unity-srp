using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/Custom Post FX Settings")]
public class PostFxSettings : ScriptableObject
{
    [SerializeField] public Shader shader;
    [System.NonSerialized] private Material m_material;

    public Material Material
    {
        get
        {
            if (m_material == null && shader != null)
            {
                m_material = new(shader);
                m_material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_material;
        }
    }
}