#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEditor;

namespace Tools
{
	public class ToolUtils
	{
		public static string GetExcelPath(string _xlsxName)
		{
			string folderPath = EditorPrefs.GetString("ICARUS_GameDBPath", string.Empty);
			string excelName = $"{_xlsxName}.xlsx";
			var fullPath = Path.Combine(folderPath, excelName);
			return fullPath;
		}

		public static object LoadTable<KEY, VALUE>(ToolTableBase<KEY, VALUE> _newTable) where VALUE : class
		{
			_newTable.Load();

			List<IToolTableBase> mListTables = new List<IToolTableBase>();

			mListTables.Add(_newTable);

			return _newTable;
		}

		/// <summary> xlsx파일에 존재하는 시트를 로드해준다. </summary>
		public static ExcelWorksheet LoadExcelSheet(string filePath, string sheetName, out ExcelPackage exp)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists) {
				throw new Exception($"존재하지 않는 파일 : {fileInfo.FullName}");
			}

			if (FileHelper.IsFileinUse(fileInfo)) {
				EditorApplication.isPlaying = false;
				EditorUtility.DisplayDialog("경고", "파일이 사용중입니다. 닫어주세요", "확인");
				throw new Exception($"{fileInfo.FullName} 이 수정가능한지 확인바람!!(열려있을 가능성 높음)");
			}

			exp = new ExcelPackage(fileInfo);
			if (null == exp) {
				return null;
			}

			ExcelWorksheet foundSheet = exp.Workbook.Worksheets[sheetName];
			if (exp.Workbook.Worksheets.Count <= 0 || null == foundSheet) {
				throw new Exception($"{filePath}에 Worksheet가 존재하지 않습니다.");
			}

			return foundSheet;
		}
	}
}

#endif