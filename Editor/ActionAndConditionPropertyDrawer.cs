﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ActionAndCondition))]
public class ActionAndConditionDrawer : PropertyDrawer {

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    EditorGUI.BeginProperty(position, label, property);
    int indent = EditorGUI.indentLevel;
    float labw = EditorGUIUtility.labelWidth;
    EditorGUI.indentLevel = 1;

    List<ActionAndCondition> actions;
    Item it = property.serializedObject.targetObject as Item;
    if (it != null) actions = it.actions;
    else {
      Triggerer tr = property.serializedObject.targetObject as Triggerer;
      if (tr != null) actions = tr.actions;
      else {
        Room rm = property.serializedObject.targetObject as Room;
        if (rm != null) actions = rm.actions;
        else return;
      }
    }

    int sbo = property.propertyPath.LastIndexOf('[') + 1;
    int sbc = property.propertyPath.LastIndexOf(']');
    string indexpath = property.propertyPath.Substring(sbo, sbc - sbo);
    int.TryParse(indexpath, out int index);
    string name;
    if (index < 0 || index >= actions.Count) name = "<empty>";
    else name = index + ") " + actions[index].Condition.ToString() + " -> " + (actions[index].NumActions == 0 ? "<none>" : actions[index].Actions[0].ToString());

    float lh = EditorGUIUtility.singleLineHeight;

    Rect titleR = new Rect(position.x, position.y, position.width, lh);
    EditorGUI.LabelField(titleR, "A&C: " + name, EditorStyles.boldLabel);
    property.isExpanded = EditorGUI.Foldout(titleR, property.isExpanded, new GUIContent(""));

    if (property.isExpanded) {
      Rect cRect = new Rect(position.x + 30, position.y + 1 * lh, position.width - 30, 2 * lh);
      SerializedProperty cond = property.FindPropertyRelative("Condition");
      EditorGUI.PropertyField(cRect, cond);
      int extra = 0;
      if (cond.FindPropertyRelative("type").intValue != 0) {
        extra = 2;
        Rect c2Rect = new Rect(position.x + 30, position.y + 3 * lh, position.width - 30, 2 * lh);
        EditorGUI.PropertyField(c2Rect, property.FindPropertyRelative("Condition2"));
      }

      Rect nRect = new Rect(position.x + 30, position.y + (3 + extra) * lh, position.width / 2 - 30, 1 * lh);
      Rect bRect = new Rect(position.x + position.width / 2 + 30, position.y + (3 + extra) * lh, position.width / 2 - 30, 1 * lh);
      SerializedProperty num = property.FindPropertyRelative("NumActions");
      SerializedProperty block = property.FindPropertyRelative("Blocking");
      EditorGUI.PropertyField(nRect, num);
      EditorGUI.PropertyField(bRect, block);
      if (num.intValue > 0) {
        SerializedProperty acts = property.FindPropertyRelative("Actions");
        while (num.intValue < acts.arraySize) {
          acts.DeleteArrayElementAtIndex(acts.arraySize - 1);
        }
        while (num.intValue > acts.arraySize) {
          acts.InsertArrayElementAtIndex(acts.arraySize);
        }
        for (int i = 0; i < num.intValue; i++) {
          Rect asRect = new Rect(position.x + 30, position.y + (4 + i * 2 + extra) * lh, position.width - 30, 2 * lh);
          SerializedProperty sa = acts.GetArrayElementAtIndex(i);
          EditorGUI.PropertyField(asRect, sa);
        }
      }
    }


    EditorGUI.indentLevel = indent;
    EditorGUIUtility.labelWidth = labw;
    EditorGUI.EndProperty();
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    float h = base.GetPropertyHeight(property, label);
    int extra = property.FindPropertyRelative("Condition").FindPropertyRelative("type").intValue != 0 ? 2 : 0;
    if (property.isExpanded) {
      SerializedProperty num = property.FindPropertyRelative("NumActions");
      return h + (3 + num.intValue * 2 + extra) * EditorGUIUtility.singleLineHeight;
    }
    return h;
  }
}