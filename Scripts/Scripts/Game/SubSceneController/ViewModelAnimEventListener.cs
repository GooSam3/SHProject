using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ViewModelAnimEventListener : MonoBehaviour
{
    private Animator anim;

    private int hash = 0;

    private string trigger = string.Empty;

    private ParticleSystem fxAttack;

    private bool reserveRelease = false;

    public Transform SocketHit { get; private set; }

    private Transform targetSocketTR = null;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        hash  = 0;
    }


    private void OnDisable()
    {
        CancelInvoke();
    }

    public void Initialize(bool useHitFx)
    {
        reserveRelease = false;

        var socketTrans = this.transform.FindTransform($"Socket_{E_ModelSocket.Hit}");

        if (null != socketTrans)
            SocketHit = socketTrans;


        if (useHitFx == false)
            return;

        var rnd = Random.Range(0, 3);

        uint fxKey = DBConfig.Pet_Expedition_Hit01;

        switch(rnd)
		{
            case 0:
                fxKey = DBConfig.Pet_Expedition_Hit01; 
                break;
            case 1:
                fxKey = DBConfig.Pet_Expedition_Hit02;
                break;
            case 2:
                fxKey = DBConfig.Pet_Expedition_Hit03; 
                break;
        }

        if(DBResource.TryGetEffect(fxKey, out var table)==false)
		{
            ZLog.Log(ZLogChannel.System, $"effectKEy 없음 !! key : {fxKey}");
            return;
		}
        
        Addressables.InstantiateAsync(table.EffectFile).Completed += (obj) =>
        {
            if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                OnLoadFx(obj.Result);
            }
        };
    }

    public void SetHitTarget(Transform tr)
	{
        targetSocketTR = tr;

        if (fxAttack == null)
            return;

        fxAttack.gameObject.SetActive(tr!=null);

        if (targetSocketTR == null)
        {
            return;
        }

        if (fxAttack == null)
            return;

        fxAttack.transform.SetParent(tr);
        fxAttack.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, Vector3.one);
	}

    private void OnLoadFx(GameObject obj)
	{
        if(reserveRelease)
		{
            Addressables.ReleaseInstance(obj);
            fxAttack = null;
            return;
		}

        fxAttack = obj.GetComponent<ParticleSystem>();

        var mainModule = fxAttack.main;
        mainModule.playOnAwake = false;

        fxAttack.Stop();

        SetHitTarget(targetSocketTR ?? SocketHit);
    }

    private void ResetTrigger()
    {
        CancelInvoke();

        if (hash > 0)
        {
            anim.ResetTrigger(trigger);
            fxAttack?.Stop();
        }

        hash = 0;
        trigger = string.Empty;

    }

    public void SetTrigger(string _trigger)
    {
        ResetTrigger();

        if (string.IsNullOrEmpty(_trigger))
        {//idle
            anim.Rebind();
            return;
        }
        trigger = _trigger;
        hash = Animator.StringToHash(trigger);

        try
        {
            var length = (anim.runtimeAnimatorController as AnimatorOverrideController)[_trigger].length;

            InvokeRepeating(nameof(TrigAnim), 0, length);
        }
        catch
        {
            ZLog.LogError(ZLogChannel.UI, $"해당 트리거 없음.. : {trigger}");
        }
        
    }

    private void TrigAnim()
    {
        if (hash == 0)
            return;

        anim.SetTrigger(hash);
    }

    public void Release()
	{
        reserveRelease = true;
        this.gameObject.SetActive(false);

        if(fxAttack!=null)
            Addressables.ReleaseInstance(fxAttack.gameObject);
    }

    [UnityEngine.Scripting.Preserve]
    private void Invoke()
    {
        fxAttack?.Play(true);
    }

    private void Effect()
    {
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
