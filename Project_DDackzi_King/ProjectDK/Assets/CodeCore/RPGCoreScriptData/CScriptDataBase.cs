using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;
using System;

abstract public class CScriptDataBase : CMonoBase
{
	[SerializeField]
	private List<TextAsset> TextScript = new List<TextAsset>();

	//------------------------------------------------------------
	internal void ImportScriptDataInitialize()
	{
		for (int i = 0; i < TextScript.Count; i++)
		{
			PrivScriptDataDecript(TextScript[i].bytes);
			OnScriptDataInitialize(TextScript[i].text);
		}
	}

	public void ImportScriptDataInitialize(byte[] aTextData)
	{
		PrivScriptDataDecript(aTextData);
		string strTextData = Encoding.Default.GetString(aTextData);
		OnScriptDataInitialize(strTextData);
	}

	public static void GlobalScriptDataReadField<INSTANCE>(INSTANCE pInstance, string strValueName, string strValue) where INSTANCE : class
	{
		Type pClassType = pInstance.GetType();

		FieldInfo pFieldInfo = pClassType.GetField(strValueName);
		if (pFieldInfo == null)
		{
//			Debug.LogWarning("[ScriptData] Invalid Value Name : " + strValueName);
			return;
		}

		if (pFieldInfo.FieldType == typeof(int))
		{
			pFieldInfo.SetValue(pInstance, int.Parse(strValue));
		}
		else if (pFieldInfo.FieldType == typeof(uint))
		{
			pFieldInfo.SetValue(pInstance, uint.Parse(strValue));
		}
		else if (pFieldInfo.FieldType == typeof(string))
		{
			pFieldInfo.SetValue(pInstance, strValue);
		}
		else if (pFieldInfo.FieldType == typeof(float))
		{
			pFieldInfo.SetValue(pInstance, float.Parse(strValue));
		}
		else if (pFieldInfo.FieldType == typeof(bool))
		{
			pFieldInfo.SetValue(pInstance, bool.Parse(strValue));
		}
		else if (pFieldInfo.FieldType.BaseType == typeof(Enum))
		{
			pFieldInfo.SetValue(pInstance, Enum.Parse(pFieldInfo.FieldType, strValue));
		}
		else if (pFieldInfo.FieldType == typeof(List<int>))
		{
			List<string> pListSeparate = PrivScriptDataSeparateComma(strValue);
			List<int> pListValue = pFieldInfo.GetValue(pInstance) as List<int>;
			for (int i = 0; i < pListSeparate.Count; i++)
			{				
				pListValue.Add(int.Parse(pListSeparate[i]));
			}
		}
		else if (pFieldInfo.FieldType == typeof(List<string>))
		{
			List<string> pListSeparate = PrivScriptDataSeparateComma(strValue);
			List<string> pListValue = pFieldInfo.GetValue(pInstance) as List<string>;
			for (int i = 0; i < pListSeparate.Count; i++)
			{
				pListValue.Add(pListSeparate[i]);
			}
		}
		else if (pFieldInfo.FieldType == typeof(List<float>))
		{
			List<string> pListSeparate = PrivScriptDataSeparateComma(strValue);
			List<float> pListValue = pFieldInfo.GetValue(pInstance) as List<float>;
			for (int i = 0; i < pListSeparate.Count; i++)
			{
				pListValue.Add(float.Parse(pListSeparate[i]));
			}
		}
	}


	//--------------------------------------------------------------
	protected void ProtScriptDataClear() // 메모리 절약을 위해 로드된 원본은 해재해 준다.
	{
		TextScript.Clear();
		OnScriptDataClear();
	}


	//------------------------------------------------------------
	private void PrivScriptDataDecript(byte[] aTextData)
	{
		// 향후 데이터 암호화 할것

	}


	private static List<string> PrivScriptDataSeparateComma(string strData)
	{
		List<string> pListResult = new List<string>();
		StringBuilder Note = new StringBuilder();
		for (int i = 0; i < strData.Length; i++)
		{
			char C = strData[i];
			if (C == ',')
			{
				pListResult.Add(Note.ToString());
				Note.Clear();
			}
			else
			{
				Note.Append(C);
			}
		}

		if (Note.Length > 0)
		{
			pListResult.Add(Note.ToString());
		}

		return pListResult;
	}


	//-------------------------------------------------------------
	protected virtual void OnScriptDataInitialize(string strTextData) { }
	protected virtual void OnScriptDataClear() { }
}
