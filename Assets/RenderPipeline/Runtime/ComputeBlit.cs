using UnityEngine;
using UnityEngine.Rendering;

public class ComputeBlit
{
    private const string c_commandBufferName = "Compute Shader";

    private CommandBuffer m_commandBuffer = new CommandBuffer()
    {
        name = c_commandBufferName,
    };

    private ScriptableRenderContext m_context;
    private Camera m_camera;

    public ComputeShader computeShader = Resources.Load<ComputeShader>("Shaders/Blit");
    private int KernelIndex => computeShader.FindKernel("CSmain");

    public void Setup(ScriptableRenderContext context, Camera camera)
    {
        m_context = context;
        m_camera = camera;
    }

    public void Render(RenderTargetIdentifier from)
    {
        var id = Shader.PropertyToID("rt2");
        var id3 = Shader.PropertyToID("rt3");

        m_commandBuffer.GetTemporaryRT(id, Screen.width, Screen.height, 24, FilterMode.Point,
            RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, true);
        // m_commandBuffer.SetT

        // m_commandBuffer.GetTemporaryRT();
        var output = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        output.filterMode = FilterMode.Point;
        output.enableRandomWrite = true;
        output.Create();

        // m_commandBuffer.Get
        // ComputeBuffer

        m_commandBuffer.SetComputeTextureParam(computeShader, KernelIndex, "rt1", from);
        m_commandBuffer.SetComputeTextureParam(computeShader, KernelIndex, "rt2", id3);
        // m_commandBuffer.SetCom

        computeShader.SetTexture(KernelIndex, "rt2", output);

        uint numThreadsX, numThreadsY, numThreadsZ;
        computeShader.GetKernelThreadGroupSizes(KernelIndex, out numThreadsX, out numThreadsY, out numThreadsZ);
        // computeShader.Dispatch(KernelIndex, (int)(output.width / numThreadsX),
        // (int)(output.height / numThreadsY), 1);
        m_commandBuffer.DispatchCompute(computeShader, KernelIndex, (int)(output.width / numThreadsX),
            (int)(output.height / numThreadsY), 1);
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();

        m_commandBuffer.ReleaseTemporaryRT(id);
        // m_commandBuffer.WaitAllAsyncReadbackRequests();
        // m_camera.targetTexture = output;
        return;
    }
}