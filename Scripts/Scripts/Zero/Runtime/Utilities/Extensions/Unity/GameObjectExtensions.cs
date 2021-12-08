using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유니티 GameObject 확장함수들
/// </summary>
/// <remarks>
/// http://rubberduckygames.com/useful-extension-methods/
/// </remarks>
public static class GameObjectExtensions
{
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		//return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();

		// 위에꺼로 NavMeshAgent추가 하다가 안되서..
		T result = gameObject.GetComponent<T>();
		if (result == null)
		{
			result = gameObject.AddComponent<T>();
		}
		return result == null ? gameObject.AddComponent<T>() : result;
	}

	public static bool HasComponent<T>(this GameObject gameObject) where T : Component
	{
		return gameObject.GetComponent<T>() != null;
	}

	public static void DestroyComponent<T>(this GameObject gameObject) where T : Component
	{
		T result = gameObject.GetComponent<T>();
		if (result != null)
		{
			Object.DestroyImmediate(result);
		}
	}

	public static void DestroyChildren(this GameObject gameObject)
	{
		gameObject.transform.DestroyChildren();
	}

	/// <summary>
	/// Sets the layer of the calling object and all its children.
	/// </summary>
	public static void SetLayersRecursively(this GameObject gameObject, int layer)
	{
		gameObject.layer = layer;

		foreach (Transform child in gameObject.transform)
		{
			child.gameObject.SetLayersRecursively(layer);
		}
	}

	/// <summary> Type에 해당하는 컴포넌트의 레이어는 별도 처리 </summary>
	public static void SetLayersRecursively<TYPE>(this GameObject gameObject, int layer, int typeLayer) where TYPE : Component
	{
		if(null != gameObject.GetComponent<TYPE>())
			gameObject.layer = typeLayer;
		else
			gameObject.layer = layer;

		foreach (Transform child in gameObject.transform)
		{
			child.gameObject.SetLayersRecursively<TYPE>(layer, typeLayer);
		}
	}

	/// <summary>
	/// Sets the layer of the calling object and all its children.
	/// </summary>
	public static void SetLayersRecursively(this GameObject gameObject, string layerName)
	{
		int layer = LayerMask.NameToLayer(layerName);
		gameObject.SetLayersRecursively(layer);
	}

	/// <summary>
	/// Checks if a GameObject has been destroyed.
	/// </summary>
	/// <param name="gameObject">GameObject reference to check for destructedness</param>
	/// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
	public static bool IsDestroyed(this GameObject gameObject)
	{
		// UnityEngine overloads the == opeator for the GameObject type
		// and returns null when the object has been destroyed, but 
		// actually the object is still there but has not been cleaned up yet
		// if we test both we can determine if the object has been destroyed.
		return gameObject == null && !ReferenceEquals(gameObject, null);
	}

	public static GameObject FindChild(this GameObject parent, string name, bool includeInactive = true, bool ignoreCase = false)
	{
		Transform[] children = parent.GetComponentsInChildren<Transform>(includeInactive);
		for (int i = 0; i < children.Length; ++i) {
			string compareString = children[i].gameObject.name;
			if (ignoreCase) {
				compareString = compareString.ToLower();
				name = name.ToLower();
			}

			if (compareString == name) {
				return children[i].gameObject;
			}
		}
		return null;
	}

	public static List<GameObject> FindChildren(this GameObject parent, string containName, bool includeInactive = true)
	{
		List<GameObject> result = new List<GameObject>();

		Transform[] children = parent.GetComponentsInChildren<Transform>(includeInactive);
		for (int i = 0; i < children.Length; ++i) {
			if (children[i].gameObject.name.Contains(containName) == true) {
				result.Add(children[i].gameObject);
			}
		}
		return result;
	}

	public static List<GameObject> FindChildren<T>(this GameObject parent, string ContainsName, bool includeInactive = true) where T : Component
	{
		List<GameObject> result = new List<GameObject>();

		T[] children = parent.GetComponentsInChildren<T>(includeInactive);
		for (int i = 0; i < children.Length; ++i) {
			if (children[i].gameObject.name.Contains(ContainsName)) {
				result.Add(children[i].gameObject);
			}
		}
		return result;
	}

	public static List<T> FindChildrenToList<T>(this GameObject parent, bool includeInactive = true) where T : Component
	{
		List<T> result = new List<T>();
		T[] children = parent.GetComponentsInChildren<T>(includeInactive);
		for (int i = 0; i < children.Length; ++i) {
			result.Add(children[i]);
		}
		return result;
	}

	public static T FindChildComponent<T>(this GameObject parent, string name, bool includeInactive = true) where T : Component
	{
		T[] children = parent.GetComponentsInChildren<T>(includeInactive);
		for (int i = 0; i < children.Length; ++i) {
			if (children[i].gameObject.name == name) {
				return children[i];
			}
		}
		return null;
	}

	public static List<T> FindChildrenComponents<T>(this GameObject parent, string name, bool includeInactive = true) where T : Component
	{
		List<T> result = new List<T>();
		T[] children = parent.GetComponentsInChildren<T>(includeInactive);
		for (int i = 0; i < children.Length; ++i) {
			if (children[i].gameObject.name == name) {
				result.Add(children[i]);
			}
		}
		return result;
	}
}
