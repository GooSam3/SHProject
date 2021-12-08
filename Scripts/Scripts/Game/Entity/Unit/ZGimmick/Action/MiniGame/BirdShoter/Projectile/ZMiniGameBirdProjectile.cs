using DG.Tweening;
using UnityEngine;
using System;

/// <summary> 새 잡기 미니게임 새 </summary>
public class ZMiniGameBirdProjectile : MonoBehaviour
{
    private Action<ZMiniGameBird> OnHit;
    private Vector3 Dir;
    private Vector3 StartPosition;
    private float MoveSpeed = 5;
    private void FireProjectile(Vector3 dir, float moveSpeed, Action<ZMiniGameBird> onHit)
    {
        OnHit = onHit;
        MoveSpeed = moveSpeed;
        Dir = dir;
        StartPosition = transform.position;
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    private void OnDestroy()
    {
        if(ZMonoManager.hasInstance)
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    /// <summary> 총알 히트 처리 </summary>
    private void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;

        if (go.layer != UnityConstants.Layers.MiniGameObject)
            return;

        var comp = go.GetComponent<ZMiniGameBird>();

        if (null == comp)
            return;

        if (false == comp.Hit())
            return;

        OnHit?.Invoke(comp);
    }

    private void HandleLateUpdate()
    {
        float moveDistance = (StartPosition - transform.position).magnitude;

        transform.position += (Dir * Time.smoothDeltaTime * MoveSpeed); // + Time.smoothDeltaTime * Vector3.down * 9.8f);

        if (moveDistance > 50)
            GameObject.Destroy(gameObject);
    }

    /// <summary> 발사 </summary>
    public static void Fire(GameObject prefab, Vector3 startPos, Vector3 dir, float speed, Action<ZMiniGameBird> onHit)
    {
        GameObject go = GameObject.Instantiate(prefab, startPos, Quaternion.LookRotation(dir));

        ZMiniGameBirdProjectile projectile = go.GetOrAddComponent<ZMiniGameBirdProjectile>();

        projectile.FireProjectile(dir, speed, onHit);
    }
}
