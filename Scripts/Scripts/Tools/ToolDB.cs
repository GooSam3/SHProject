#if UNITY_EDITOR

using System.Collections.Generic;
using GameDB;
using OfficeOpenXml;
namespace Tools
{
	public interface IToolTableBase
	{
		void Load();
		void Save();
		void Clear();
	}

	/// <summary> 맵 에디터용 테이블 처리용 클래스 </summary>
	public abstract class ToolTableBase<KEY_TYPE, TABLE_TYPE> : IToolTableBase
		where TABLE_TYPE : class
	{
		public Dictionary<KEY_TYPE, TABLE_TYPE> DicTable { get; protected set; } = new Dictionary<KEY_TYPE, TABLE_TYPE>();

		/// <summary>테이블 저장/로드시 사용될 시트명</summary>
		protected abstract string SheetName { get; }

		private string mTableFilePath;

		public ToolTableBase(string _xlsxPath)
		{
			mTableFilePath = _xlsxPath;
		}

		public void Load()
		{
			var sheet = ToolUtils.LoadExcelSheet(
				mTableFilePath,
				SheetName,
				out var excelPackage);

			Clear();

			OnLoad(sheet, excelPackage);
		}

		public void Save()
		{
			var sheet = ToolUtils.LoadExcelSheet(
				mTableFilePath,
				SheetName,
				out var excelPackage);

			OnSave(sheet, excelPackage);

			// 저장
			excelPackage.Save();
		}

		/// <summary></summary>
		protected abstract void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage);
		/// <summary></summary>
		protected abstract void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage);

		protected Dictionary<string, int> GetColumnIndexes(ExcelWorksheet _sheet)
		{
			Dictionary<string, int> indexes = new Dictionary<string, int>();

			var fields = typeof(TABLE_TYPE).GetFields();
			foreach (var f in fields) {
				indexes.Add(f.Name, 0);
			}

			for (int i = 1; i <= _sheet.Dimension.Columns; i++) {
				if (_sheet.GetValue(1, i) == null)
					continue;

				// 컬럼명 구하기
				string str = _sheet.GetValue<string>(1, i);

				if (!indexes.ContainsKey(str))
					continue;

				indexes[str] = i;
			}

			return indexes;
		}

		public virtual void Clear()
		{
			DicTable.Clear();
		}
	}

	#region TABLES

	public class Tool_EffectTable : ToolTableBase<uint, Effect_Table>
	{
		private List<FXSoundEffectData> effectList = new List<FXSoundEffectData>();

		protected override string SheetName => nameof(Effect_Table);

		public Tool_EffectTable(string _xlsxPath) : base(_xlsxPath) { }

		protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
		{
			Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

			for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx) {
				var tableData = new Effect_Table() {
					EffectID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Effect_Table.EffectID)]),
					EffectSoundID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Effect_Table.EffectSoundID)]),
					EffectFile = _sheet.GetValue<string>(rowIdx, indexes[nameof(Effect_Table.EffectFile)]),
					EffectDelayTime = _sheet.GetValue<float>(rowIdx, indexes[nameof(Effect_Table.EffectDelayTime)]),
				};

				if (0 == tableData.EffectID) {
					continue;
				}

				DicTable.Add(tableData.EffectID, tableData);
			}
		}

		protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
		{
			Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

			for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx) {
				var effectId = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Effect_Table.EffectID)]);

				var data = effectList.Find(v => v.EffectID == effectId && v.IsChange);
				if (data == null) {
					continue;
				}

				_sheet.SetValue(rowIdx, indexes[nameof(Effect_Table.EffectDelayTime)], data.EffectDelayTime_SV);
				_sheet.SetValue(rowIdx, indexes[nameof(Effect_Table.EffectSoundID)], data.EffectSoundID);
			}

			_excelPackage.Save();
		}

		public void Save(List<FXSoundEffectData> _effectList)
		{
			effectList = _effectList;
			this.Save();
		}
	}

	public class Tool_SoundTable : ToolTableBase<uint, Sound_Table>
	{
		protected override string SheetName => nameof(Sound_Table);

		public Tool_SoundTable(string _xlsxPath) : base(_xlsxPath) { }

		protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
		{
			Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

			for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx) {
				var tableData = new Sound_Table() {
					SoundID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Sound_Table.SoundID)]),
					SoundFile = _sheet.GetValue<string>(rowIdx, indexes[nameof(Sound_Table.SoundFile)]),
					SoundVolume = _sheet.GetValue<float>(rowIdx, indexes[nameof(Sound_Table.SoundVolume)]),
				};

				if (0 == tableData.SoundID) {
					continue;
				}

				DicTable.Add(tableData.SoundID, tableData);
			}
		}

		protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
		{
		}
	}

	#endregion
}

#endif