// Written By Roman
// email: 1325980292@qq.com
// 参考： https://www.cnblogs.com/chenliyang/p/6558471.html
// 参考： https://blog.csdn.net/weixin_42129718/article/details/120084238

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class FBXAnimationClipper : EditorWindow
{
    [MenuItem("Roman/FBX动画裁剪工具")]
    public static void ShowWindow() => EditorWindow.GetWindow(typeof(FBXAnimationClipper));
    
    static public Object importObj;
    static public TextAsset txtObj;

    private bool ignoreLastFrameWraning = false;

    //当窗口关闭，执行Clear
    void OnDestroy() => Clear();


    void OnGUI(){
        //引入文件
        GUILayout.Label("1.请先拖入要处理的FBX", EditorStyles.boldLabel);
        importObj = (Object)EditorGUILayout.ObjectField("拖入要处理的fbx", importObj, typeof(Object), false);
        if(!AssetDatabase.GetAssetPath(importObj).ToLower().EndsWith(".fbx") && importObj != null){
            //如果有文件，但不是FBX
            EditorUtility.DisplayDialog("提示", "请放入FBX!", "确定");
            importObj = null;
        }

        GUILayout.Space(50);

        GUILayout.Label("2.请拖入切割信息TXT", EditorStyles.boldLabel);
        txtObj = (TextAsset)EditorGUILayout.ObjectField("拖入切割信息TXT", txtObj, typeof(TextAsset), false);
        if(!AssetDatabase.GetAssetPath(txtObj).ToLower().EndsWith(".txt") && txtObj != null){
            EditorUtility.DisplayDialog("提示", "请放入TXT!", "确定");
            txtObj = null;
        }

        //首先分析TXT文件
        if(txtObj != null)
        {
            Regex regexString = new Regex(
                " *(?<firstFrame>[0-9]+) *[-|~| *] *(?<lastFrame>[0-9]+) *(?<loop>(loop|noloop| )) *(?<name>[^\r^\n]*[^\r^\n^ ])",  
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );  
            string text = txtObj.text;
            Match match = regexString.Match(text);

            //通过lable显示所有match信息
            int count = 0;
            int txtMAXFrame = 0;
            GUILayout.Label("TXT内信息如下：", EditorStyles.boldLabel);
            while (match.Success){
                //显示match信息
                GUILayout.Label(match.Value);

                //获取最大帧数
                int frame = int.Parse(match.Groups["lastFrame"].Value);
                if(frame > txtMAXFrame) txtMAXFrame = frame;

                //下一组
                count++;
                match = match.NextMatch();
            }
            
            //对比FBX内动画的最大帧数
            if(importObj != null){
                ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(importObj)) as ModelImporter;
                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                int fbxMAXFrame = 0;
                foreach(ModelImporterClipAnimation clip in clips){
                    if(clip.lastFrame > fbxMAXFrame) fbxMAXFrame = (int)clip.lastFrame;
                }
                GUILayout.Label("FBX内最大帧数" + fbxMAXFrame);
                if(txtMAXFrame > fbxMAXFrame && !ignoreLastFrameWraning){
                    if(EditorUtility.DisplayDialog("提示", "FBX内最大帧数小于TXT内最大帧数，请确认是否正确。若无误，请忽略本提示。但还是建议您不要让切割信息的末尾帧大于原Clip的末尾帧，多出来的部分将会保持不动", "确定"))
                        ignoreLastFrameWraning = true;
                }
            }

            GUILayout.Label("共有" + count + "段动画信息，请确认是否正确");
        }

        GUILayout.Space(50);

        //监听确认切割按钮
        if(GUILayout.Button("确认切割", GUILayout.Height(50)))
        {
            //首先检查是否放入了FBX，若无，弹窗提示
            if(importObj == null){
                EditorUtility.DisplayDialog("提示","请先拖入FBX！","确认");
            }
            //检查是否放入了TXT
            else if(txtObj == null)
                EditorUtility.DisplayDialog("提示","请先拖入TXT！","确认");
            else{
                //再进行一步本身动画段数检查
                ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(importObj)) as ModelImporter;
                if(importer.defaultClipAnimations.Length > 1){
                    if(EditorUtility.DisplayDialog("提示", "该FBX本身就含有多段动画，本次切割不生效！", "确定"))
                        return;
                }

                //检查完毕，开始分段写入到导入设置中
                Regex regexString = new Regex(
                    " *(?<firstFrame>[0-9]+) *[-|~| *] *(?<lastFrame>[0-9]+) *(?<loop>(loop|noloop| )) *(?<name>[^\r^\n]*[^\r^\n^ ])",  
                    RegexOptions.Compiled | RegexOptions.ExplicitCapture
                );  
                string text = txtObj.text;
                Match match = regexString.Match(text);
                List<ModelImporterClipAnimation> newAnimationClipList;
                ReCutTheAnimation(match,out newAnimationClipList);
                //写完至本地，把它替换到FBX中去
                importer.clipAnimations = newAnimationClipList.ToArray();
                //数据清理
                importer.SaveAndReimport();
                //
                EditorUtility.DisplayDialog("提示", "已经按照TXT的内容切好了！请到FBX目录下确认！"  , "确定");
                //自动选择到FBX
                Selection.activeObject = importObj; 
                //清理
                //Clear();
            }   
        }

        //监听撤销按钮
        if(GUILayout.Button("撤销切割", GUILayout.Height(50))){
            if(importObj == null){
                EditorUtility.DisplayDialog("提示","请先拖入FBX！","确认");
                return;
            }
            ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(importObj)) as ModelImporter;
            importer.clipAnimations = importer.defaultClipAnimations;
            //数据更新
            importer.SaveAndReimport();
            //
            EditorUtility.DisplayDialog("提示", "已经恢复FBX到导入时的动画切割" , "确定");
            //自动选择到FBX
            Selection.activeObject = importObj; 
            //数据清理
            //Clear();
        }      
    }

    void ReCutTheAnimation(Match match, out List<ModelImporterClipAnimation> newAnimationClipList){
        newAnimationClipList = new List<ModelImporterClipAnimation>();

        while(match.Success){
            ModelImporterClipAnimation clip = new ModelImporterClipAnimation();  

            if (match.Groups["firstFrame"].Success)  
                clip.firstFrame = System.Convert.ToInt32(match.Groups["firstFrame"].Value, 10);               
            if (match.Groups["lastFrame"].Success) 
                clip.lastFrame = System.Convert.ToInt32(match.Groups["lastFrame"].Value, 10);  
            if (match.Groups["loop"].Success)
                clip.loop = match.Groups["loop"].Value == "loop";    
            if (match.Groups["name"].Success)
                clip.name = match.Groups["name"].Value;   
  
            newAnimationClipList.Add(clip);

            match = match.NextMatch();
        }
    }

    void Clear(){
        importObj = null;
        txtObj = null;
    }

}
