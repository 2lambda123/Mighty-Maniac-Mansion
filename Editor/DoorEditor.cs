﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Door))]
public class DoorEditor : Editor {

  SerializedProperty itemEnum, Name;
  SerializedProperty whatItDoesL, whatItDoesR;
  SerializedProperty Usable, UsableWith;
  SerializedProperty openImage, closeImage, lockImage;
  SerializedProperty overColor, normalColor;
  SerializedProperty HotSpot, dir, arrivaldir;
  SerializedProperty src, dst, correspondingDoor;
  SerializedProperty camerapos, transition;
  SerializedProperty actions;


  void OnEnable() {
    itemEnum = serializedObject.FindProperty("Item");
    Name = serializedObject.FindProperty("Name");
    whatItDoesL = serializedObject.FindProperty("whatItDoesL");
    whatItDoesR = serializedObject.FindProperty("whatItDoesR");
    Usable = serializedObject.FindProperty("Usable");
    UsableWith = serializedObject.FindProperty("UsableWith");
    openImage = serializedObject.FindProperty("openImage");
    closeImage = serializedObject.FindProperty("closeImage");
    lockImage = serializedObject.FindProperty("lockImage");
    overColor = serializedObject.FindProperty("overColor");
    normalColor = serializedObject.FindProperty("normalColor");
    HotSpot = serializedObject.FindProperty("HotSpot");
    dir = serializedObject.FindProperty("dir");
    arrivaldir = serializedObject.FindProperty("arrivalDirection");
    src = serializedObject.FindProperty("src");
    dst = serializedObject.FindProperty("dst");
    camerapos = serializedObject.FindProperty("camerapos");
    correspondingDoor = serializedObject.FindProperty("correspondingDoor");
    transition = serializedObject.FindProperty("transition");
    actions = serializedObject.FindProperty("actions");
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();
    float oldw = EditorGUIUtility.labelWidth;
    EditorGUIUtility.labelWidth = 40;

    // ID and Name
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(itemEnum);
    EditorGUILayout.PropertyField(Name);
    EditorGUILayout.EndHorizontal();

    EditorGUIUtility.labelWidth = 50;

    // What does
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(whatItDoesL, new GUIContent("Click L"));
    EditorGUILayout.PropertyField(whatItDoesR, new GUIContent("Click R"));
    EditorGUILayout.EndHorizontal();

    // Usable UsableWith
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(Usable, new GUIContent("Usable"));
    EditorGUILayout.PropertyField(UsableWith, new GUIContent("  with"));
    EditorGUILayout.EndHorizontal();

    // Images
    EditorGUILayout.BeginHorizontal();
    EditorGUIUtility.labelWidth = 35;
    EditorGUILayout.PropertyField(openImage, new GUIContent("Open"));
    EditorGUILayout.PropertyField(closeImage, new GUIContent("Close"));
    EditorGUILayout.PropertyField(lockImage, new GUIContent("Lock"));
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.EndHorizontal();

    // Over and Normal colors
    EditorGUILayout.BeginHorizontal();
    EditorGUIUtility.labelWidth = 50;
    EditorGUILayout.PropertyField(overColor, new GUIContent("Over"));
    EditorGUILayout.PropertyField(normalColor, new GUIContent("Normal"));
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.EndHorizontal();

    // HotSpot
    EditorGUILayout.BeginHorizontal();
    EditorGUIUtility.labelWidth = 50;
    EditorGUILayout.PropertyField(HotSpot, new GUIContent("HotSpot"));
    EditorGUIUtility.labelWidth = 20;
    EditorGUILayout.PropertyField(dir, new GUIContent("Dir"), GUILayout.Width(70));
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.PropertyField(arrivaldir, new GUIContent("ToDir"), GUILayout.Width(70));
    EditorGUILayout.Space(30, false);
    if (GUILayout.Button("Set", GUILayout.Width(40))) {
      Item door = target as Door;
      if (door.transform.childCount == 0) {
        Debug.LogError("Missing spawn point for " + door.name);
        return;
      }
      Transform spawn = door.transform.GetChild(0);
      Debug.Log(spawn.name + " is at " + spawn.transform.position);
      door.HotSpot = spawn.transform.position;
    }
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.EndHorizontal();

    // Rooms
    EditorGUILayout.BeginHorizontal();
    EditorGUIUtility.labelWidth = 50;
    EditorGUILayout.PropertyField(src, new GUIContent("Src Room"));
    EditorGUILayout.PropertyField(dst, new GUIContent("Dst Room"));
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUIUtility.labelWidth = 50;
    EditorGUILayout.PropertyField(correspondingDoor, new GUIContent("Door"));
    EditorGUILayout.PropertyField(camerapos, new GUIContent("Cam"));
    EditorGUILayout.PropertyField(transition, new GUIContent("Trans"));
    EditorGUIUtility.labelWidth = 40;
    EditorGUILayout.EndHorizontal();


    EditorGUILayout.PropertyField(actions);

    serializedObject.ApplyModifiedProperties();
    EditorGUIUtility.labelWidth = oldw;
  }
}

