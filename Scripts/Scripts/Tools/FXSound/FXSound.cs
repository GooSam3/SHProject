#if UNITY_EDITOR

using ParadoxNotion;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


namespace Tools
{
	public class FXSoundEffectData
	{
		public uint EffectID;
		public uint EffectSoundID;
		public string EffectFile;
		public float EffectDelayTime;
		public float EffectDelayTime_SV;
		public bool IsChange;
		public bool IsSelected;
	}

	public class FXSoundData
	{
		public uint SoundID;
		public string SoundFile;
		public float SoundVolume;
		public bool IsSelected;
	}

	public class FXSound : MonoBehaviour
	{
		[SerializeField] private FXSoundItemAdpater ScrollAdapter;
		[SerializeField] private FXSoundItem listItemPf;
		[SerializeField] private Transform fxRoot;
		[SerializeField] private InputField input;
		[SerializeField] private Text inputText;
		[SerializeField] private InputField searchInput;
		[SerializeField] private InputField audioSearchInput;
		[SerializeField] private Text timeText;
		[SerializeField] private Text stopText;
		[SerializeField] private Text timeScaleText;
		[SerializeField] private ZScrollBar timeSlider;
		[SerializeField] private FXSoundAudioItemAdapter ScrollAudioAdapter;
		[SerializeField] private FXSoundAudioItem listAudioItemPf;
		[SerializeField] private ZText curSoundName;
		[SerializeField] private GameObject audioIcon;

		private string[] FX_PATH_ARRAY = {
				"Assets/_ZAssetBundle/Effect",
				"Assets/_ZAssetBundle/Effect/Drop",
				"Assets/_ZAssetBundle/Effect/Mon",
				"Assets/_ZAssetBundle/Effect/Trans"};

		private string[] SOUND_PATH_ARRAY = {
				"Assets/_ZAssetBundle/Sound/Effect",
				"Assets/_ZAssetBundle/Sound/UI",
				"Assets/_ZAssetBundle/Sound/BGM"};

		private List<FXSoundEffectData> effectList = new List<FXSoundEffectData>();
		private List<FXSoundData> soundList = new List<FXSoundData>();
		private GameObject loadFX = null;
		private Coroutine corSoundDelay;
		private Tool_EffectTable effectTable = null;
		private Tool_SoundTable soundTable = null;
		private uint curEffectId;
		private bool stopFlag;
		private float fxTotalDuration;
		private bool isLoopFX;
		private float timeScale = 1;
		private float timer = 0;
		private bool isTimerStart;
		private GameObject curAudioGo;
		private uint curSoundId;

		[MenuItem("ZGame/FXSoundTool")]
		static void Initialize()
		{
			EditorSceneManager.OpenScene("Assets/Editor/RuntimeTool/FXSoundTool.unity");
			EditorApplication.isPlaying = true;
		}

		private void Awake()
		{
			ScrollAdapter.Parameters.ItemPrefab = listItemPf.GetComponent<RectTransform>();
			var pf = ScrollAdapter.Parameters.ItemPrefab;
			pf.SetParent(listItemPf.transform.parent);
			pf.localScale = Vector2.one;
			pf.localPosition = Vector3.zero;
			pf.gameObject.SetActive(false);
			ScrollAdapter.Initialize(OnClickedItem);

			ScrollAudioAdapter.Parameters.ItemPrefab = listAudioItemPf.GetComponent<RectTransform>();
			pf = ScrollAudioAdapter.Parameters.ItemPrefab;
			pf.SetParent(listItemPf.transform.parent);
			pf.localScale = Vector2.one;
			pf.localPosition = Vector3.zero;
			pf.gameObject.SetActive(false);
			ScrollAudioAdapter.Initialize(OnClickedAudioItem);
		}

		void Start()
		{
			LoadTables();
			CameraManager.Instance.DoSetTarget(fxRoot.transform);
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Free, Cinemachine.CinemachineBlendDefinition.Style.Cut, 0, true);
		}

		private void SetTimeScale(float _timeScale)
		{
			timeScaleText.text = string.Format("{0:f1}", _timeScale);
			Time.timeScale = _timeScale;
		}

		private void ResetTimer()
		{
			isTimerStart = false;
			timer = 0.0f;
		}

		private void ReStartTimer()
		{
			isTimerStart = true;
			timer = 0.0f;
		}

		private void StopTimer()
		{
			isTimerStart = false;
		}

		private void StartTimer()
		{
			isTimerStart = true;
		}

		void Update()
		{
			if (isTimerStart) {
				timer += Time.deltaTime;
			}

			audioIcon.SetActive(curAudioGo != null);

			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				MoveFxScrollToPrev();
			}

			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				MoveFxScrollToNext();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow)) {
				timeScale += 0.1f;
				if (timeScale > 1.0f) {
					timeScale = 1.0f;
				}
				SetTimeScale(timeScale);
			}

			if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				timeScale -= 0.1f;

				if (timeScale < 0.1f) {
					timeScale = 0.1f;
				}
				SetTimeScale(timeScale);
			}

			if ( searchInput.isFocused == false && audioSearchInput.isFocused == false ) {
				if (Input.GetKeyDown(KeyCode.S)) {
					OnClickStop();
				}

				if (Input.GetKeyDown(KeyCode.R)) {
					OnReplay();
				}

				if (Input.GetKeyDown(KeyCode.Q)) {
					OnClickApplyTime();
				}

				if (Input.GetKeyDown(KeyCode.E)) {
					OnClickApplySound();
				}

			}

			if (loadFX != null) {
				if (timer >= fxTotalDuration) {
					timeText.text = string.Format("{0:f2}/{1:f2}", fxTotalDuration, fxTotalDuration);

					if (isLoopFX) {
						ReStartTimer();
					}
					else {
						ResetTimer();
						Resume();

						Destroy(loadFX);
					}
				}
				else {
					timeText.text = string.Format("{0:f2}/{1:f2}", timer, fxTotalDuration);
				}

				timeSlider.size = timer / fxTotalDuration;
			}
			else {
				StopTimer();
				timeText.text = string.Format("{0:f2}/{1:f2}", 0, 0);

				timeSlider.size = 0;
			}
		}

		private void LoadTables()
		{
			effectTable = (Tool_EffectTable)ToolUtils.LoadTable(
				new Tool_EffectTable(ToolUtils.GetExcelPath("Effect_table")));

			soundTable = (Tool_SoundTable)ToolUtils.LoadTable(
				new Tool_SoundTable(ToolUtils.GetExcelPath("Sound_table")));

			effectList.Clear();
			foreach (var table in effectTable.DicTable.Values) {
				var data = new FXSoundEffectData();
				data.EffectID = table.EffectID;
				data.EffectSoundID = table.EffectSoundID;
				data.EffectFile = table.EffectFile;
				data.EffectDelayTime = table.EffectDelayTime;
				data.EffectDelayTime_SV = 0;
				effectList.Add(data);
			}

			RefreshFxScroll(true, 0);

			soundList.Clear();
			foreach (var table in soundTable.DicTable.Values) {
				var data = new FXSoundData();
				data.SoundID = table.SoundID;
				data.SoundFile = table.SoundFile;

				if (table.SoundFile.Contains("bgm_")) {
					continue;
				}

				data.SoundVolume = table.SoundVolume;
				soundList.Add(data);
			}

			RefreshAudioScroll(true, 0);
		}

		/// <summary> 이펙트 로드 성공시 사운드 재생 </summary>
		private void LoadFxAndSound(FXSoundEffectData fxData)
		{
			if (loadFX != null) {
				Destroy(loadFX);
			}

			for (int i = 0; i < FX_PATH_ARRAY.Length; ++i) {
				loadFX = LoadFX(FX_PATH_ARRAY[i], fxData.EffectFile);
				if (loadFX != null) {

					fxTotalDuration = 0;
					isLoopFX = false;

					var fxList = loadFX.GetComponentsInChildren<ParticleSystem>();
					for (int j = 0; j < fxList.Length; ++j) {
						var ps = fxList[j];
						var dur = ps.main.duration;
						var dly = Mathf.Max(ps.main.startDelay.constant, ps.main.startDelay.constantMax);

						if (fxTotalDuration < dur + dly) {
							fxTotalDuration = dur + dly;
						}

						if (isLoopFX == false) {
							if (ps.main.loop) {
								isLoopFX = true;
							}
						}
					}

					ReStartTimer();

					//이펙트 로드성공시 사운드 플레이
					if (corSoundDelay != null) {
						CoroutineManager.Instance.StopAction(corSoundDelay);
					}

					if (fxData.IsChange || fxData.EffectDelayTime_SV > 0) {
						corSoundDelay = CoroutineManager.Instance.StartTimer(fxData.EffectDelayTime_SV, () => {
							LoadAndPlaySound(fxData.EffectSoundID);
						});
					}
					else if (fxData.EffectDelayTime > 0) {
						corSoundDelay = CoroutineManager.Instance.StartTimer(fxData.EffectDelayTime, () => {
							LoadAndPlaySound(fxData.EffectSoundID);
						});
					}
					else {
						LoadAndPlaySound(fxData.EffectSoundID);
					}

					var soundData = soundList.Find(v => v.SoundID == fxData.EffectSoundID);
					if (soundData != null) {
						SetSoundName(soundData);
					}
					else {
						SetSoundName(null);
					}

					return;
				}
			}

			SetSoundName(null);
			ZLog.LogError(ZLogChannel.Default, $"이펙트 객체를 찾을수 없다, {fxData.EffectFile}");
		}

		private void SetSoundName( FXSoundData data )
		{
			if(data == null || data.SoundID==0) {
				curSoundName.text = "사운드 없음";
			}
			else {
				string name = $"<color=#ffff00>({data.SoundID})</color>";
				curSoundName.text = $"{data.SoundFile} {name}";
			}
		}

		private GameObject LoadFX(string path, string fxName)
		{
			string fullPath = string.Format("{0}/{1}.prefab", path, fxName);
			var asset = AssetDatabase.LoadMainAssetAtPath(fullPath) as GameObject;
			if (asset == null) {
				return null;
			}

			return Instantiate(asset, Vector3.zero, Quaternion.identity, fxRoot);
		}

		/// <summary> 사운드 로드 후 플레이 </summary>
		private GameObject LoadAndPlaySound(uint soundId)
		{
			var sondData = soundList.Find(v => v.SoundID == soundId);
			if (sondData == null) {
				ZLog.Log(ZLogChannel.Default, $"사운드가 없다, soundId:{soundId}");
				return null;
			}

			for (int i = 0; i < SOUND_PATH_ARRAY.Length; ++i) {
				var loadClip = LoadAudio(SOUND_PATH_ARRAY[i], sondData.SoundFile);
				if (loadClip != null) {
					return PlayAudio(loadClip, sondData.SoundVolume);
				}
			}

			ZLog.LogError(ZLogChannel.Default, $"사운드 객체를 찾을수 없다, {sondData.SoundFile}");
			return null;
		}

		private AudioClip LoadAudio(string path, string fxName)
		{
			string fullPath = string.Format("{0}/{1}.wav", path, fxName);
			return AssetDatabase.LoadMainAssetAtPath(fullPath) as AudioClip;
		}

		private GameObject PlayAudio(AudioClip clip, float volume)
		{
			if (clip == null) {
				ZLog.LogError(ZLogChannel.Default, $"AudioClip이 null이다, {clip}");
				return null;
			}

			GameObject audioGo = new GameObject(clip.name + "(sfx)");
			audioGo.transform.parent = fxRoot;

			AudioSource source = audioGo.AddComponent<AudioSource>();
			source.clip = clip;
			source.volume = volume;
			source.Play();
			Destroy(audioGo, clip.length);
			return audioGo;
		}

		/// <summary> 스크롤 제어 </summary>
		private void MoveFxScrollToNext()
		{
			var str = searchInput.text.ToLower();
			var findAll = effectList.FindAll(v => v.EffectFile.ToLower().Contains(str));
			var list = (findAll.Count > 0) ? findAll : effectList;

			int index = list.FindIndex(v => v.EffectID == curEffectId);
			index++;

			if (index >= list.Count) {
				return;
			}

			ScrollAdapter.ScrollTo(index);
			OnClickedItem(list[index].EffectID);
		}

		private void MoveFxScrollToPrev()
		{
			var str = searchInput.text.ToLower();
			var findAll = effectList.FindAll(v => v.EffectFile.ToLower().Contains(str));
			var list = (findAll.Count > 0) ? findAll : effectList;

			int index = list.FindIndex(v => v.EffectID == curEffectId);
			index--;

			if (index < 0) {
				return;
			}

			ScrollAdapter.ScrollTo(index);
			OnClickedItem(list[index].EffectID);
		}

		private void RefreshFxScroll(bool resetPos, uint focusTid)
		{
			if (resetPos) {
				ScrollAdapter.SetNormalizedPosition(1);
			}

			for (int i = 0; i < effectList.Count; ++i) {
				effectList[i].IsSelected = effectList[i].EffectID == focusTid;
			}

			var str = searchInput.text.ToLower();
			var findAll = effectList.FindAll(v => v.EffectFile.ToLower().Contains(str));
			if (findAll.Count > 0) {
				ScrollAdapter.Refresh(findAll);
			}
			else {
				ScrollAdapter.Refresh(effectList);
			}
		}

		private void RefreshAudioScroll(bool resetPos, uint focusTid)
		{
			if (resetPos) {
				ScrollAudioAdapter.SetNormalizedPosition(1);
			}

			for (int i = 0; i < soundList.Count; ++i) {
				soundList[i].IsSelected = soundList[i].SoundID == focusTid;
			}

			var str = audioSearchInput.text.ToLower();
			var findAll = soundList.FindAll(v => v.SoundFile.ToLower().Contains(str));
			if (findAll.Count > 0) {
				ScrollAudioAdapter.Refresh(findAll);
			}
			else {
				ScrollAudioAdapter.Refresh(soundList);
			}
		}

		private void ShowInputText(FXSoundEffectData data)
		{
			if (data.IsChange || data.EffectDelayTime_SV > 0) {
				input.text = $"{ data.EffectDelayTime_SV}";
				inputText.color = ColorUtils.HexToColor("F14848");
			}
			else {
				input.text = $"{ data.EffectDelayTime}";
				inputText.color = Color.yellow;
			}
		}

		/// <summary> 입력된 사운드딜레이시간을 적용 </summary>
		private void ApplyDelayTime(string str)
		{
			if (float.TryParse(str, out var num)) {
				var effect = effectList.Find(v => v.EffectID == curEffectId);
				if (effect == null) {
					ZLog.LogError(ZLogChannel.Default, $"적용중에 리스트에서 이펙트ID 찾을수 없다, {effect.EffectFile}");
					return;
				}

				if (effect.EffectSoundID == 0) {
					EditorUtility.DisplayDialog("경고", "사운드가 없는 이펙트입니다.\nEffectDelayTime값을 설정할 수 없습니다.", "확인");
					return;
				}

				if (effect.EffectDelayTime == num) {
					ZLog.Log(ZLogChannel.Default, $"입력한 숫자가 기존값과 동일합니다, {num}");
					return;
				}

				effect.EffectDelayTime_SV = num;
				effect.IsChange = true;

				RefreshFxScroll(false, curEffectId);
			}
			else {
				ZLog.LogError(ZLogChannel.Default, $"입력한 숫자 입력중 잘못되었습니다, {str}");
			}
		}

		private void Resume()
		{
			SetTimeScale(timeScale);

			stopFlag = false;
			stopText.text = "일시정지(S)";
		}

		// 버튼이벤트 ///////////////////////////////////////////////////////////////

		/// <summary> 이펙트 리스트 클릭 </summary>
		public void OnClickedItem(uint effectId)
		{
			Resume();

			curEffectId = effectId;

			RefreshFxScroll(false, effectId);

			// 인풋텍스트에 표시
			var data = effectList.Find(v => v.EffectID == effectId);
			if (data == null) {
				ZLog.LogError(ZLogChannel.Default, $"리스트에서 이펙트ID 찾을수 없다, {effectId}");
				return;
			}

			ShowInputText(data);

			LoadFxAndSound(data);
		}

		/// <summary> 사운드 리스트 클릭 </summary>
		public void OnClickedAudioItem(uint soundId)
		{
			Resume();

			curSoundId = soundId;

			RefreshAudioScroll(false, soundId);

			// 인풋텍스트에 표시
			var data = soundList.Find(v => v.SoundID == soundId);
			if (data == null) {
				ZLog.LogError(ZLogChannel.Default, $"리스트에서 사운드ID 찾을수 없다, {soundId}");
				return;
			}

			//ShowInputText(data);
			//LoadFxAndSound(data);

			if (curAudioGo != null) {
				Destroy(curAudioGo);
			}

			curAudioGo = LoadAndPlaySound(data.SoundID);
		}

		/// <summary> 입력한 사운드딜레이값 적용 </summary>
		public void OnClickApply()
		{
			Resume();

			if (curEffectId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 FX가 없습니다", "확인");
				return;
			}

			ApplyDelayTime(input.text);
		}

		/// <summary> fx 다시 재생 </summary>
		public void OnReplay()
		{
			Resume();

			if (curEffectId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 FX가 없습니다", "확인");
				return;
			}

			var effect = effectList.Find(v => v.EffectID == curEffectId);
			if (effect == null) {
				ZLog.LogError(ZLogChannel.Default, $"적용중에 리스트에서 이펙트ID 찾을수 없다, {curEffectId}");
				return;
			}

			LoadFxAndSound(effect);
		}

		/// <summary> 엑셀에 저장 </summary>
		public void OnClickSave()
		{
			Resume();

			if (curEffectId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 FX가 없습니다", "확인");
				return;
			}

			if (effectTable != null) {
				effectTable.Save(effectList);

				EditorUtility.DisplayDialog("알림", "저장했습니다.", "확인");
			}
		}

		/// <summary> 검색필드 이벤트 발동시마다 호출 </summary>
		public void OnChangeSearchField()
		{
			RefreshFxScroll(true, curEffectId);
		}

		public void OnChangeAudioSearchField()
		{
			RefreshAudioScroll(true, curSoundId);
		}

		/// <summary> fx 일시멈춤 and 재생 </summary>
		public void OnClickStop()
		{
			if (curEffectId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 FX가 없습니다", "확인");
				return;
			}

			if (loadFX == null) {
				return;
			}

			stopFlag = !stopFlag;

			if (stopFlag) {
				SetTimeScale(0);
				StopTimer();

				stopText.text = "시작(S)";
			}
			else {
				Resume();
				StartTimer();
			}
		}

		/// <summary> 멈춘 현재 딜레이시간을 적용  </summary>
		public void OnClickApplyTime()
		{
			if (curEffectId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 FX가 없습니다", "확인");
				return;
			}

			var timerStr = string.Format("{0:f2}", timer);
			ApplyDelayTime(timerStr);

			// 인풋텍스트에 표시
			var data = effectList.Find(v => v.EffectID == curEffectId);
			if (data == null) {
				ZLog.LogError(ZLogChannel.Default, $"리스트에서 이펙트ID 찾을수 없다, {curEffectId}");
				return;
			}

			ShowInputText(data);
		}

		/// <summary> 이펙트에 사운드 적용  </summary>
		public void OnClickApplySound()
		{
			if (curEffectId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 FX가 없습니다", "확인");
				return;
			}

			var data = effectList.Find(v => v.EffectID == curEffectId);
			if (data == null) {
				ZLog.LogError(ZLogChannel.Default, $"리스트에서 이펙트ID 찾을수 없다, {curEffectId}");
				return;
			}

			if (curSoundId == 0) {
				EditorUtility.DisplayDialog("경고", "선택한 사운드가 없습니다", "확인");
				return;
			}

			var soundData = soundList.Find(v => v.SoundID == curSoundId);
			if (soundData == null) {
				ZLog.LogError(ZLogChannel.Default, $"리스트에서 사운드ID 찾을수 없다, {curSoundId}");
				return;
			}

			data.EffectSoundID = soundData.SoundID;
			data.IsChange = true;

			RefreshFxScroll(false, curEffectId);

			SetSoundName(soundData);
		}

	} //class
}

#endif