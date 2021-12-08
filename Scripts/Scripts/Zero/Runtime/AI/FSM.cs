using System.Collections.Generic;
using UnityEngine;

/*----------------------------------------------------------
 * NameSpace	: FSM
 * Author		: 김경호
 * Desc			: Unity3D 컴포넌트 기반 Finite State Machine.
 ----------------------------------------------------------*/
namespace FSM
{
	/*----------------------------------------------------------
     * ClassName	: FSM<EVENT, STATE, PARENT>
     * Author		: 김경호
     ----------------------------------------------------------*/
	/// <summary>
	/// 상태 머신 클래스.
	/// </summary>
	/// <typeparam name="EVENT">public enum 형식의 상태에 발동될 이벤트 목록</typeparam>
	/// <typeparam name="STATE">public enum 형식의 상태 목록</typeparam>
	/// <typeparam name="PARENT">상태를 가지는 객채</typeparam>
	public class FSM<EVENT, STATE, PARENT>
		where EVENT : struct, System.Enum
		where STATE : struct, System.Enum
	{
		protected PARENT mParent;
		public STATE Current_State { get; private set; }
		protected bool isEntering;
		Devcat.EnumDictionary<STATE, Dictionary<EVENT, STATE>> TransitionMap;
		Devcat.EnumDictionary<STATE, BaseState<PARENT>> StateMap;
		public BaseState<PARENT> Current => StateMap[Current_State];

		/// <summary> GUI 함수 작동 여부 </summary>
		public bool DrawOnGui { get; set; } = true;

		/// <summary>
		/// 생성자.
		/// </summary>
		/// <param name="parent">상태를 가지는 객채 자신(this)/</param>
		public FSM(PARENT parent)
		{
			mParent = parent;
			Initialize();
		}

		/// <summary> </summary>
		void Initialize()
		{
			TransitionMap = new Devcat.EnumDictionary<STATE, Dictionary<EVENT, STATE>>();
			StateMap = new Devcat.EnumDictionary<STATE, BaseState<PARENT>>(32);
		}

		/// <summary> </summary>
		public void Release()
		{
			if (null != StateMap[Current_State])
				StateMap[Current_State].OnExit(ReleaseCb);
		}

		public void CancelInvokeALL()
		{
			foreach (BaseState<PARENT> ibs in StateMap.Values)
			{
				if (null == ibs)
					continue;

				ibs.CancelInvoke();
			}
		}

		void ReleaseCb()
		{
			foreach (BaseState<PARENT> ibs in StateMap.Values)
			{
				if (null == ibs)
					continue;

				ibs.OnRelease();
			}
			StateMap.Clear();
			StateMap = null;
			TransitionMap.Clear();
			TransitionMap = null;
		}

#if UNITY_EDITOR || UNITY_ANDROID
		/// <summary> Unity IMGUI용 </summary>
		public void OnGUI_Dev()
		{
			if (!DrawOnGui)
				return;

			StateMap[Current_State].Dev_OnGUI();
		}
#endif

		public bool IsEnable { get { return EnableFlag; } }
		bool EnableFlag = false;
		public void Enable(STATE state)
		{
			if (EnableFlag)
			{
				ChangeState(state);
			}
			else
			{
				Current_State = state;
				Enable(true);
			}
		}
		private void Enable(bool flag)
		{
			if (EnableFlag == flag)
				return;
			else EnableFlag = flag;

			if (EnableFlag)
			{
				if (StateMap.ContainsKey(Current_State))
					StateMap[Current_State].OnEnter(null);
			}

			else StateMap[Current_State].OnExit(null);
		}

		/// <summary>
		/// 상태 등록.
		/// </summary>
		/// <param name="_state">상태 값</param>
		/// <param name="_stateinterface">상태 클래스</param>
		public void AddState(STATE _state, BaseState<PARENT> _stateinterface)
		{
			StateMap.Add(_state, _stateinterface);
			_stateinterface.OnInitialize(mParent);
		}

		public bool RemoveState(STATE _state)
		{
			if (StateMap.TryGetValue(_state, out var _stateComp))
			{
				Object.DestroyImmediate(_stateComp);
			}

			return StateMap.Remove(_state);
		}

		//이벤트 값을 얻어옴 ( 지금 상태에서 타겟 상태로 가기위한 이벤트가 있는지 확인 후 있으면 그 이벤트를 줌 )
		public EVENT GetEvent(STATE _BaseState, STATE _TargetState)
		{
			if (!TransitionMap.ContainsKey(_BaseState))
				return default(EVENT);

			foreach (EVENT e in TransitionMap[_BaseState].Keys)
			{
				if (TransitionMap[_BaseState][e].Equals(_TargetState))
				{
					return e;
				}
			}

			return default(EVENT);
		}

		/// <summary>
		/// 이벤트 등록.
		/// 적용할 상태에서 발생될 이벤트에 이동할 상태를 등록한다.
		/// </summary>
		/// <param name="_state">적용 할 상태</param>
		/// <param name="_event">발생될 이벤트</param>
		/// <param name="_targetstate">목표 상태</param>
		public void RegistEvent(STATE _state, EVENT _event, STATE _targetstate)
		{
			try
			{
				if (!TransitionMap.ContainsKey(_state))
					TransitionMap.Add(_state, new Dictionary<EVENT, STATE>());
				TransitionMap[_state].Add(_event, _targetstate);
			}
			catch (System.Exception e)
			{
				ZLog.Log(ZLogChannel.Default, ZLogLevel.Error, $"FiniteStateMap Add Error : {_event}\nException : {e}");
				Debug.Break();
			}
		}

		/// <summary>
		/// 이벤트를 발생시켜 현재 상태에 등록된 이벤트에 맞는 상태를 변경한다.
		/// </summary>
		/// <param name="_event">발생되는 이벤트</param>
		/// <param name="args">OnEnter 로 전해질 값</param>
		/// <returns></returns>
		public virtual bool ChangeState(EVENT _event, params object[] args)
		{
			if (isEntering)
			{
				ZLog.Log(ZLogChannel.Default, ZLogLevel.Warning, $"Current : {Current_State}, Target : {_event} State Map Error.");
				ZLog.Log(ZLogChannel.Default, ZLogLevel.Warning, $"current state is already entering!");
				return false;
			}

			if (TransitionMap[Current_State].ContainsKey(_event))
			{
				//현재 상태와 동일 할경우 
				if (StateMap[Current_State].Equals(TransitionMap[Current_State][_event]))
					return false;

				isEntering = true;
				StateMap[Current_State].OnExit(delegate ()
				{
					Current_State = TransitionMap[Current_State][_event];
					StateMap[Current_State].OnEnter(delegate ()
					{
						isEntering = false;
					}, args);
				});
				return true;
			}
			else
			{
				ZLog.Log(ZLogChannel.Default, ZLogLevel.Warning, $"Current : {Current_State}, Target : {_event} State Map Error.");
				return false;
			}
		}

		/// <summary>
		/// 이벤트 없이 상태전이를 바로 한다.
		/// </summary>
		/// <param name="_event">새로운 상태</param>
		public virtual bool ChangeState(STATE _state, bool _bForce = false, params object[] args)
		{
			if (isEntering)
			{
				ZLog.Log(ZLogChannel.Default, ZLogLevel.Warning, $"Current : {Current_State}");
				ZLog.Log(ZLogChannel.Default, ZLogLevel.Warning, $"current state is already entering!");
				return false;
			}

			//현재 상태와 동일 할경우 
			if (false == _bForce && Current_State.Equals(_state))
				return false;

			isEntering = true;

			if (StateMap[Current_State] != null)
			{
				StateMap[Current_State].OnExit(delegate ()
				{
					Current_State = _state;
					StateMap[Current_State].OnEnter(delegate ()
					{
						isEntering = false;
					}, args);
				});
			}
			else
			{
				ZLog.Log(ZLogChannel.Default, $"Null State {Current_State}");
				Current_State = _state;
				if (StateMap[Current_State] != null)
				{
					StateMap[Current_State].OnEnter(delegate ()
					{
						isEntering = false;
					}, args);
				}
				else
				{
					ZLog.Log(ZLogChannel.Default, $"Null State {Current_State}");
				}
			}

			return true;
		}

		/// <summary>
		/// 현재 상태에 다음 상태를 전환을 위한 이벤트가 등록존재하는지 확인
		/// </summary>
		/// <param name="_event">확인할 이벤트.</param>
		/// <returns></returns>
		public bool CheckEvent(EVENT _event)
		{
			if (TransitionMap.ContainsKey(Current_State) && TransitionMap[Current_State].ContainsKey(_event))
			{
				if (StateMap[Current_State].Equals(TransitionMap[Current_State][_event]))
				{

					return false;
				}
				else
				{

					return true;
				}
			}

			return false;
		}

		public bool HasState(STATE state)
		{
			return StateMap.ContainsKey(state);
		}
	}
}
