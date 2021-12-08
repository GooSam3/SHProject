using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// 대상 객체를 고정해서 바라본다.
/// </summary>
[EditorTool("LookAt Tool", typeof(GameObject))]
public class LookatTool : EditorTool
{
	GUIContent cachedIcon;

	// NOTE: as were caching this, unity will serialize it between compiles! so if we want to test out new looks,
	// just return the new GUIContent and bypass the cache until were happy with the icon...
	public override GUIContent toolbarIcon
	{
		get
		{
			if (cachedIcon == null)
				cachedIcon = EditorGUIUtility.IconContent("ViewToolOrbit", "|LookAt Tool");
			return cachedIcon;
		}
	}

	public override void OnToolGUI(EditorWindow window)
	{
		var view = SceneView.lastActiveSceneView;

		// If there are multiple selected objects, we want to focus the look position somewhere in the middle..
		if (Selection.transforms.Length > 1)
		{
			Vector3 center = Vector3.zero;
			foreach (var transform in Selection.transforms)
			{
				center += transform.position;
			}

			center = center / Selection.transforms.Length;
			view.LookAt(center);

			// Lets draw a tiny sphere at that center point, so we can verify it works
			Handles.SphereHandleCap(0, center, Quaternion.identity, 0.5f, EventType.Repaint);
		}
		// If we only have one selected object, thats way easier, we just look at it
		else if (Selection.activeTransform != null)
		{
			view.LookAt(Selection.activeTransform.position);
		}
	}
}