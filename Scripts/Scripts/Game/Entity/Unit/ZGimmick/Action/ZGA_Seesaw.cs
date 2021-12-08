using UnityEngine;

/// <summary>
/// </summary>
public class ZGA_Seesaw : ZGimmickActionBase
{
    [SerializeField]
    private TriggerArea TriggerAreaForMyPc;

    [SerializeField]
    private Rigidbody mRBody;

    protected override void InitializeImpl()
    {
        mRBody = GetComponent<Rigidbody>();
        if (null != mRBody)
            mRBody.isKinematic = true;
    }

    protected override void InvokeImpl()
    {
        if (null == mRBody)
            return;

        mRBody.isKinematic = false;
    }

    protected override void CancelImpl()
    {
        mRBody.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (null == mRBody || true == mRBody.isKinematic || null == TriggerAreaForMyPc)
            return;

        var list = TriggerAreaForMyPc.GetEnteredRigidbody();

        foreach (var target in list)
        {
            var myPc = target.GetComponent<ZPawnMyPc>();

            if (null == myPc)
                continue;

            EntityComponentMovement_Temple movement = myPc.GetMovement<EntityComponentMovement_Temple>();
            if (movement.CurrentState is TempleCharacterControlState_Default defaultMovement)
            {
                if (defaultMovement.CurrentCharacterState != E_TempleCharacterState.Grounded)
                    continue;
                
                var velocity = mRBody.GetPointVelocity(myPc.Position);                

                if (velocity.magnitude <= 3f)
                {
                    float v = (9.8f * myPc.Weight/*target.mass*/);// * Time.fixedDeltaTime;

                    mRBody.AddForceAtPosition(Vector3.down * v, myPc.Position);
                    continue;
                }   
                
                defaultMovement.DoAddMomentum(velocity, 1f);
            }
        }
    }
}
