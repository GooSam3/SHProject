using System.Collections;
using UnityEngine;

public class ZTempleMiniGame_LibraSwitch : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField]
    private Animator animator;

    [Header("천칭 미니게임")]
    [SerializeField]
    private ZTempleMiniGame_Libra Libra;

    // 애니메이션 시간
    private float Start_aniTime;
    private float End_aniTime;

    // 애니메이션 하는 중인지 여부
    private bool isPlayAni;

	private void Start()
	{
        isPlayAni = false;
        
        var controller = animator.runtimeAnimatorController as AnimatorOverrideController;
        Start_aniTime = controller["Start_001"].length;
        End_aniTime = controller["End_001"].length;
    }

    public void SwitchOn()
	{
        if (isPlayAni)
            return;

        StartCoroutine(nameof(AniPlay));
	}

    IEnumerator AniPlay()
	{
        isPlayAni = true;

        animator.SetTrigger("Start_001");

        yield return new WaitForSeconds(Start_aniTime);

        Libra.WeightCheckStart();
        animator.SetTrigger("End_001");

        yield return new WaitForSeconds(End_aniTime);
        isPlayAni = false;
    }
}
