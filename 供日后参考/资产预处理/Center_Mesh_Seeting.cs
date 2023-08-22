using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Center_Mesh_Seeting", menuName = "Center_Mesh_Seeting")]
public class Center_Mesh_Seeting : ScriptableObject
{

    /// <summary>
    /// 是否开启一律不
    /// </summary>
    [Header("是否开启一律不归中轴心？")]
    public bool isAllNot = true;
}
