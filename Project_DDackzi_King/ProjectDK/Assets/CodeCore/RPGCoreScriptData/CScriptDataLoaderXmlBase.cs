using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Reflection;
using System;
public class CScriptDataLoaderXmlBase 
{
	public List<CScriptDataLoaderXmlBase> m_listChildElement = new List<CScriptDataLoaderXmlBase>();
	//------------------------------------------------------------
	public void DoScriptDataLoad(XmlElement pElem, string strNameSpace, Assembly pAssembly)
	{
		Type pClassType = GetType();
		XmlAttributeCollection pAttList = pElem.Attributes;
		for (int i = 0; i < pAttList.Count; i++)
		{
			PrivXmlAttribute(pAttList[i]);
		}

		OnScriptXMLLoadedAttribute();

		XmlNodeList pNodeList = pElem.ChildNodes;
		for (int i = 0; i < pNodeList.Count; i++)
		{
			XmlNode pNode = pNodeList[i];
			if (pNode.NodeType == XmlNodeType.Element)
			{
				PrivXmlElement(pNode as XmlElement, strNameSpace, pAssembly);
			}
		}
	}

	//--------------------------------------------------------------
	private void PrivXmlAttribute(XmlAttribute pAtt)
	{
		CScriptDataBase.GlobalScriptDataReadField(this, pAtt.Name, pAtt.Value);
	}

	private void PrivXmlElement(XmlElement pElem, string strNameSpace, Assembly pAssembly)
	{
		string strClassName = strNameSpace + pElem.Name;
		CScriptDataLoaderXmlBase pChildLoader = pAssembly.CreateInstance(strClassName) as CScriptDataLoaderXmlBase;
		if (pChildLoader == null)
		{
			Debug.LogError($"[DataLoader] Element invalid class Instance {pElem.Name} ");
			return;
		}

		m_listChildElement.Add(pChildLoader);
		pChildLoader.DoScriptDataLoad(pElem, strNameSpace, pAssembly);
	}

	//------------------------------------------------------------------------------------
	protected virtual void OnScriptXMLLoadedAttribute() { }

}
