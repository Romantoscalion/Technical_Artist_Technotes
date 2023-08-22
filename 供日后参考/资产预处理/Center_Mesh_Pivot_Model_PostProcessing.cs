using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 导入资产时，自动居中模型的轴心的资产预处理脚本
/// </summary>
public class Center_Mesh_Pivot_Model_PostProcessing : AssetPostprocessor
{
    //回调，导入完模型之后通知
    void OnPostprocessModel(GameObject g)
    {
        //如果开启了一律不，则直接返回
        if(AssetDatabase.LoadAssetAtPath<Center_Mesh_Seeting>("Assets/Editor/Center_Mesh_Seeting.asset").isAllNot) return;
        //弹出一个对话
        if(EditorUtility.DisplayDialog("提示", "模型导入完毕，要把轴心居中吗？", "确定" ,"取消"))
        {
            //若确定，则进入轴心居中的逻辑

            //获取所有的MeshFilter
            MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();

            //遍历所有的MeshFilter
            foreach (MeshFilter meshFilter in meshFilters)
            {
                //获取Mesh，申明中间变量
                Mesh mesh = meshFilter.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                Vector3 center = Vector3.zero;
                Vector3 center2 = Vector3.zero;

                //计算中心点
                foreach (Vector3 v in vertices){
                    center += v;
                }
                center /= vertices.Length;
                
                //把Mesh的中心移动到原点
                for (int i = 0; i < vertices.Length; i++){
                    vertices[i] -= center;
                }
                mesh.vertices = vertices;

                // 把mesh的Transform移动到Center
                meshFilter.transform.position += center * meshFilter.transform.lossyScale.x;

                //如果有子物体，让所有子物体跟着一起运动
                foreach (Transform child in meshFilter.transform){
                    child.position -= center * meshFilter.transform.lossyScale.x;
                }

                //重计算包围盒，保存更改
                mesh.RecalculateBounds();
            }
        }
        else{
            //若取消，则进入一律不重置的逻辑
            if(EditorUtility.DisplayDialog("提示", "是否一律不重置轴心？", "一律不重置", "取消")){
                //若一律不重置，则把一律不重置的标志位设置为true
                Center_Mesh_Seeting center_Mesh_Seeting = 
                    AssetDatabase.LoadAssetAtPath<Center_Mesh_Seeting>("Assets/Editor/Center_Mesh_Seeting.asset");
                center_Mesh_Seeting.isAllNot = true;
            }
        }
        
    }
}
