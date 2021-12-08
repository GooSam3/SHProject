using FlatBuffers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public class ZNetHelper
{
	#region ========:: for FlatBuffer ::========

	/// <summary>
	/// <see cref="IFlatbufferObject"/>
	/// </summary>
	private static Dictionary<string, MethodInfo> RootAsMethodInfoDic = new Dictionary<string, MethodInfo>();

	/// <summary>
	/// "GetRootAs" 함수를 찾아서 반환해준다.
	/// </summary>
	/// <remarks> IFlatbufferObject 상속받은 클래스에서 객체생성용 함수를 말한다. </remarks>
	public static MethodInfo GetRootAsMethod(Type type)
	{
		if (!RootAsMethodInfoDic.ContainsKey(type.FullName))
		{
			foreach (MethodInfo mInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				if (mInfo != null && mInfo.Name.Contains("GetRootAs"))
				{
					if (mInfo.GetParameters().Length == 1)
					{
						RootAsMethodInfoDic.Add(type.FullName, mInfo);
						break;
					}
				}
			}
		}

		return RootAsMethodInfoDic[type.FullName];
	}

	/// <summary> <see cref="IFlatbufferObject"/> 기반 프로토콜 타입 객체로 변환 </summary>
	/// <typeparam name="FBOBJECT_TYPE"><see cref="IFlatbufferObject"/> 기반 타입 </typeparam>
	public static FBOBJECT_TYPE ConvertFBObject<FBOBJECT_TYPE>(List<byte> _objBytes) where FBOBJECT_TYPE : IFlatbufferObject
	{
		if (_objBytes == null || _objBytes.Count <= 0)
			return default;

		ByteBuffer bytebuffer = new ByteBuffer(_objBytes.ToArray());

		FBOBJECT_TYPE fbObject = (FBOBJECT_TYPE)ZNetHelper.GetRootAsMethod(typeof(FBOBJECT_TYPE)).Invoke(null, new object[] { bytebuffer });

		return fbObject;
	}

	/// <summary> <see cref="IFlatbufferObject"/> 기반 프로토콜 타입 객체로 변환 </summary>
	public static FBOBJECT_TYPE ConvertFBObject<FBOBJECT_TYPE>(ByteBuffer bytebuffer)
	{
		try
		{
			FBOBJECT_TYPE fbObject = (FBOBJECT_TYPE)ZNetHelper.GetRootAsMethod(typeof(FBOBJECT_TYPE)).Invoke(null, new object[] { bytebuffer });
			return fbObject;
		}
		catch (System.Exception e)
		{
			ZLog.Log(ZLogChannel.System, $"{typeof(FBOBJECT_TYPE).FullName} Convert failed! : {e.Message}, {e.StackTrace}");
			return default;
		}
	}

	/// <summary> <see cref="IFlatbufferObject"/> 기반 프로토콜 타입 객체로 변환 </summary>
	/// <typeparam name="FBOBJECT_TYPE"><see cref="IFlatbufferObject"/> 기반 타입 </typeparam>
	public static FBOBJECT_TYPE ConvertFBObject<FBOBJECT_TYPE>(byte[] _bytes) where FBOBJECT_TYPE : IFlatbufferObject
	{
		if (_bytes == null || _bytes.Length <= 0)
			return default;

		ByteBuffer bytebuffer = new ByteBuffer(_bytes);

		FBOBJECT_TYPE fbObject = (FBOBJECT_TYPE)ZNetHelper.GetRootAsMethod(typeof(FBOBJECT_TYPE)).Invoke(null, new object[] { bytebuffer });

		return fbObject;
	}

	#endregion

	/// <summary>
	/// 대상 객체가 가지고 있는 값들을 문자열화 시켜서 반환해준다.
	/// </summary>
	/// <param name="typeObject"></param>
	public static string GetPropertyStrings(object typeObject)
	{
		if (null == typeObject)
		{
			return "(null)";
		}

		System.Type type = typeObject.GetType();
		StringBuilder sb = new StringBuilder();
		PropertyInfo[] properties = type.GetProperties();

		try
		{
			for (int iProp = 0; iProp < properties.Length; ++iProp)
			{
				MethodInfo propGetMethod = properties[iProp].GetGetMethod();
				if (null == propGetMethod)
				{
					continue;
				}
				if (propGetMethod.Name.Equals("get_ByteBuffer"))
				{
					continue;
				}

				ParameterInfo[] parameters = propGetMethod.GetParameters();

				if (0 < parameters.Length)
				{
					continue;
				}

				object property = propGetMethod.Invoke(typeObject, null);

				string strElementName = "";
				MethodInfo getElementMethod = null;

				// 이름이 Length로 끝나는 int 프로퍼티는 목록에 대한 길이다.
				if (propGetMethod.ReturnType == typeof(int) && propGetMethod.Name.EndsWith("Length"))
				{
					strElementName = propGetMethod.Name.Substring(4, propGetMethod.Name.Length - 10);
					getElementMethod = type.GetMethod(strElementName);
				}

				// 목록을 가져오는 함수가 있으면
				if (null != getElementMethod)
				{
					sb.Append(strElementName); // Removes 'get_'
					sb.Append("=[");

					// 목록의 길이만큼
					int elementLength = (int)property;
					// 해당 오브젝트의 프로퍼티들을 재귀로 출력한다.
					for (int iElem = 0; iElem < elementLength; ++iElem)
					{
						object element = getElementMethod.Invoke(typeObject, new object[] { iElem });
						sb.Append("{");
						if (element is IFlatbufferObject)
							sb.Append(GetPropertyStrings(element));
						else
							sb.Append(element.ToString());
						sb.Append("}");
						if (iElem + 1 < elementLength)
							sb.Append(", ");
					}
					sb.Append("]");
				}
				// 하나의 버퍼 오브젝트를 가져오는 프로퍼티는 해당 오브젝트 프로퍼티들을 재귀로 출력
				else if (typeof(IFlatbufferObject).IsAssignableFrom(System.Nullable.GetUnderlyingType(propGetMethod.ReturnType)))
				{
					sb.Append(propGetMethod.Name.Substring(4)); // Removes 'get_'
					sb.Append("={");
					sb.Append(GetPropertyStrings(property));
					sb.Append("}");
				}
				else
				{
					sb.Append(propGetMethod.Name.Substring(4)); // Removes 'get_'
					sb.Append("=");
					sb.Append(null != property ? property.ToString() : "(null)");
				}

				if (iProp + 1 < properties.Length)
					sb.Append(", ");
			}
		}
		catch (System.Exception e)
		{
			ZLog.Log(ZLogChannel.Default, ZLogLevel.Error, $"Log make failed! : {e.Message} , {e.StackTrace}");
		}

		return sb.ToString();
	}

	public static string GetFieldStrings(object typeObject)
	{
		if (null == typeObject)
		{
			return "(null)";
		}

		System.Type type = typeObject.GetType();
		StringBuilder sb = new StringBuilder();
		FieldInfo[] fields = type.GetFields();

		try
		{
			for (int iField = 0; iField < fields.Length; ++iField)
			{
				object valueObj = fields[iField].GetValue(typeObject);
				if (null == valueObj)
				{
					continue;
				}				
				
				sb.Append(fields[iField].Name);
				sb.Append("=");
				sb.Append(null != valueObj ? valueObj.ToString() : "(null)");

				if (iField + 1 < fields.Length)
					sb.Append(", ");
			}
		}
		catch (System.Exception e)
		{
			ZLog.Log(ZLogChannel.Default, ZLogLevel.Error, $"Log make failed! : {e.Message} , {e.StackTrace}");
		}

		return sb.ToString();
	}
}
