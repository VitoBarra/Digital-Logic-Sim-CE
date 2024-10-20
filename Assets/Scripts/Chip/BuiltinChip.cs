using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuiltinChip : SpawnableChip
{
    public bool DefaultEnabled = true;
    private ChipPackageDisplay chipPackageDisplay;


    public void OnValidate()
    {
        if (ScalingManager.i is null) return;
        chipPackageDisplay ??= GetComponent<ChipPackageDisplay>();
        chipPackageDisplay?.Init();

    }
}