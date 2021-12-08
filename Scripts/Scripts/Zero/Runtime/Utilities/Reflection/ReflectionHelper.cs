using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zero
{
	public static class ReflectionHelper
	{
		public static List<Type> FindAllDerivedTypes<T>()
		{
			return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
		}

		public static List<Type> FindAllDerivedTypes(Type type)
		{
			return FindAllDerivedTypes(Assembly.GetAssembly(type), type);
		}

		/// <summary> [선택한 타입]을 [상속받은 모든 타입]들을 찾아준다. </summary>
		/// <remarks></remarks>
		public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
		{
			var derivedType = typeof(T);
			return assembly
				.GetTypes()
				.Where(t =>
					t != derivedType &&
					derivedType.IsAssignableFrom(t)
					).ToList();
		}

		public static List<Type> FindAllDerivedTypes(Assembly assembly, Type findType)
		{
			var derivedType = findType;
			return assembly
				.GetTypes()
				.Where(t =>
					t != derivedType &&
					derivedType.IsAssignableFrom(t)
					).ToList();
		}

		public static List<Type> GetAllDerivedTypes<T>(this System.AppDomain aAppDomain)
		{
			var result = new List<Type>();
			var assemblies = aAppDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				result.AddRange(FindAllDerivedTypes<T>(assembly));
			}
			return result;
		}
	}
}