using UnityEngine;

namespace Zero
{
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		private static T mInstance = null;
		public static T Instance
		{
			get
			{
				if (mInstance == null)
				{
					mInstance = GameObject.FindObjectOfType(typeof(T)) as T;

					if (mInstance == null && canCreate)
					{
						mInstance = SingletonConfig.GetSingletonObject<T>();
						if (null == mInstance)
						{
							mInstance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
						}
					}
					else
						mInstance.Init();
				}
				return mInstance;
			}
		}

		/// <summary> 해당 인스턴스가 생성되어있는지 확인용 </summary>
		public static bool hasInstance => null != mInstance && canCreate;

		public int InstanceID;
		public new Transform transform { get; private set; }
		public new GameObject gameObject { get; private set; }
		/// <summary> Application이 닫히는 중이라면 사용안되도록 체크 하기 위함 </summary>
		static bool canCreate = true;
		
		public virtual void Awake()
		{
			Init();
		}

		protected virtual void Init()
		{
			if (mInstance == null)
			{
				mInstance = this as T;
				SetDefault();

			}
			else
			{
				if (mInstance != this)
					DestroyImmediate(base.gameObject);
			}
		}

		protected void SetDefault()
		{
			transform = base.transform;
			gameObject = base.gameObject;
			InstanceID = GetInstanceID();

			if (Application.isPlaying)
				DontDestroyOnLoad(base.gameObject);
		}

		protected virtual void OnApplicationQuit()
		{
			canCreate = false;
		}

		protected virtual void OnDestroy()
		{
			mInstance = null;
		}

		/// <summary>
		/// https://docs.unity3d.com/kr/2019.4/Manual/DomainReloading.html 대응함수
		/// Reload Domain을 off했다면 static값들 초기화 임의로 해주어야함.
		/// </summary>
		public static void ClearStatics()
		{
			mInstance = null;
			canCreate = true;
		}
	}

	static class SingletonConfig
	{
		public const string ResourcePath = "Defaults";

		/// <summary>
		/// 미리 설정된 상태 프리팹 객체가 존재한다면 가져오도록 한다.
		/// </summary>
		public static T GetSingletonObject<T>() where T : MonoBehaviour
		{
			var loadObj = Resources.Load($"{SingletonConfig.ResourcePath}/{typeof(T).Name}");
			if (null == loadObj)
			{
				return null;
			}

			Object createdObj = Object.Instantiate(loadObj);
			GameObject goForSinglton = createdObj as GameObject;
			if (null == goForSinglton)
			{
				// MonoBehaviour기반이 아니라면 잘못된 객체라서 제거필요
				Object.DestroyImmediate(createdObj);
				return null;
			}

			goForSinglton.name = goForSinglton.name.Replace("(Clone)", "");

			return goForSinglton.GetComponent<T>();
		}
	}

}//namespace