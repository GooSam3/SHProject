﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkDirection_GroupObtained : MarkDirectionControllerBase
{
    public override bool SupportSpeedControl => true;
    public override bool AutoStopOnResourceDeactived => true;

    public override void Play(MarkDirectionHandler.DirectionParam param)
    {
        base.Play(param);
    }

    public override void Reset()
    {
        base.Reset();

    }

    public override void ProcessUpdate()
    {

    }
}
