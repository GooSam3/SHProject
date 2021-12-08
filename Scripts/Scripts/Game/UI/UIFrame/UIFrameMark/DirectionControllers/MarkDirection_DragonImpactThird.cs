using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkDirection_DragonImpactThird : MarkDirectionControllerBase
{
    public override bool SupportSpeedControl => true;
    public override bool AutoStopOnResourceDeactived => true;

    public override void Play(MarkDirectionHandler.DirectionParam param)
    {
        base.SetResourceWorldPos(param.WorldPos);
        base.Play(param);
    }
    public override void ProcessUpdate()
    {

    }
}
