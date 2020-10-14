﻿using UnityEngine;


[System.Serializable]
public class BehaviorAction {
  public string name;
  public BehaviorActonStatus status = BehaviorActonStatus.NotStarted;
  public BehaviorActionType type;
  public Vector3 pos;
  public string str; // room | text
  public int val1; // actor, item, sound, flag
  public int val2; // expr, val, item

  public override string ToString() {
    string name = "FIXME";

    switch (type) {
      case BehaviorActionType.Teleport: return "Teleport " + pos;
      case BehaviorActionType.MoveToSpecificSpot: return "Move to " + pos;
      case BehaviorActionType.MoveToActor: return "Move to " + (Chars)val1;
      case BehaviorActionType.Speak: return "Say " + str + ": " + (Chars)val1;
      case BehaviorActionType.Ask: return "Ask " + str + ": " + (Chars)val1;
      case BehaviorActionType.Expression: return "Epr " + (Expression)val2 + " " + (Chars)val1;
      case BehaviorActionType.EnableDisable: return (ItemEnum)val1 + ((FlagValue)val2 == FlagValue.Yes ? " Enabled" : " Disabled");
      case BehaviorActionType.OpenClose: return (ItemEnum)val1 + ((FlagValue)val2 == FlagValue.Yes ? " Open" : " Close");
      case BehaviorActionType.LockUnlock: return (ItemEnum)val1 + ((FlagValue)val2 == FlagValue.Yes ? " Lock" : " Unlock");
      case BehaviorActionType.Sound: return "Play " + (Audios)val1;
      case BehaviorActionType.AnimActor: return (Chars)val1 + " anim " + str;
      case BehaviorActionType.AnimItem: return (ItemEnum)val1 + " anim " + str;
      case BehaviorActionType.SetFlag: return (GameFlag)val1 + " " + (FlagValue)val2;
      case BehaviorActionType.BlockActor: return (Chars)val1 + ((FlagValue)val2 == FlagValue.Yes ? " blocked" : " free");
    }
    return name;
  }

  /*
   * 

                            | v3  | str  | int   | str  | int  | int | int  | int   | int
    Teleport                | pos | room |
    Move to specific spot   | pos | room |
    Move to actor           |     | room | actor |
    Speak                   |     |      | actor | text |
    Ask (starts dialogue?)  |     |      | actor | text |
    Expression              |     |      | actor |      | expr |
    Enable/Disable          |     |      |       |      |      | Item | val |
    Open/Close              |     |      |       |      |      | Item | val |
    Lock/Unlock             |     |      |       |      |      | Item | val |
    Sound                   |     |      |       |      |      |      |     | sound |
    AnimActor               |     |      | actor | anim |
    AnimItem                |     |      |       | anim |      | Item |     |
    Set flag                |     |      |       |      |      |      | val |       | flag |
    Give                    |     |      | actor |      |      | Item |     |       |      |
    BlockActor              |     |      | actor |      |      |      | val |       |      |
   
   */

}

public enum BehaviorActionType {
  Teleport,
  MoveToSpecificSpot,
  MoveToActor,
  Speak,
  Ask,
  Expression,
  EnableDisable,
  OpenClose,
  LockUnlock,
  Sound,
  AnimActor,
  AnimItem,
  SetFlag,
  BlockActor
}

public enum BehaviorActonStatus {
  NotStarted,
  Running,
  WaitingToCompleteAsync,
  Completed
}
