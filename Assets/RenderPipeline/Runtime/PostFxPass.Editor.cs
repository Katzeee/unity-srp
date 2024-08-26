using UnityEngine;
using UnityEditor;

public partial class PostFxPass
{
#if UNITY_EDITOR
    private void ApplySceneViewState()
    {
        if (m_camera.cameraType == CameraType.SceneView &&
            !SceneView.currentDrawingSceneView.sceneViewState.showImageEffects)
        {
            m_postFxSettings = null;
        }
    }
#else
    private void ApplySceneViewState() { }
#endif
}