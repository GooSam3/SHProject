using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// https://github.com/mminer/unity-extensions/blob/master/TransformExtensions.cs
/// https://github.com/rfadeev/unity-forge-extension-methods/blob/master/Source/ExtensionMethods/Transform/TransformExtentions.cs
/// </remarks>
public static class TransformExtentions
{
	/// <summary> 월드 위치 회전, 크기 한번에 설정 </summary>
	public static void SetTRS(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		transform.SetPositionAndRotation(pos, rot);
		transform.localScale = scale;
	}

	/// <summary> 로컬 위치 회전, 크기 한번에 설정 </summary>
	public static void SetLocalTRS(this Transform transform, Vector3 localPos, Quaternion localRot, Vector3 scale)
	{
		transform.localPosition = localPos;
		transform.localRotation = localRot;
		transform.localScale = scale;
	}

	public static void SetX(this Transform transform, float x)
	{
		transform.position = new Vector3(x, transform.position.y, transform.position.z);
	}

	public static void SetY(this Transform transform, float y)
	{
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}

	public static void SetZ(this Transform transform, float z)
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, z);
	}

	public static void AddChild(this Transform transform, GameObject childGameObject)
	{
		childGameObject.transform.SetParent(transform, false);
	}

	public static void AddChild(this Transform transform, GameObject childGameObject, bool worldPositionStays)
	{
		childGameObject.transform.SetParent(transform, worldPositionStays);
	}

	public static void AddChild(this Transform transform, Transform childTransform)
	{
		childTransform.SetParent(transform, false);
	}

	public static void AddChild(this Transform transform, Transform childTransform, bool worldPositionStays)
	{
		childTransform.SetParent(transform, worldPositionStays);
	}

	public static void DestroyChildren(this Transform transform)
	{
		for (var i = 0; i < transform.childCount; ++i)
		{
			Object.Destroy(transform.GetChild(i).gameObject);
		}
	}

	public static Transform FindTransform(this Transform t, string name)
	{
		Transform trans = t.Find(name);
		if (trans)
			return trans;
		else
		{
			foreach (Transform child in t)
			{
				trans = FindTransform(child, name);
				if (trans)
					return trans;
			}
		}
		return null;
	}
}
