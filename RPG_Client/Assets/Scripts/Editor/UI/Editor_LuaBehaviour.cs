﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

[CustomEditor(typeof(LuaBehaviour), true)]
public class Editor_LuaBehaviour : Editor
{
    public static string path = UITools.GetLuaPathInEditor()+"/";
    public static string templetPath = path + "UI/Templet.lua";

   // public static string tableName;

    public SerializedProperty tableName;
    public SerializedProperty luaFileName;
    public SerializedProperty domain;
    public LuaBehaviour lb;
    public SerializedProperty varList;
    public string rename = "";
    //public bool isEdit;
    void OnEnable()
    {
        tableName = serializedObject.FindProperty("tableName");
        luaFileName = serializedObject.FindProperty("luaFilename");
        domain = serializedObject.FindProperty("domain");
        varList = serializedObject.FindProperty("varList");
    }
    public override void OnInspectorGUI()
    {
        lb = (LuaBehaviour)target;

        if (target is LuaWithNoFile)
        {
            EditorGUILayout.PropertyField(domain, new GUIContent("Domain:"));
            lb.domain = domain.stringValue;
            return;
        }

        string relativeName = GetFilename(lb.gameObject , lb.tableName);
        string fullName = path + relativeName;

        if (!File.Exists(fullName))
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(tableName, new GUIContent("TableName:"));
            lb.tableName = tableName.stringValue;
            if (GUILayout.Button("Create File"))
            {
                CreateFile(fullName);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            lb.luaFilename = relativeName;
            luaFileName.stringValue = relativeName;
            EditorGUILayout.PropertyField(luaFileName, new GUIContent("LuaFilename:"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Rename:"));
            rename = EditorGUILayout.TextField(rename);
            if (GUILayout.Button("RenameFile"))
            {
                string oldName = fullName;
                string oldTable = lb.tableName;
                string newTable = rename;
                string newName = path + GetFilename(lb.gameObject , rename);
                if (renameFile(oldName, newName, oldTable, newTable))
                {
                    lb.tableName = tableName.stringValue = rename;
                    rename = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Domain:"));
        lb.domain = EditorGUILayout.TextField(lb.domain);

        if (GUILayout.Button("Commpile"))
        {
            UITools.Compile(fullName);
        }
        EditorGUILayout.EndHorizontal();

        if (File.Exists(fullName) && GUILayout.Button("OpenFile"))
        {
            OpenFile(fullName);
        }
        base.DrawDefaultInspector();
    }




    public bool renameFile(string oldName , string newName ,string oldTable ,string newTable)
    {
        if (!UITools.isValidString(newTable))
        {
            return false;
        }
        if (File.Exists(newName))
        {
            Debug.LogError("文件名已存在");
            return false;
        }
        if (!File.Exists(oldName))
        {
            Debug.LogError("文件不存在");
            return false;
        }
        string content = File.ReadAllText(oldName);
        content = content.Replace(oldTable, newTable);
        File.WriteAllText(newName, content, Encoding.UTF8);
        File.Delete(oldName);
        AssetDatabase.Refresh();
        return true;
    }


    public void OpenFile(string fullName)
    {
        System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
        Info.FileName = "sublime";
        Info.Arguments = fullName;
        System.Diagnostics.Process.Start(Info);
    }


    public void CreateFile(string filename) 
    {
        if (File.Exists(filename))
        {
            return;
        }
        string p = filename.Substring(0, filename.LastIndexOf('/'));
        if (!Directory.Exists(p))
        {
            Directory.CreateDirectory(p);
        }
        //复制模板的内容
        string content = File.ReadAllText(templetPath);
        content = content.Replace("tableName", lb.tableName);
        File.WriteAllText(filename, content, Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    public string GetFilename(GameObject go , string luaName)
    {
        string filename = "UI";
        List<string> ps = new List<string>();
        Transform p = go.transform.parent;
        while (p != null && p.transform.parent != p)
        {
            if (p.name != "Camera")
            {
                ps.Add(p.name);
                p = p.parent;
            }
            else
            {
                break;
            }
        }
        for (int i = ps.Count - 1; i >= 0; i--)
        {
            filename = filename + "/" + ps[i];
        }
        return (filename + "/" + luaName + ".lua").Replace(" ", "");
    }
}