using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ColorReference
{
    public bool UseConstant = true;
    public Color32 ConstantValue = new UnityEngine.Color32(255, 255, 255, 255);
    public ColorVariable Variable = null;

    public ColorReference()
    {

    }
    public Color32 Value
    {
        get { return UseConstant ? ConstantValue : Variable.Value; }
    }

}
