using UnityEngine;
using UnityEngine.Rendering;

public class CCommonUtils
{
    public static void ClearFrameBuffer(CommandBuffer commandBuffer, CameraClearFlags flags, Color color)
    {
        if (flags == CameraClearFlags.Color || flags == CameraClearFlags.SolidColor)
        {
            commandBuffer.ClearRenderTarget(true, true, color);
        }
        else if (flags == CameraClearFlags.Depth)
        {
            commandBuffer.ClearRenderTarget(true, false, Color.black);
        }
        else if (flags == CameraClearFlags.Skybox)
        {
            commandBuffer.ClearRenderTarget(true, true, Color.black);
        }
    }
}