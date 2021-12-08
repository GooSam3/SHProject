using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Zero;

public class GameDBManager : Singleton<GameDBManager>
{
	public enum E_LoadStatus
	{
		NotLoaded,
		Loading,
		LoadingDone,
	}
	
	public class TableLoadProgress
	{
		public string Name = string.Empty;
		public int CurrentNo = 0;
		public int TotalCount = 0;

		public void Empty()
		{
			Name = string.Empty;
			CurrentNo = 0;
			TotalCount = 0;
		}
	}

	/// <summary>
	/// 실 테이블 데이터 저장소
	/// </summary>
	public static GameDBContainer Container
	{
		get; private set;
	}

	/// <summary> 파일 확장자명 </summary>
	private const string FileExtensionName = "Bin";

	/// <summary> 
	/// 초기화 및 실 데이터로드가 완료되었는지 여부
	/// </summary>
	public E_LoadStatus LoadStatus { get; private set; } = E_LoadStatus.NotLoaded;

	/// <summary> 테이블이 모두 로드됐을때 호출되는 이벤트 </summary>
	public Action AllLoaded;
	/// <summary> 테이블 로딩중 상태 알림 이벤트 </summary>
	public Action<TableLoadProgress> ProgressUpdated = null;

    private Dictionary<string, IGameDBHelper> mDataClasses = new Dictionary<string, IGameDBHelper>();
	private Coroutine mLoadRoutine = null;
	private TableLoadProgress mProgress = new TableLoadProgress();

	protected override void Init()
    {
        base.Init();

		LoadStatus = E_LoadStatus.NotLoaded;

		InitMessagePackOptions();
	}

	/// <summary>
	/// 예약된 MessagePack 전용 Resolver 셋팅
	/// </summary>
	private void InitMessagePackOptions()
	{
		/*
		 * Deserialize 성능 최적화를 위한 셋팅 
		 * 
		 * Bianry생성툴과 Pair가 맞아야함.
		 */
		MessagePack.Resolvers.ZDBCompositeResolver.Instance.Register(			
			MessagePack.Resolvers.GeneratedResolver.Instance,
			MessagePack.Resolvers.StandardResolver.Instance
		);

		var options = MessagePack.MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ZDBCompositeResolver.Instance);
		MessagePack.MessagePackSerializer.DefaultOptions = options;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_rootDirPath">테이블 파일들이 존재하는 폴더 경로</param>
	public void Load(string _rootDirPath, Action<string> _onError)
	{
		if (null != mLoadRoutine)
			StopCoroutine(mLoadRoutine);

		Unload();

		mLoadRoutine = StartCoroutine(LoadDB_Routine(_rootDirPath, _onError));
	}

	public void Unload()
	{
		Container = new GameDBContainer();

		LoadStatus = E_LoadStatus.NotLoaded;
		mProgress.Empty();
	}

	IEnumerator LoadDB_Routine(string _rootDirPath, System.Action<string> _onError)
    {
		LoadStatus = E_LoadStatus.Loading;

		FieldInfo[] allFields = typeof(GameDBContainer).GetFields();

        int totalCount = allFields.Length;
        int curTableNo = 0;

		var watch = new System.Diagnostics.Stopwatch();
		long elapsedMs = 0;

		foreach (var field in allFields)
        {
			if (!field.FieldType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
				continue;

			watch.Restart();

			Type tableType = field.FieldType.GetGenericArguments()[1];                
            string fileFullPath = $"{_rootDirPath}/{tableType.Name}.{FileExtensionName.ToLower()}";

			ZLog.BeginProfile(tableType.Name);
			{
				try
				{
					//==================================================
					// 파일 읽어오기
					// https://referencesource.microsoft.com/#mscorlib/system/io/file.cs,ee8033fcb7e7a677
					byte[] readBytes = File.ReadAllBytes(fileFullPath);

					//==================================================
					// 테이블에 맞게 Deserialize해서 Container에 할당
					object value = tableType.InvokeMember("Deserialize", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { readBytes });
					field.SetValue(Container, value);
				}
				catch (System.Exception exception)
				{
#if UNITY_EDITOR
					if (UnityEditor.EditorUtility.DisplayDialog(
						"테이블 로드 에러",
						$"예상1. 테이블관련 코드가 변경됐는데, SVN Update 받지 않고 실행했을 경우. (기획+클라+서버 확인필요)" +
						$"\n예상2. 테이블 자체 오류 발생 (기획  확인필요)", "확인"))
					{
						UnityEditor.EditorApplication.isPlaying = false;
					}
#endif

					Debug.LogError($"{field.Name} deserialize Failed!! Message : {exception}");

					_onError?.Invoke(tableType.Name);

					LoadStatus = E_LoadStatus.NotLoaded;
					yield break;
				}
			}
			ZLog.EndProfile(tableType.Name);

            curTableNo++;

			// 로딩정보 갱신 이벤트 호출
			mProgress.Name = field.Name;
			mProgress.CurrentNo = curTableNo;
			mProgress.TotalCount = totalCount;
			ProgressUpdated?.Invoke(mProgress);

			// 작은 파일읽는데는 현 프레임에서 해결하도록 한다.
			watch.Stop();
			elapsedMs += watch.ElapsedMilliseconds;
			if (elapsedMs > 50)
			{
				elapsedMs = 0;
				yield return null;
			}
        }

		ZLog.BeginProfile(nameof(IGameDBHelper.OnReadyData));
		{
			OnReadyDatas();
		}
		ZLog.EndProfile(nameof(IGameDBHelper.OnReadyData));

		LoadStatus = E_LoadStatus.LoadingDone;

		AllLoaded?.Invoke();
    }

	/// <summary>
	/// <see cref="IGameDBHelper"/>를 상속받은 클래스 인스턴스들 모두를 생성하도록 한다
	/// </summary>
    private void OnReadyDatas()
    {
        mDataClasses.Clear();

		// IGameDBData상속받은 모든 클래스 찾기
		List<Type> derivedTypes = ReflectionHelper.FindAllDerivedTypes<IGameDBHelper>();
        foreach (Type type in derivedTypes)
        {
            if (!type.IsClass || type.IsInterface || type.IsAbstract)
                continue;

            if (this.mDataClasses.ContainsKey(type.Name))
            {
                Debug.LogError($"{type}의 {nameof(IGameDBHelper)} Class는 이미 존재합니다.");
                continue;
            }

			if (Activator.CreateInstance(type) is IGameDBHelper dataIntance)
			{
				dataIntance.OnReadyData();
				mDataClasses.Add(type.Name, dataIntance);
			}
		}
    }
}