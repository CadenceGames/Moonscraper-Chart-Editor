﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionPool : SongObjectPool
{
    public SectionPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<SectionController>())
            throw new System.Exception("No SectionController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        SectionController controller = sCon as SectionController;

        // Assign pooled objects
        controller.section = (Section)songObject;
        controller.gameObject.SetActive(true);
    }
}
