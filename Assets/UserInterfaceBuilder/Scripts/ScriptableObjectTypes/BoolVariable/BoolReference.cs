using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoolReference
{
	public bool UseConstant = true;
	public bool ConstantValue = false;
	public BoolVariable Variable = null;

	public BoolReference()
	{

	}
	public bool Value
	{
		get { return UseConstant ? ConstantValue : Variable.Value; }
		set
		{
			if (UseConstant)
				ConstantValue = value;
			else
				Variable.Value = value;
		}
	}
}