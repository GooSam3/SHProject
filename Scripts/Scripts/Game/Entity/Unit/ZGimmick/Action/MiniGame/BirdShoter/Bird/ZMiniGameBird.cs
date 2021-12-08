using DG.Tweening;
using Dreamteck.Splines;
using System.Collections;
using UnityEngine;

/// <summary> 새 잡기 미니게임 새 </summary>
public class ZMiniGameBird : ZMiniGameObject
{
    [Header("히트 이펙트")]
    [SerializeField]
    private GameObject Fx_Hit;

    private SplineComputer mSplineComputer;

    private SplineFollower mSplineFollower;

    private Animator Anim;

    private AnimatorOverrideController Controller;

    private float CurrentSpeed;

    private bool IsDead = false;

    public void Run(float minSpeed, float maxSpeed, Vector3[] points)
    {
        CurrentSpeed = Random.Range(minSpeed, maxSpeed);

        mSplineComputer = gameObject.GetOrAddComponent<SplineComputer>();
        mSplineFollower = gameObject.GetOrAddComponent<SplineFollower>();

        var splinePoints = new SplinePoint[points.Length];

        for(int i = 0; i < points.Length; ++i)
        {
            splinePoints[i] = new SplinePoint(points[i]);
        }

        mSplineComputer.SetPoints(splinePoints, SplineComputer.Space.World);

        mSplineFollower.startPosition = 0;
        mSplineFollower.followSpeed = CurrentSpeed;

        mSplineFollower.onEndReached += OnFinish;

        SetAnim();
    }

    private void OnDestroy()
    {
        mSplineFollower.onEndReached -= OnFinish;
    }

    private void SetAnim()
    {
        Anim = gameObject.GetComponentInChildren<Animator>();

        if (null == Anim)
        {
            return;
        }

        Controller = Anim.runtimeAnimatorController as AnimatorOverrideController;// new AnimatorOverrideController(Anim.runtimeAnimatorController);

        Anim.enabled = true;

        EntityAnimatorParameter.SetParameter(Anim, E_AnimParameter.Start_001);
    }


    private void OnFinish(double value)
    {
        mSplineFollower.follow = false;

        StopAllCoroutines();
        //도망  
        StartCoroutine(Co_Move(transform.forward, CurrentSpeed));
    }

    public bool Hit()
    {
        if (true == IsDead)
            return false;

        IsDead = true;

        mSplineFollower.follow = false;
        
        ZTempleHelper.SetActiveFx(Fx_Hit, true);

        StopAllCoroutines();
        //사망 연출        
        EntityAnimatorParameter.SetParameter(Anim, E_AnimParameter.End_001);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        StartCoroutine(Co_Move(Vector3.down, 9.8f, 0.3f));

        return true;
    }

    private IEnumerator Co_Move(Vector3 dir, float speed, float delayTime = 0f)
    {
        float duration = 5f;        

        yield return new WaitForSeconds(delayTime);

        while (0 < duration)
        {
            yield return null;
            transform.position += dir * Time.smoothDeltaTime * speed;
            duration -= Time.smoothDeltaTime;
        }

        GameObject.Destroy(gameObject);
    }

}
