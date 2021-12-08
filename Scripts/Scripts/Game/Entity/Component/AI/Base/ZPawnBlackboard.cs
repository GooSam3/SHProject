using UnityEngine;
using NodeCanvas.Framework;

#if UNITY_EDITOR
using UnityEditor;

namespace NodeCanvas.Editor
{
	[CustomEditor(typeof(ZPawnBlackboard))]
	public class ZPawnBlackboardInspector : UnityEditor.Editor
	{
		private ZPawnBlackboard bb { get { return (ZPawnBlackboard)target; } }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			BlackboardEditor.ShowVariables(bb);
			ParadoxNotion.Design.EditorUtils.EndOfInspector();
			if (Event.current.isMouse)
			{
				Repaint();
			}

			if (GUILayout.Button($"Sync Blackboard"))
			{
				bb.SyncBlackboard();
			}
		}
	}
}
#endif

namespace NodeCanvas.Framework
{
	public class ZPawnBlackboard : Blackboard
	{
		[SerializeField]
		private float MoveSpeed = 5f;

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			AddVariable(ZBlackbloardKey.MoveSpeed, MoveSpeed);
		}

#if UNITY_EDITOR
		public void SyncBlackboard()
		{
			MoveSpeed = GetVariableValue<float>(ZBlackbloardKey.MoveSpeed);
		}
#endif
	}
}