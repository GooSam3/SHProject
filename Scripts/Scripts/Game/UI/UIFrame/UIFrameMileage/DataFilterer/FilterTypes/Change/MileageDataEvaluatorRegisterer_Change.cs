using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

public partial class MileageDataEvaluatorRegisterer
{
    void Initialize_Change(MileageDataWrapper dataWrapper)
    {
        /// 기본적으로 ViewType 이 View 여야 보여줌 
        #region Character Type
        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_All,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Archer,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.UseAttackType == GameDB.E_CharacterType.Archer; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Assassin,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.UseAttackType == GameDB.E_CharacterType.Assassin; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Gladiator,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.UseAttackType == GameDB.E_CharacterType.Knight; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Wizard,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.UseAttackType == GameDB.E_CharacterType.Wizard; });

        #endregion

        #region Element

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Elemental_All,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Elemental_Fire,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.AttributeType == GameDB.E_UnitAttributeType.Fire; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Elemental_Water,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.AttributeType == GameDB.E_UnitAttributeType.Water; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Elemental_Electric,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.AttributeType == GameDB.E_UnitAttributeType.Electric; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Elemental_Light,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.AttributeType == GameDB.E_UnitAttributeType.Light; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_Elemental_Dark,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.AttributeType == GameDB.E_UnitAttributeType.Dark; });

        #endregion

        #region Obtain Or Not 

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_ObtainedOrNotObtained,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View; });

        dataWrapper.AddEvaluatorPredicate_Change(MileageDataEvaluatorKey.Character_NotObtained,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && Me.CurCharData.GetChangeDataByTID(tableData.ChangeID) == null; });

        #endregion
    }
}
