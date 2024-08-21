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
    private const int c_maxDirLightShadowCount = 1;
    private int m_dirLightShadowCount = 0;
    private static int s_dirLightShadowMapId = Shader.PropertyToID("g_dirLightShadowMap");
    private static int s_worldToDirLightClipSpaceMatrixId = Shader.PropertyToID("g_worldToDirLightClipSpaceMatrix");
    private static Matrix4x4[] s_worldToDirLightClipSpaceMatrix = new Matrix4x4[c_maxDirLightShadowCount];

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
        m_commandBuffer.GetTemporaryRT(s_dirLightShadowMapId, textureSize, textureSize, 32, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        m_commandBuffer.SetRenderTarget(s_dirLightShadowMapId, RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store);
        m_commandBuffer.ClearRenderTarget(true, false, Color.white);
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
            ConvertToAtlasMatrix(blockSize, i);
        }

        m_commandBuffer.SetGlobalMatrixArray(s_worldToDirLightClipSpaceMatrixId, s_worldToDirLightClipSpaceMatrix);
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
        s_worldToDirLightClipSpaceMatrix[blockIndex] = projMatrix * viewMatrix;
        ExcuteBuffer();
        m_context.DrawShadows(ref shadowSettings);
    }

    private void ConvertToAtlasMatrix(int blockSize, int index)
    {
        // convert uv from -1 - 1 to 0 - 1
        s_worldToDirLightClipSpaceMatrix[index] = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0)) *
                                                  Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 1.0f)) *
                                                  s_worldToDirLightClipSpaceMatrix[index];
        // convert z if reversed z
        if (SystemInfo.usesReversedZBuffer)
        {
            s_worldToDirLightClipSpaceMatrix[index] =
                Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * s_worldToDirLightClipSpaceMatrix[index];
        }

        // SystemInfo.
        // float scaleSize = 1.0f / (float)blockSize;
        // float xOffset = (index % blockSize) * scaleSize;
        // float yOffset = ((float)index / blockSize) * scaleSize;
        // Matrix4x4.Translate(new Vector3(xOffset, yOffset, 0)) * Matrix4x4.Scale(new Vector3(scaleSize, scaleSize, 0));
    }

    public void CleanUp()
    {
        m_commandBuffer.ReleaseTemporaryRT(s_dirLightShadowMapId);
        ExcuteBuffer();
    }

    private void ExcuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();
    }
}