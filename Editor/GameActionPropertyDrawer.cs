﻿using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameAction))]
public class GameActionPropertyDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    EditorGUI.BeginProperty(position, label, property);

    int indent = EditorGUI.indentLevel;
    float labw = EditorGUIUtility.labelWidth;
    EditorGUI.indentLevel = 1;


    SerializedProperty type = property.FindPropertyRelative("type");
    SerializedProperty Repeatable = property.FindPropertyRelative("Repeatable");
    SerializedProperty actor = property.FindPropertyRelative("actor");
    SerializedProperty pos = property.FindPropertyRelative("pos");
    SerializedProperty strValue = property.FindPropertyRelative("strValue");
    SerializedProperty expression = property.FindPropertyRelative("expression");
    SerializedProperty sound = property.FindPropertyRelative("sound");
    SerializedProperty dir = property.FindPropertyRelative("dir");
    SerializedProperty delay = property.FindPropertyRelative("delay");
    SerializedProperty item = property.FindPropertyRelative("item");
    SerializedProperty action = property.FindPropertyRelative("action");
    SerializedProperty change = property.FindPropertyRelative("change");
    SerializedProperty when = property.FindPropertyRelative("when");



    string name = GameAction.CalculateName((ActionType)type.intValue, actor.enumDisplayNames[actor.intValue], (ItemEnum)item.intValue, strValue.stringValue, (Expression)expression.intValue, (Audios)sound.intValue, pos.vector2Value, (ChangeWay)change.intValue, (WhatItDoes)when.intValue);

    Rect rectAct = new Rect(position.x, position.y, 90, EditorGUIUtility.singleLineHeight);
    Rect rectType = new Rect(position.x + 90, position.y, position.width / 3 - 50, EditorGUIUtility.singleLineHeight);
    Rect rectName = new Rect(position.x + 90 + position.width / 3, position.y, position.width * 2 / 3 - 50, EditorGUIUtility.singleLineHeight);
    Rect rect1, rect2, rect3;

    EditorGUI.LabelField(rectAct, "Action", EditorStyles.boldLabel);
    type.intValue = EditorGUI.Popup(rectType, type.intValue, type.enumDisplayNames);
    EditorGUI.LabelField(rectName, name);

    // ID and Repeatable
    Rect rectWhen = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
    Rect rectRep = new Rect(position.x + position.width / 2, position.y + EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
    Rect rectDel = new Rect(position.x + 100 + position.width / 2, position.y + EditorGUIUtility.singleLineHeight, position.width / 2 - 100, EditorGUIUtility.singleLineHeight);
    EditorGUIUtility.labelWidth = 90;
    when.intValue = EditorGUI.Popup(rectWhen, "When", when.intValue, when.enumDisplayNames);
    Repeatable.boolValue = EditorGUI.Toggle(rectRep, "Repeatable", Repeatable.boolValue);
    delay.floatValue = EditorGUI.FloatField(rectDel, "Delay", delay.floatValue);

    switch ((ActionType)type.intValue) {
      case ActionType.ShowRoom:
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
        rect2 = new Rect(position.x + position.width / 2, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
        EditorGUIUtility.labelWidth = 80;
        strValue.stringValue = EditorGUI.TextField(rect1, "RoomID", strValue.stringValue);
        pos.vector2Value = EditorGUI.Vector2Field(rect2, "Position", pos.vector2Value);

        if (string.IsNullOrEmpty(strValue.stringValue) || pos.vector2Value == Vector2.zero) {
          GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
          style.normal.textColor = Color.red;
          Rect rectErr = new Rect(position.x + position.width / 4, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(rectErr, "INVALID!", style);
        }
        break;

      case ActionType.Teleport:
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
        rect2 = new Rect(position.x + position.width / 3, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
        rect3 = new Rect(position.x + 2 * position.width / 3, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
        EditorGUIUtility.labelWidth = 60;
        actor.intValue = EditorGUI.Popup(rect1, "Actor", actor.intValue, actor.enumDisplayNames);
        pos.vector2Value = EditorGUI.Vector2Field(rect2, "Position", pos.vector2Value);
        dir.intValue = EditorGUI.Popup(rect3, "Dir", dir.intValue, dir.enumDisplayNames);

        if (actor.intValue < 1 || pos.vector2Value == Vector2.zero) {
          GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
          style.normal.textColor = Color.red;
          Rect rectErr = new Rect(position.x + position.width / 4, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(rectErr, "INVALID!", style);
        }
        break;

      case ActionType.Speak:
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) {
          wordWrap = true,
          fixedHeight = EditorGUIUtility.singleLineHeight * 5
        };
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
        rect2 = new Rect(position.x + position.width / 2, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
        rect3 = new Rect(position.x, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width, 5 * EditorGUIUtility.singleLineHeight);
        EditorGUIUtility.labelWidth = 60;
        actor.intValue = EditorGUI.Popup(rect1, "Actor", actor.intValue, actor.enumDisplayNames);
        dir.intValue = EditorGUI.Popup(rect2, "Dir", dir.intValue, dir.enumDisplayNames);
        strValue.stringValue = EditorGUI.TextArea(rect3, strValue.stringValue, textAreaStyle);

        if (actor.intValue < 1 || string.IsNullOrEmpty(strValue.stringValue)) {
          GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
          style.normal.textColor = Color.red;
          Rect rectErr = new Rect(position.x + position.width / 4, position.y + 8 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(rectErr, "INVALID!", style);
        }
        break;

      case ActionType.Expression:
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
        rect2 = new Rect(position.x + position.width / 3, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
        rect3 = new Rect(position.x + 2 * position.width / 3, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
        EditorGUIUtility.labelWidth = 60;
        actor.intValue = EditorGUI.Popup(rect1, "Actor", actor.intValue, actor.enumDisplayNames);
        expression.intValue = EditorGUI.Popup(rect2, "Expr", expression.intValue, expression.enumDisplayNames);
        dir.intValue = EditorGUI.Popup(rect3, "Dir", dir.intValue, dir.enumDisplayNames);

        if (actor.intValue < 1 || delay.floatValue <= 0) {
          GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
          style.normal.textColor = Color.red;
          Rect rectErr = new Rect(position.x + position.width / 4, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(rectErr, "INVALID!", style);
        }
        break;

      case ActionType.Sound:
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
        rect2 = new Rect(position.x, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
        rect3 = new Rect(position.x + position.width / 2, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
        EditorGUIUtility.labelWidth = 60;
        sound.intValue = EditorGUI.Popup(rect1, "Sound", sound.intValue, sound.enumDisplayNames);
        actor.intValue = EditorGUI.Popup(rect2, "Actor", actor.intValue, actor.enumDisplayNames);
        if (actor.intValue != 0) dir.intValue = EditorGUI.Popup(rect3, "Dir", dir.intValue, dir.enumDisplayNames);

        if (delay.floatValue <= 0 || (actor.intValue != 0 && dir.intValue > 3)) {
          GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
          style.normal.textColor = Color.red;
          Rect rectErr = new Rect(position.x + position.width / 4, position.y + 4 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(rectErr, "INVALID!", style);
        }
        break;

      case ActionType.Enable:
      case ActionType.Open:
      case ActionType.Lock:
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width * 3 / 4, EditorGUIUtility.singleLineHeight);
        rect2 = new Rect(position.x + position.width * 3 / 4, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width / 4, EditorGUIUtility.singleLineHeight);
        item.intValue = EditorGUI.Popup(rect1, "Item", item.intValue, item.enumNames);
        change.intValue = EditorGUI.Popup(rect2, "Change", change.intValue, change.enumDisplayNames);

        if (item.intValue == 0) {
          GUIStyle style = new GUIStyle();
          style.normal.textColor = Color.red;
          Rect rectErr = new Rect(position.x + position.width / 4, position.y + 4 * EditorGUIUtility.singleLineHeight, position.width / 2, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(rectErr, "INVALID!", style);
        }
        break;


      case ActionType.SetSequence:
        rect1 = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
        action.intValue = EditorGUI.Popup(rect1, "ID", action.intValue, action.enumDisplayNames);
        break;
    }

    EditorGUI.indentLevel = indent;
    EditorGUIUtility.labelWidth = labw;
    EditorGUI.EndProperty();
  }

  //This will need to be adjusted based on what you are displaying
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    SerializedProperty type = property.FindPropertyRelative("type");
    switch ((ActionType)type.intValue) {
      case ActionType.ShowRoom: return EditorGUIUtility.singleLineHeight * 4;
      case ActionType.Teleport: return EditorGUIUtility.singleLineHeight * 4;
      case ActionType.Speak: return EditorGUIUtility.singleLineHeight * 9;
      case ActionType.Expression: return EditorGUIUtility.singleLineHeight * 5 + 5;
      case ActionType.Sound: return EditorGUIUtility.singleLineHeight * 4 + 5;
      case ActionType.Enable: return EditorGUIUtility.singleLineHeight * 4 + 5;
      case ActionType.Open: return EditorGUIUtility.singleLineHeight * 4 + 5;
      case ActionType.Lock: return EditorGUIUtility.singleLineHeight * 4 + 5;
    }
    return EditorGUIUtility.singleLineHeight * 5;
  }
}
