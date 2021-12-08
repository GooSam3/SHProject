using UnityEditor;

public abstract class EditorPaneBase<OWNER_WINDOW> where OWNER_WINDOW : EditorWindow
{
	public OWNER_WINDOW Owner { get; }

	public EditorPaneBase(OWNER_WINDOW _owner)
	{
		this.Owner = _owner;
	}

	public abstract void OnEnable();
	public abstract void OnDisable();

	public abstract void DrawGUI();
	//public abstract void DrawSceneGUI();
}
