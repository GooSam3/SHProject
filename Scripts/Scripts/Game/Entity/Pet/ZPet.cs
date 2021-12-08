using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//(추후 서버 및 테이블 데이터 연결 작업 진행해야함)
/// <summary> 펫 </summary>
public class ZPet : ZPetBase
{
    protected override void OnPostInitializeImpl()
    {
        StartCoroutine(UpdateCoroutine());
    }

    protected override void OnLoadedModelImpl()
    {
		if (null != ModelComponent)
			transform.localScale = Vector3.one * ModelComponent.ModelScaleFactor;

        var position = mOwnerCharacter.Position - mOwnerCharacter.transform.forward * 2f;
        Warp(position);
        // 펫 소환 이펙트
        ZEffectManager.Instance.SpawnEffect(DBResource.Fx_Summon_Pet, position, transform.rotation, 0f, 1f, null);
    }

    IEnumerator UpdateCoroutine()
    {
        while (null != mOwnerCharacter)
        {
            Vector3 petPos = Position;
            Vector3 ownerPos = mOwnerCharacter.transform.position;
            float distance = (petPos - ownerPos).magnitude;

            if (distance > 5 && distance < 40)
            {
                Vector3 movePos = ownerPos + ((petPos - ownerPos).normalized * 2.0f);                
                MoveTo(new List<Vector3>() { movePos }, mOwnerCharacter.MoveSpeed);
            }
            else if (distance >= 40)
            {
                Warp(mOwnerCharacter.Position - mOwnerCharacter.transform.forward * 2f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        GameObject.Destroy(gameObject);
    }
}
