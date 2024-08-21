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
    private const int c_maxDirLightShadowCascadeCount = 4;
    private int m_dirLightShadowCount = 0;

    // shader variable id
    private static int s_dirLightShadowMapId = Shader.PropertyToID("g_dirLightShadowMap");
    private static int s_worldToDirLightShadowMatrixId = Shader.PropertyToID("g_worldToDirLightShadowMatrix");
    private static int s_dirLightShadowDataPackedId = Shader.PropertyToID("g_dirLightShadowDataPacked");
    private static int s_dirLightShadowCountId = Shader.PropertyToID("g_dirLightShadowCount");
    private static int s_cascadeSphereId = Shader.PropertyToID("g_cascadeSphere");

    // shader variables
    private static Matrix4x4[] s_worldToDirLightShadowMatrix =
        new Matrix4x4[c_maxDirLightShadowCount * c_maxDirLightShadowCascadeCount];

    private static Vector4[] s_dirLightShadowDataPacked = new Vector4[c_maxDirLightShadowCount];
    private static Vector4[] s_cascadeSphere = new Vector4[c_maxDirLightShadowCascadeCount];

    struct SShadowDirLight
    {
        public int index;
        public float shadowStrength;
    }

    private SShadowDirLight[] m_shadowDirLights = new SShadowDirLight[c_maxDirLightShadowCount];

    public void Setup(ScriptableRenderContext context, CullingResults cullingRes, ShadowSettings shadowSettings)
    {
        m_context = context;
        m_cullingRes = cullingRes;
        m_shadowSettings = shadowSettings;
        m_dirLightShadowCount = 0;
    }

    // called by Lighting to reserve light data
    public void ReserveDirShadows(Light light, int lightIndex)
    {
        if (m_dirLightShadowCount < c_maxDirLightShadowCount && light.shadows != LightShadows.None &&
            light.shadowStrength > 0f && m_cullingRes.GetShadowCasterBounds(lightIndex, out Bounds b))
        {
            m_shadowDirLights[m_dirLightShadowCount] = new SShadowDirLight
                { index = lightIndex, shadowStrength = light.shadowStrength };
            m_dirLightShadowCount++;
        }
    }

    public void Render()
    {
        if (m_dirLightShadowCount > 0)
        {
            RenderDirLightShadows();
        }
    }

    private void SendDataToShader()
    {
        m_commandBuffer.SetGlobalInt(s_dirLightShadowCountId, m_dirLightShadowCount);
        m_commandBuffer.SetGlobalVectorArray(s_dirLightShadowDataPackedId, s_dirLightShadowDataPacked);
        m_commandBuffer.SetGlobalMatrixArray(s_worldToDirLightShadowMatrixId, s_worldToDirLightShadowMatrix);
        m_commandBuffer.SetGlobalVectorArray(s_cascadeSphereId, s_cascadeSphere);
    }

    private void RenderDirLightShadows()
    {
        // create RT
        int textureSize = (int)m_shadowSettings.dirLightShadow.textureSize; // one axis
        m_commandBuffer.GetTemporaryRT(s_dirLightShadowMapId, textureSize, textureSize, 32, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        m_commandBuffer.SetRenderTarget(s_dirLightShadowMapId, RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store);
        m_commandBuffer.ClearRenderTarget(true, false, Color.white);
        ExcuteBuffer();

        // start render shadow map 
        m_commandBuffer.BeginSample(c_commandBufferName);
        ExcuteBuffer();
        // divide shadow RT to blocks, total lightCount * cascadeCount blocks
        const int blockCount = c_maxDirLightShadowCount * c_maxDirLightShadowCascadeCount;
        int blockSize = m_dirLightShadowCount <= 1
            ? c_maxDirLightShadowCascadeCount
            : Mathf.CeilToInt(Mathf.Sqrt((float)blockCount)); // one axis
        int blockLength = textureSize / blockSize;
        for (int i = 0; i < m_dirLightShadowCount; i++)
        {
            for (int j = 0; j < c_maxDirLightShadowCascadeCount; j++)
            {
                RenderDirLightShadow(i, j, blockSize, blockLength);
                ConvertToAtlasMatrix(blockSize, i, j);
            }
        }

        SendDataToShader();
        m_commandBuffer.EndSample(c_commandBufferName);
        ExcuteBuffer();
    }

    private Vector3 GetCascadeRation()
    {
        return new Vector3(0.3f, 0.4f, 0.5f);
    }

    private void RenderDirLightShadow(int lightIndex, int cascadeIndex, int blockSize, int blockLength)
    {
        var light = m_shadowDirLights[lightIndex];
        s_dirLightShadowDataPacked[lightIndex].x = light.shadowStrength;
        var shadowSettings =
            new ShadowDrawingSettings(m_cullingRes, light.index, BatchCullingProjectionType.Orthographic);
        m_cullingRes.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.index, cascadeIndex,
            c_maxDirLightShadowCascadeCount, GetCascadeRation(), blockLength,
            0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData);
        shadowSettings.splitData = shadowSplitData;
        // all light use the same cascade
        if (lightIndex == 0)
        {
            s_cascadeSphere[cascadeIndex] = shadowSplitData.cullingSphere;
            // radius square, for distance compare in shader
            s_cascadeSphere[cascadeIndex].w *= s_cascadeSphere[cascadeIndex].w;
        }

        // calculate the block index of atlas
        int blockIndex = lightIndex * c_maxDirLightShadowCascadeCount + cascadeIndex;
        var viewPort = new Rect(new Vector2(blockIndex % blockSize, blockIndex / blockSize) * blockLength,
            new Vector2(blockLength, blockLength));
        m_commandBuffer.SetViewport(viewPort);
        m_commandBuffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        s_worldToDirLightShadowMatrix[blockIndex] = projMatrix * viewMatrix;
        ExcuteBuffer();
        m_context.DrawShadows(ref shadowSettings);
    }

    private void ConvertToAtlasMatrix(int blockSize, int lightIndex, int cascadeIndex)
    {
        int blockIndex = lightIndex * c_maxDirLightShadowCascadeCount + cascadeIndex;
        // convert uv from -1 - 1 to 0 - 1
        s_worldToDirLightShadowMatrix[blockIndex] = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0)) *
                                                    Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 1.0f)) *
                                                    s_worldToDirLightShadowMatrix[blockIndex];
        // convert z if reversed z
        if (SystemInfo.usesReversedZBuffer)
        {
            s_worldToDirLightShadowMatrix[blockIndex] =
                Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * s_worldToDirLightShadowMatrix[blockIndex];
        }

        // convert uv from 0 - 1 to block uv, when blockSize == 2
        float scaleSize = 1.0f / (float)blockSize;
        int xOffset = blockIndex % blockSize;
        int yOffset = blockIndex / blockSize;

        s_worldToDirLightShadowMatrix[blockIndex] =
            Matrix4x4.Translate(new Vector3(xOffset * scaleSize, yOffset * scaleSize, 0)) *
            Matrix4x4.Scale(new Vector3(scaleSize, scaleSize, 1)) *
            s_worldToDirLightShadowMatrix[blockIndex];
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