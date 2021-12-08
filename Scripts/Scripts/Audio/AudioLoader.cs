using ClockStone;
using GameDB;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AudioLoader : MonoBehaviour, IAudioLoader
{
    public void AddAudioFileLoadAsync(uint soundID, System.Action<AudioItem> loadCB)
    {
        DBSound.TryGet(soundID, out Sound_Table tableData);

        AudioItem audio = AudioController.GetAudioItem(tableData.SoundFile);

        if (null != audio)
        {
			audio.Volume = tableData.SoundVolume;
			audio.SoundType = tableData.SoundType;

			loadCB?.Invoke(audio);
        }
		else
		{
			/// <summary>
			/// 리소스 로더가 구현되면 로더를 사용하도록 변경할 예정
			/// 테스트를 위해 외부 별도 폴더를 사용하도록 임시 구현
			/// </summary>

			// TODO : 작업자는 개선바람.
			Addressables.LoadAssetAsync<AudioClip>(tableData.SoundFile).Completed += (objHandle) =>
			{
				if (objHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				{
					AudioItem newAudioItem = AudioManager.Instance.Helper.GetAudioItem(objHandle.Result, tableData.SoundType.ToString());
					newAudioItem.Volume = tableData.SoundVolume;
					newAudioItem.SoundType = tableData.SoundType;

					loadCB?.Invoke(newAudioItem);
				}
				else
				{
					Debug.LogError($"SoundTID[{tableData.SoundID}]에 해당하는 File[{tableData.SoundFile}]이 존재하지 않습니다.");

					loadCB?.Invoke(null);
				}
			};
		}
    }
}