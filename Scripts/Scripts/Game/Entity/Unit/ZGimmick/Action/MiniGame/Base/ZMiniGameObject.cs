using DG.Tweening;
using UnityEngine;

/// <summary> 새 잡기 미니게임 새 </summary>
public class ZMiniGameObject : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetLayersRecursively(UnityConstants.Layers.MiniGameObject);        
    }
}
