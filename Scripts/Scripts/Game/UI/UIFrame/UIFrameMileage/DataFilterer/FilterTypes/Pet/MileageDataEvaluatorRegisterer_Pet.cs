using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MileageDataEvaluatorRegisterer
{
    void Initialize_Pet(MileageDataWrapper dataWrapper)
    {
        dataWrapper.AddEvaluatorPredicate_Pet(MileageDataEvaluatorKey.Pet_All,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View; });

        dataWrapper.AddEvaluatorPredicate_Pet(MileageDataEvaluatorKey.Pet_PetType_Pet,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.PetType == GameDB.E_PetType.Pet; });

        dataWrapper.AddEvaluatorPredicate_Pet(MileageDataEvaluatorKey.Pet_PetType_Vehicle,
            (tableData) => { return tableData.ViewType == GameDB.E_ViewType.View && tableData.PetType == GameDB.E_PetType.Vehicle; });
    }
}
