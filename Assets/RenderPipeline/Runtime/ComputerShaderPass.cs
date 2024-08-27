using UnityEngine;
using UnityEngine.Rendering;

public class ComputerShaderPass
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

    public void Render(RenderTargetIdentifier src)
    {
        // m_camera.;
        // m_commandBuffer.GetTemporaryRT();

        var output = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        output.filterMode = FilterMode.Point;
        output.enableRandomWrite = true;
        output.Create();

        // m_commandBuffer.Get
        // ComputeBuffer

        m_commandBuffer.SetComputeTextureParam(computeShader, KernelIndex, "rt1", src);
        m_commandBuffer.SetComputeTextureParam(computeShader, KernelIndex, "rt2", output);

        computeShader.SetTexture(KernelIndex, "rt2", output);

        uint numThreadsX, numThreadsY, numThreadsZ;
        computeShader.GetKernelThreadGroupSizes(KernelIndex, out numThreadsX, out numThreadsY, out numThreadsZ);
        // computeShader.Dispatch(KernelIndex, (int)(output.width / numThreadsX),
        // (int)(output.height / numThreadsY), 1);
        m_commandBuffer.DispatchCompute(computeShader, KernelIndex, (int)(output.width / numThreadsX),
            (int)(output.height / numThreadsY), 1);
        m_context.ExecuteCommandBuffer(m_commandBuffer);
        m_commandBuffer.Clear();

        // m_commandBuffer.WaitAllAsyncReadbackRequests();
        // m_camera.targetTexture = output;
        // RenderTexture.ReleaseTemporary(output);
        return;
    }
}