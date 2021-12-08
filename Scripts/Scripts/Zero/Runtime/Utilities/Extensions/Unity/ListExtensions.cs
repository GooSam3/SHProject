using System;
using System.Collections.Generic;

public static class ListExtensions
{
	public static void Shuffle<T>(this List<T> list)
	{
		Random random = new Random();
		for (int i = 0; i < list.Count; ++i) {
			int index = random.Next(0, list.Count);
			T temp = list[i];
			list[i] = list[index];
			list[index] = temp;
		}
	}

	public static void Swap<T>(this List<T> list, int source, int dest)
	{
		T temp = list[dest];
		list[dest] = list[source];
		list[source] = temp;
	}

	public static void From<T>(this List<T> this_list, List<T> list)
	{
		for (int i = 0; i < list.Count; ++i) {
			this_list.Add(list[i]);
		}
	}

	public static void Replace<T>(this List<T> this_list, List<T> list)
	{
		this_list.Clear();
		for (int i = 0; i < list.Count; ++i) {
			this_list.Add(list[i]);
		}
	}

	public static T Random<T>(this List<T> this_list)
	{
		if (this_list.Count == 0) {
			return default;
		}

		int index = UnityEngine.Random.Range(0, this_list.Count);
		return this_list[index];
	}
}