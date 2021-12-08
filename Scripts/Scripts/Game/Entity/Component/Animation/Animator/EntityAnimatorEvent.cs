using UnityEngine;

/// <summary> 애니메이션 클립에 셋팅된 event. 추후 필요하다면 사용하자 </summary>
public class EntityAnimatorEvent : MonoBehaviour
{
    private EntityBase Owner;
    public void Initialize(EntityBase owner)
    {
        Owner = owner;
    }

	[UnityEngine.Scripting.Preserve]
	private void Invoke()
    {
    }

    private void Effect()
    {
    }
}
