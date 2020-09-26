﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor {
  SerializedProperty ID, Name;
  SerializedProperty minL, maxR;
  SerializedProperty minY, maxY, scalePerc;
  SerializedProperty CameraGround;


  void OnEnable() {
    ID = serializedObject.FindProperty("ID");
    Name = serializedObject.FindProperty("RoomName");
    minL = serializedObject.FindProperty("minL");
    maxR = serializedObject.FindProperty("maxR");
    minY = serializedObject.FindProperty("minY");
    maxY = serializedObject.FindProperty("maxY");
    scalePerc = serializedObject.FindProperty("scalePerc");
    CameraGround = serializedObject.FindProperty("CameraGround");
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();
    float oldw = EditorGUIUtility.labelWidth;

    // ID and Name
    EditorGUIUtility.labelWidth = 30;
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(ID);
    EditorGUIUtility.labelWidth = 80;
    EditorGUILayout.PropertyField(Name);
    EditorGUILayout.EndHorizontal();


    EditorGUIUtility.labelWidth = 70;
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(minL, new GUIContent("Min Left"));
    EditorGUILayout.PropertyField(maxR, new GUIContent("Max Right"));
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.PropertyField(minY, new GUIContent("Min Y"));
    EditorGUILayout.PropertyField(maxY, new GUIContent("Max Y"));
    EditorGUIUtility.labelWidth = 90;
    EditorGUILayout.PropertyField(scalePerc, new GUIContent("Scale Perc"));
    EditorGUILayout.EndHorizontal();

    EditorGUIUtility.labelWidth = 100;
    EditorGUILayout.PropertyField(CameraGround, new GUIContent("Camera Ground"));

    EditorGUILayout.Space();
    if (GUILayout.Button("Move camera here", GUILayout.Width(160))) {
      Vector3 pos = new Vector3((minL.floatValue + maxR.floatValue) / 2, CameraGround.floatValue, -10);
      Camera.main.transform.position = pos;
      SceneView.lastActiveSceneView.pivot = pos;
      SceneView.lastActiveSceneView.Repaint();
    }


  serializedObject.ApplyModifiedProperties();
    EditorGUIUtility.labelWidth = oldw;
  }
}


