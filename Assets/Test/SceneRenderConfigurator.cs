using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Debug = SAS.Debug;

[DefaultExecutionOrder(-100)] // Ensure this runs early
public class SceneRenderConfigurator : MonoBehaviour
{
    private const string Tag = "SceneRenderConfigurator";
    [SerializeField] private ScriptableRendererData m_TargetRendererData;

    [Tooltip("Optional: Specify a camera. If null, will use Camera.main")] [SerializeField]
    private Camera m_TargetCamera;

    void Start()
    {
        if (m_TargetRendererData == null)
        {
            Debug.LogWarning("SceneRenderConfigurator: No RendererData assigned.", this, Tag);
            return;
        }

        Camera cam = m_TargetCamera != null ? m_TargetCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogError("SceneRenderConfigurator: No camera found.", this, Tag);
            return;
        }

        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("SceneRenderConfigurator: URP asset not found.");
            return;
        }

        int rendererIndex = FindRendererIndex(urpAsset, m_TargetRendererData);
        if (rendererIndex < 0)
        {
            Debug.LogError("SceneRenderConfigurator: The assigned RendererData is not part of the URP asset.", this, Tag);
            return;
        }

        var urpCameraData = cam.GetUniversalAdditionalCameraData();
        urpCameraData.SetRenderer(rendererIndex);
        Debug.Log($"SceneRenderConfigurator: Renderer set to index {rendererIndex} for camera {cam.name}", this, Tag);
    }

    private int FindRendererIndex(UniversalRenderPipelineAsset asset, ScriptableRendererData target)
    {
        var rendererDataList = asset.rendererDataList;

        for (int i = 0; i < rendererDataList.Length; i++)
        {
            if (rendererDataList[i] == target)
                return i;
        }

        return -1;
    }
}