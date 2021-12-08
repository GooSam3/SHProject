using UnityEngine;

[RequireComponent(typeof(ZEffectComponent))]
public class ZEffectFollowTarget : MonoBehaviour
{
    private Transform mTarget = null;
    private Vector3 mOffset;

    public void SetTarget(Transform target, Vector3 offset)
    {
        if(null == target)
        {
            GameObject.DestroyImmediate(this);
            return;
        }

        mTarget = target;
        mOffset = offset;

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdatePosition);
    }

    private void OnDestroy()
    {
        if (false == ZMonoManager.hasInstance)
            return;

        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdatePosition);
    }

    private void UpdatePosition()
    {
        if(null == mTarget)
        {
            //타겟이 제거되면 이펙트도 제거한다.
            var comp = GetComponent<ZEffectComponent>();
            comp?.Despawn();
            return;
        }

        transform.position = mTarget.position + mOffset;
    }
}