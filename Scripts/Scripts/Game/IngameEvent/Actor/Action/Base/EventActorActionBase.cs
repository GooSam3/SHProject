using UnityEngine;
using System;

namespace IngameEvent
{
	/// <summary> 이벤트 actor에게 특수한 형태의 action을 수행하도록 한다. </summary>
	public abstract class EventActorActionBase : MonoBehaviour
	{
		protected IngameEventActorBase Actor { get; private set; }

		private Action mEventFinishAction;
		private Action mEventEndAction;

		public void BeginAction(IngameEventActorBase owner, Action onFinish, Action onEndAction)
		{
			Actor = owner;

			mEventFinishAction = onFinish;

			InvokeBeginAction();
		}

		private void InvokeBeginAction()
		{
			CancelInvoke(nameof(InvokeBeginAction));
			if(null == Actor.Pawn)
			{
				Invoke(nameof(InvokeBeginAction), 0.1f);
				return;
			}

			BeginActionImpl();
		}

		public void StopAction()
		{
			StopAllCoroutines();
			StopActionImpl();

			mEventEndAction?.Invoke();
			mEventFinishAction?.Invoke();
		}

		private void OnDestroy()
		{
			StopActionImpl();
		}

		protected abstract void BeginActionImpl();
		protected abstract void StopActionImpl();
	}
}
