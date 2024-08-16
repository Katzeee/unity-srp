using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
public class Lighting
{
    private const string c_commandBufferName = "Lighting";

    private CommandBuffer m_commandBuffer = new CommandBuffer()
    {
        name = c_commandBufferName
    };

    private static int s_dirLightCountId = Shader.PropertyToID("g_DirectionalLightCount");
    private static int s_dirLightColorId = Shader.PropertyToID("g_DirectionalLightColors");
    private static int s_dirLightDirId = Shader.PropertyToID("g_DirectionalLightDirs");
    private CullingResults m_cullingRes;
    private const int c_maxDirLightCount = 4;
    private static Vector4[] s_dirLightColors = new Vector4[c_maxDirLightCount];
    private static Vector4[] s_dirLightDirs = new Vector4[c_maxDirLightCount];

    public void Setup(ScriptableRenderContext context, CullingResults cullingRes)
    {
        m_cullingRes = cullingRes;
        m_commandBuffer.BeginSample(c_commandBufferName);
        SetupLights();
        m_commandBuffer.EndSample(c_commandBufferName);
        context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();
    }

    public void SetupLights()
    {
        var lights = m_cullingRes.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].lightType == LightType.Directional && dirLightCount < c_maxDirLightCount)
            {
                SetupDirLight(dirLightCount, lights[i]);
                dirLightCount++;
            }
        }

        m_commandBuffer.SetGlobalInt(s_dirLightCountId, dirLightCount);
        m_commandBuffer.SetGlobalVectorArray(s_dirLightColorId, s_dirLightColors);
        m_commandBuffer.SetGlobalVectorArray(s_dirLightDirId, s_dirLightDirs);
    }

    private void SetupDirLight(int index, VisibleLight light)
    {
        s_dirLightColors[index] = light.finalColor;
        // z axis is the forward vector of the directional light
        s_dirLightDirs[index] = -light.localToWorldMatrix.GetColumn(2);
    }
}

