using UnityEngine;
using UnityEngine.UI;

public class UISearchTargetSlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private GameObject Normal = null;
    [SerializeField] private GameObject On = null;
    [SerializeField] private Text TargetTxt = null;
    [SerializeField] private Text TargetNum = null;
    #endregion

    #region System Variable
    private ZPawn Target = null;
    #endregion

    public void DoAddEvent()
    {
        ZPawnManager.Instance.MyEntity?.DoAddEventChangeTarget(ChangeTarget);
    }

    public void DoRemoveEvent()
    {
        ZPawnManager.Instance.MyEntity?.DoRemoveEventChangeTarget(ChangeTarget);
    }

    public void Init(ZPawn target, int orderNum)
    {
        if (ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity)
        {
            // 현재 타겟된 entity가 있으면 검색된 타겟 리스트 중 해당 슬롯에 UI 표시
            if (null != myEntity.GetTarget() && myEntity.GetTarget().EntityId == target.EntityId)
            {
                Normal.SetActive(false);
                On.SetActive(true);
            }
            else
            {
                Normal.SetActive(true);
                On.SetActive(false);
            }
        }

        this.Target = target;

        TargetNum.text = (orderNum + 1).ToString();
        TargetTxt.text = target.EntityData.Name;
    }

    public void OnClick()
    {
        if (!(ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity))
            return;

        ZPawn target = myEntity.TargetSearchList.Find(a => a.EntityId == Target.EntityId);

        if(myEntity.IsAttacking && myEntity.GetTarget() == target)
        {
            return;
        }

        myEntity.SetTarget(null);
        myEntity.SetTarget(target);


        // l2m 기준으로 무조건 공격함.
        {
            myEntity.ChangeState(E_EntityState.Empty);
            myEntity.UseNormalAttack(true);
        }
        
        //if (myEntity.IsAttacking)
        //{
        //    myEntity.ChangeState(E_EntityState.Empty);
        //    myEntity.UseNormalAttack(true);
        //}
        //else
        //{
        //    myEntity.ChangeState(E_EntityState.Empty);
        //    if(myEntity.IsMoving())
        //        myEntity.StopMove();
        //}


        //// 기존에 선택한 타겟이 있고 현재 선택한 타겟이 같을 경우
        //if (myEntity.GetTarget() != null && target == myEntity.GetTarget())
        //{
        //    // 공격중이고 공격중인 타겟과 같다면 공격 중지
        //    if(myEntity.IsAttacking && myEntity.PresentAttackTargetId == target.EntityId)
        //    {
        //        myEntity.SetTarget(null);
        //        myEntity.ChangeState(E_EntityState.Empty);
        //    }
        //    // 이동하여 공격
        //    else
        //    {
        //        myEntity.ChangeState(E_EntityState.Empty);
        //        myEntity.UseNormalAttack();
        //    }
        //}
        //// 설정된 타겟이 다르거나 최초로 타겟슬롯을 누르면 해당슬롯의 타겟을 설정
        //else
        //{
        //    myEntity.SetTarget(target);
        //}
    }

    /// <summary> 타겟이 변경될 경우 해당 타겟에 대한 슬롯 UI 변경 </summary>
    private void ChangeTarget(uint prevTargetId, uint nextTargetId)
    {
        if (null == Target)
            return;

        if (nextTargetId == 0 || nextTargetId != Target.EntityId)
        {
            On.SetActive(false);
            Normal.SetActive(true);
        }
        else
        {
            On.SetActive(true);
            Normal.SetActive(false);
        }
    }

    public void ResetTarget()
    {
        Target = null;
    }
}
