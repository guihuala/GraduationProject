using KidGame.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset]
public class data_config : ScriptableObject
{
    Dictionary<Type, IList> dataListT2LDic;

    private void InitDataDic()
    {
        if (dataListT2LDic != null) return;

        dataListT2LDic = new Dictionary<Type, IList>();
    }

    private void Add2Dic(Type type, IList list)
    {
        dataListT2LDic.Add(type, list);
    }
}