using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MileageDataEvaluatorRegisterer
{
    public void Initialize(MileageDataWrapper dataWrapper)
    {
        Initialize_Item(dataWrapper);
        Initialize_Change(dataWrapper);
        Initialize_Pet(dataWrapper);
        Initialize_Rune(dataWrapper);
    }
}
