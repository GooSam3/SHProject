using System.Collections;
using UnityEngine;



/// <summary> 기울기패널 / 그저 기울여서 '공' 을 굴리는 용도이므로 Ani 사용 안함 </summary>
public class ZGA_Balance : ZGimmickActionBase
{
    [Header("무게 감지 자식 저울 객체 1")]
    [SerializeField]
    private ZGA_BalanceHandle HandleFirst;

    [Header("무게 감지 자식 저울 객체 2")]
    [SerializeField]
    private ZGA_BalanceHandle HandleSecond;

    [Header("애니메이터")]
    [SerializeField]
    private Animator animator;

    ZDirty mUpdateDirty;

    // 무게가 변했는지 감지하여 AddForce 해주는 용도
    bool isChangeWeight = false;

    // 첫번쨰 저울 무게
    private float FirstWeight;

    // 두번째 저울 무게
    private float SecondWeight;

	protected override void InitializeImpl()
	{
        mUpdateDirty = new ZDirty(1f);
        mUpdateDirty.CurrentValue = 0.5f;

        HandleFirst.SetBalanceHandle(this, E_Balance.First);
        HandleSecond.SetBalanceHandle(this, E_Balance.Second);

        Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 0f);
        Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, mUpdateDirty.CurrentValue);

        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
    }

	protected override void InvokeImpl()
    {
        StartCoroutine(LateFixedUpdate());
    }

    protected override void CancelImpl()
    {
        StopAllCoroutines();
    }

	protected override void DestroyImpl()
	{
		base.DestroyImpl();
        StopCoroutine(LateFixedUpdate());
    }

	public void SetWeight(E_Balance eBalance, float mWeight)
	{
		switch (eBalance)
		{
			case E_Balance.First: FirstWeight = mWeight; break;
			case E_Balance.Second: SecondWeight = mWeight; break;
		}

		float aniNormal = 0;

        // 양팔의 무게가 같은경우 중앙
		if (FirstWeight == SecondWeight)
            aniNormal = 0.5f;
		else
		{
            // 왼쪽이 0 이면 애니메이션은 무조건 오른쪽으로
            if (0 == SecondWeight)
			{
                aniNormal = 1;
            }
            // 오른쪽이 0 이면 애니메이션은 왼쪽으로
            else if (0 == FirstWeight)
			{
                aniNormal = 0;
            }
            else
			{
                aniNormal = FirstWeight / SecondWeight * 0.5f;
            }
		}

        isChangeWeight = true;
        mUpdateDirty.GoalValue = aniNormal;
        mUpdateDirty.IsDirty = true;
    }

    
    private IEnumerator LateFixedUpdate()
    {
        WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
        while (true)
        {
            yield return _instruction;

            HandleFirst.CheckEnterGimmickVelocity();
            HandleSecond.CheckEnterGimmickVelocity();

            if (mUpdateDirty.IsDirty)
			{
                mUpdateDirty.Update();
                if(isChangeWeight)
				{
                    HandleFirst.AddForce();
                    HandleSecond.AddForce();
                    isChangeWeight = false;
                }
            }

            
            Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, mUpdateDirty.CurrentValue);
        }
    }
}