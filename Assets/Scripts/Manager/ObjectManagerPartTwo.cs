﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ObjectManager
{
    /// <summary>
    /// 节省GetComponent开销
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public OfflineData FindOfflineData(GameObject gameObject)
    {
        OfflineData offlineData = null;
        if (ObjectItemsInstanceTempDic.TryGetValue(gameObject.GetInstanceID(), out ObjectItem objectItem))
        {
            offlineData = objectItem.OfflineData;
        }
        return offlineData;
    }
}