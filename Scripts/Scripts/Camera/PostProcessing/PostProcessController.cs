using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessController : CMonoBase
{
    /// <summary> �������� ����� ����Ʈ ���μ��� ���� </summary>
    [SerializeField]
    private Volume PPGlobalVolume = null;

    /// <summary> GlobalVolume�� profile�� �����Ѵ�. </summary>
    public void DoSetupGlobalPostProcessing(VolumeProfile profile)
    {
        bool invalid = false;
        if (null != PPGlobalVolume)
        {
            var volume = PPGlobalVolume;
            // ���ڷ� ���� ���ο� �������� ����
            volume.sharedProfile = profile;

            if (null == volume.sharedProfile)
            {
                volume.enabled = false;
            }
            else
            {
                // Profile���� ��� ������ ��Ȱ��ȭ ���¶�� Layer�� ���۾��ϵ��� ����.
                // �׷��� CommandBuffer �������Ѵ�!!
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
                ZLog.Log(ZLogChannel.PostProcess, ZLogLevel.Error, $"PostProcessVolume�� IsGlobal Setting�� True�� �����ϼ���!");

                volume.enabled = false;
            }

            if (UnityConstants.Layers.PostProcessing != volume.gameObject.layer)
            {
                ZLog.Log(ZLogChannel.PostProcess, ZLogLevel.Error, $"PostProcessVolume ��ü�� Layer�� PostProcessing���� �����ϼ���!");

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

    /// <summary> PostProcess Layer �� Volume�� Ȱ��ȭ/��Ȱ��ȭ �Ѵ�. </summary>
    public void Turn(bool enabled)
    {
        PPGlobalVolume.enabled = enabled;
    }
}
