using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MileageShopAdvSceneRoot : MonoBehaviour
{
    public Transform SpawnRoot;

    [SerializeField]
    private Camera sceneCamera;

    [Header("ViewScale * 이 값")]
    public float scaleMultiplier = 0.1f;

    RenderTexture renderTexture;

    public RenderTexture SetRenderTexture(Vector2 size)
    {
        renderTexture = new RenderTexture((int)size.x, (int)size.y, 0);
        renderTexture.Create();
        sceneCamera.targetTexture = renderTexture;
        return renderTexture;
    }

    public void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture.DiscardContents();
            Destroy(renderTexture);
        }
    }
}
