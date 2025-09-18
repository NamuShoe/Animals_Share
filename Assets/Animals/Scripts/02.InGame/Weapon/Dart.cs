using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : Projectile
{
    private bool isPiercingFix = false;

    protected override void Init()
    {
        isPiercingFix = weaponController.isEpic;
        base.Init();
    }

    protected override float GetPiercingPercent(int max, int current)
    {
        if (isPiercingFix) {
            var piercingPercent = (max - current) switch
            {
                0 => 1.0f,
                _ => 0.75f
            };
            return piercingPercent;
        }
        else {
            return base.GetPiercingPercent(max, currentPiercing);
        }
    }
}
