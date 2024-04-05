using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class ExtensionMethod 
{
    //判断攻击目标是否在面前,点积判断
    private const float dotThreshold = 0.5f;
    public static bool IsFacingTarget (this Transform transform , Transform target)
   {
    var vectorToTarget = target.position -transform.position;
    vectorToTarget.Normalize();

    float dot = Vector3.Dot(transform.forward,vectorToTarget);

    return dot >= dotThreshold;
    

   }
}
