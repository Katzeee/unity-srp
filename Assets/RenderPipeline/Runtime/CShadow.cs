using UnityEngine;
using UnityEngine.Rendering;

public class CShadow
{
    private const string c_commandBufferName = "Shadow";

    private CommandBuffer m_commandBuffer = new CommandBuffer()
    {
        name = c_commandBufferName
    };

    private ScriptableRenderContext m_context;
    private CullingResults m_cullingRes;

    private ShadowSettings m_shadowSettings;
    private const int c_maxDirLightShadowCount = 4;
    private int m_dirLightShadowCount = 0;
    private static int s_dirLightShadowId = Shader.PropertyToID("g_dirLightShadowMap");

    struct SShadowDirLight
    {
        public int index;
    }

    private SShadowDirLight[] m_shadowDirLights = new SShadowDirLight[c_maxDirLightShadowCount];

    public void Setup(ScriptableRenderContext context, CullingResults cullingRes, ShadowSettings shadowSettings)
    {
        m_context = context;
        m_cullingRes = cullingRes;
        m_shadowSettings = shadowSettings;
        m_dirLightShadowCount = 0;
    }

    public void ReserveDirShadows(Light light, int lightIndex)
    {
        if (m_dirLightShadowCount < c_maxDirLightShadowCount && light.shadows != LightShadows.None &&
            light.shadowStrength > 0f && m_cullingRes.GetShadowCasterBounds(lightIndex, out Bounds b))
        {
            m_shadowDirLights[m_dirLightShadowCount++] = new SShadowDirLight { index = lightIndex };
        }
    }

    public void Render()
    {
        if (m_dirLightShadowCount > 0)
        {
            RenderDirLightShadows();
        }
    }

    private void RenderDirLightShadows()
    {
        // create RT
        int textureSize = (int)m_shadowSettings.dirLight.textureSize; // one axis
        m_commandBuffer.GetTemporaryRT(s_dirLightShadowId, textureSize, textureSize, 32, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        m_commandBuffer.SetRenderTarget(s_dirLightShadowId, RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store);
        m_commandBuffer.ClearRenderTarget(true, false, Color.clear);
        ExcuteBuffer();

        // render shadow map 
        m_commandBuffer.BeginSample(c_commandBufferName);
        ExcuteBuffer();
        // divide shadow RT to blocks
        int blockSize = m_dirLightShadowCount <= 1 ? 1 : 2; // one axis
        int tileSize = textureSize / blockSize;
        for (int i = 0; i < m_dirLightShadowCount; i++)
        {
            RenderDirLightShadow(i, blockSize, tileSize);
        }

        m_commandBuffer.EndSample(c_commandBufferName);
        ExcuteBuffer();
    }

    private void RenderDirLightShadow(int blockIndex, int blockSize, int tileSize)
    {
        var light = m_shadowDirLights[blockIndex];
        var shadowSettings =
            new ShadowDrawingSettings(m_cullingRes, light.index, BatchCullingProjectionType.Orthographic);
        m_cullingRes.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.index, 0, 1, Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData);
        shadowSettings.splitData = shadowSplitData;

        var viewPort = new Rect(new Vector2(blockIndex % blockSize, blockIndex / blockSize) * tileSize,
            new Vector2(tileSize, tileSize));
        m_commandBuffer.SetViewport(viewPort);
        m_commandBuffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExcuteBuffer();
        m_context.DrawShadows(ref shadowSettings);
    }

    public void CleanUp()
    {
        m_commandBuffer.ReleaseTemporaryRT(s_dirLightShadowId);
        ExcuteBuffer();
    }

    private void ExcuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();
    }
}