using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessController : CMonoBase
{
    /// <summary> 전역으로 사용할 포스트 프로세스 볼륨 </summary>
    [SerializeField]
    private Volume PPGlobalVolume = null;

    /// <summary> GlobalVolume에 profile을 셋팅한다. </summary>
    public void DoSetupGlobalPostProcessing(VolumeProfile profile)
    {
        bool invalid = false;
        if (null != PPGlobalVolume)
        {
            var volume = PPGlobalVolume;
            // 인자로 받은 새로운 프로파일 적용
            volume.sharedProfile = profile;

            if (null == volume.sharedProfile)
            {
                volume.enabled = false;
            }
            else
            {
                // Profile에서 모든 셋팅이 비활성화 상태라면 Layer도 동작안하도록 하자.
                // 그래야 CommandBuffer 생성안한다!!
                bool isAllOff = true;
                foreach (var setting in volume.sharedProfile.components)
                {
                    if (setting.active)
                    {
                        isAllOff = false;
                        break;
                    }
                }

                volume.enabled = !isAllOff;
            }

            if (false == volume.isGlobal)
            {
                ZLog.Log(ZLogChannel.PostProcess, ZLogLevel.Error, $"PostProcessVolume에 IsGlobal Setting은 True로 설정하세요!");

                volume.enabled = false;
            }

            if (UnityConstants.Layers.PostProcessing != volume.gameObject.layer)
            {
                ZLog.Log(ZLogChannel.PostProcess, ZLogLevel.Error, $"PostProcessVolume 객체의 Layer는 PostProcessing으로 설정하세요!");

                volume.enabled = false;
            }

            invalid = !volume.enabled;
        }
        else
        {
            invalid = true;
        }

        Turn(!invalid);

        ZLog.Log(ZLogChannel.PostProcess, $"SetupPostProcessing {!invalid}!");
    }

    /// <summary> PostProcess Layer 및 Volume을 활성화/비활성화 한다. </summary>
    public void Turn(bool enabled)
    {
        PPGlobalVolume.enabled = enabled;
    }
}
