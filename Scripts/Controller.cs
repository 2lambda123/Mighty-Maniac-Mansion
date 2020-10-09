﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using SimpleJSON;

public class Controller : MonoBehaviour {
  Camera cam;
  public AllObjects allObjects;
  public LayerMask pathLayer;
  public UnityEngine.UI.Image BlackFade;
  public Transform PickedItems;
  public Material SceneSelectionPoint;
  public AudioSource MusicPlayer;


  public TextMeshProUGUI DbgMsg;
  public static void Dbg(string txt) {
    GD.c.DbgMsg.text = txt;
  }


  #region *********************** Mouse and Interaction *********************** Mouse and Interaction *********************** Mouse and Interaction ***********************
  void Update() {
    if (Options.IsActive()) return;
    if (GD.status == GameStatus.StartGame) {
      StartGame();
      return;
    }
    if (GD.status != GameStatus.NormalGamePlay && GD.status != GameStatus.Cutscene) return;


    cursorTime += Time.deltaTime;
    HandleCursor();


    #region Handling of text messages
    if (textMsgTime > 0) {
      textMsgTime -= Time.deltaTime;
      switch (txtMsgMode) {
        case TextMsgMode.Appearing:
          TextMsgRT.sizeDelta = new Vector2(textSizeX * 4 * (.25f - textMsgTime), 50);
          TextBackRT.sizeDelta = TextMsgRT.sizeDelta;
          break;
        case TextMsgMode.Disappearing:
          TextMsgRT.sizeDelta = new Vector2(textSizeX * 4 * (textMsgTime), 50);
          TextBackRT.sizeDelta = TextMsgRT.sizeDelta;
          break;
      }
      if (textMsgTime <= 0) {
        switch (txtMsgMode) {
          case TextMsgMode.Appearing:
            txtMsgMode = TextMsgMode.Visible;
            textMsgTime = 3 * textSpeed;
            break;
          case TextMsgMode.Disappearing:
            txtMsgMode = TextMsgMode.None;
            TextMsg.text = "";
            break;
          case TextMsgMode.Visible:
            txtMsgMode = TextMsgMode.Disappearing;
            textMsgTime = .25f;
            break;
          case TextMsgMode.None:
            break;
        }
      }
    }
    #endregion

    #region Sequences and actions
    if (currentCutscene != null) { // Do we have a sequence?
      if (currentAction == null) { // Do we have the action?
        currentAction = currentCutscene.GetNextAction();
        if (currentAction == null) {
          currentCutscene = null;
          forcedCursor = CursorTypes.None;
          if (actionsToPlay.Count > 0) currentAction = actionsToPlay.GetFirst();
        }
      }
    }
    else if (actionsToPlay.Count > 0) currentAction = actionsToPlay.GetFirst();

    if (currentAction != null) {
      PlayCurrentAction();
      return;
    }
    #endregion

    #region Handle camera
    Vector2 cpos = cam.WorldToScreenPoint(currentActor.transform.position);
    if (cam.transform.position.x < currentRoom.minL) {
      Vector3 p = cam.transform.position;
      p.x = currentRoom.minL;
      cam.transform.position = p;
    }
    else if (cam.transform.position.x > currentRoom.maxR) {
      Vector3 p = cam.transform.position;
      p.x = currentRoom.maxR;
      cam.transform.position = p;
    }
    else if (cpos.x < .4f * Screen.width) {
      if (cam.transform.position.x > currentRoom.minL) {
        cam.transform.position -= cam.transform.right * Time.deltaTime * (.4f * Screen.width - cpos.x) / 10;
      }
    }
    else if (cpos.x > .6f * Screen.width) {
      if (cam.transform.position.x < currentRoom.maxR) {
        cam.transform.position += cam.transform.right * Time.deltaTime * (cpos.x - .6f * Screen.width) / 10;
      }
    }
    #endregion


    if (GD.status != GameStatus.NormalGamePlay) return;

    #region Mouse control
    bool lmb = Input.GetMouseButtonDown(0);
    bool rmb = Input.GetMouseButtonDown(1);

    if (overActor != null) {
      if (rmb && usedItem != null) {
        if (currentActor == overActor) return;
        receiverActor = overActor;
        usedItem.Give(currentActor, receiverActor);
        Inventory.SetActive(false);
        usedItem = null;
        oldCursor = null;
        forcedCursor = CursorTypes.None;
        return;
      }
      if (lmb) {
        // FIXME remove from the final build
        if (overActor != currentActor) {
          SelectActor(overActor);
          return;
        }
      }
    }


    if (Input.GetMouseButton(0) && currentActor.IsWalking()) {
      if (walkDelay > .25f) {
        RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), cam.transform.forward, 10000, pathLayer);
        if (hit.collider != null) {
          PathNode p = hit.collider.GetComponent<PathNode>();
          currentActor.WalkTo(hit.point, p);
          walkDelay = 0;
        }
      }
      else {
        walkDelay += Time.deltaTime;
      }
    }

    if (!lmb && !rmb) return;

    if (overInventoryItem != null) {
      if (usedItem == overInventoryItem) {
        if (lmb) { /* lmb - remove used */
          forcedCursor = CursorTypes.None;
          oldCursor = null;
          usedItem = null;
        }
        else { /* rmb - read */
          if (overInventoryItem.HasActions(When.Use)) {
            string res = overInventoryItem.PlayActions(currentActor, null, When.Use, null, out bool goodByDefault);
            if (string.IsNullOrEmpty(res))
              currentActor.Say(overInventoryItem.Description); // By default read what is written in the description of the object
            else
              currentActor.Say(res);
          }
          else {
            string msg = overInventoryItem.Description.Replace("%open", overInventoryItem.GetOpenStatus());
            currentActor.Say(msg);
          }
        }
      }
      else if (usedItem == null) {
        if (lmb) { /* lmb - set as used */
          usedItem = overInventoryItem;
          overInventoryItem = null;
          oldCursor = null;
          forcedCursor = CursorTypes.Item;
          Cursor.SetCursor(usedItem.cursorImage, new Vector2(usedItem.cursorImage.width / 2, usedItem.cursorImage.height / 2), CursorMode.Auto);
        }
        else { /* rmb - use immediately */
          string res = overInventoryItem.Use(currentActor);
          if (!string.IsNullOrEmpty(res)) currentActor.Say(res);
        }

      }
      else {
        if (lmb) { /* lmb - Swap */
          usedItem = overInventoryItem;
          overInventoryItem = null;
          oldCursor = null;
          forcedCursor = CursorTypes.Item;
          Cursor.SetCursor(usedItem.cursorImage, new Vector2(usedItem.cursorImage.width / 2, usedItem.cursorImage.height / 2), CursorMode.Auto);
        }
        else { /* rmb - Use together */
          // Can we use the two items together?
          string res = usedItem.UseTogether(currentActor, overInventoryItem);
          if (!string.IsNullOrEmpty(res)) currentActor.Say(res);
          UpdateInventory();
          forcedCursor = CursorTypes.None;
          oldCursor = null;
          usedItem = null;
          Inventory.SetActive(false);
          return;
        }
      }

    }
    else if (overItem != null) {
      if (usedItem == overItem) { // Not possible
      }
      else if (usedItem == null) {
        if ((lmb && overItem.whatItDoesL == WhatItDoes.Read) || (rmb && overItem.whatItDoesR == WhatItDoes.Read)) { /* read */
          WalkAndAction(currentActor, overItem,
            new System.Action<Actor, Item>((actor, item) => {
              actor.SetDirection(item.dir);
              if (item.HasActions(When.Use)) {
                string res = item.PlayActions(currentActor, null, When.Use, null, out bool goodByDefault);
                if (string.IsNullOrEmpty(res))
                  actor.Say(item.Description); // By default read what is written in the description of the object
                else
                  actor.Say(res);
              }
              else {
                string msg = item.Description.Replace("%open", item.GetOpenStatus());
                actor.Say(msg);
              }
            }));
        }

        else if ((lmb && overItem.whatItDoesL == WhatItDoes.Pick) || (rmb && overItem.whatItDoesR == WhatItDoes.Pick)) { /* pick */
          WalkAndAction(currentActor, overItem,
            new System.Action<Actor, Item>((actor, item) => {
              ShowName(currentActor + " got " + item.Name);
              actor.inventory.Add(item);
              item.transform.parent = PickedItems;
              item.gameObject.SetActive(false);
              item.owner = Chars.None;
              if (actor == actor1) item.owner = Chars.Actor1;
              else if (actor == actor2) item.owner = Chars.Actor2;
              else if (actor == actor3) item.owner = Chars.Actor3;
              item.PlayActions(currentActor, null, When.Pick, null, out bool goodByDefault);
              item = null;
              forcedCursor = CursorTypes.None;
              if (Inventory.activeSelf) ActivateInventory(currentActor);
            }));
          overItem = null;
        }

        else if ((lmb && overItem.whatItDoesL == WhatItDoes.Use) || (rmb && overItem.whatItDoesR == WhatItDoes.Use)) { /* use */
          WalkAndAction(currentActor, overItem,
            new System.Action<Actor, Item>((actor, item) => {
              actor.SetDirection(item.dir);
              string res = item.Use(currentActor);
              if (!string.IsNullOrEmpty(res)) actor.Say(res);
            }));
        }

        else if ((lmb && overItem.whatItDoesL == WhatItDoes.Walk) || (rmb && overItem.whatItDoesR == WhatItDoes.Walk)) { /* walk */
          Door d = overItem as Door;
          if (d == null)
            WalkAndAction(currentActor, overItem, null);
          else
            WalkAndAction(currentActor, overItem,
              new System.Action<Actor, Item>((actor, item) => {
                if (item.Usable == Tstatus.OpenableLocked || item.Usable == Tstatus.OpenableLockedAutolock) {
                  actor.Say("Is locked");
                  return;
                }
                else if (item.Usable == Tstatus.OpenableClosed || item.Usable == Tstatus.OpenableClosedAutolock) {
                  return;
                }
                StartCoroutine(ChangeRoom(actor, (item as Door)));
              }));
        }
      }
      else {
        if (lmb) { /* lmb Walk */
          WalkAndAction(currentActor, overItem, null);
        }
        else { /* rmb - Use together */
          WalkAndAction(currentActor, overItem,
            new System.Action<Actor, Item>((actor, item) => {
              // Can we use the two items together?
              string res = usedItem.UseTogether(currentActor, overItem);
              if (!string.IsNullOrEmpty(res)) currentActor.Say(res);
              UpdateInventory();
              forcedCursor = CursorTypes.None;
              oldCursor = null;
              usedItem = null;
              Inventory.SetActive(false);
              return;
            }));
        }
      }
    }
    else {
      if (lmb && !currentActor.IsWalking()) { /* lmb - walk */
        RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), cam.transform.forward, 10000, pathLayer);
        if (hit.collider != null) {
          PathNode p = hit.collider.GetComponent<PathNode>();
          currentActor.WalkTo(hit.point, p);
          walkDelay = 0;
        }
      }
      else { /* rmb - do nothing but unselect used items */
        forcedCursor = CursorTypes.None;
        oldCursor = null;
        usedItem = null;
      }
    }


    #endregion
  }

  private CursorTypes forcedCursor = CursorTypes.None;
  private Texture2D oldCursor = null;
  public Texture2D[] Cursors;
  float cursorTime = 0;

  void HandleCursor() {
    if (GD.status != GameStatus.NormalGamePlay) {
      if (oldCursor != Cursors[(int)CursorTypes.Wait]) {
        Cursor.SetCursor(Cursors[(int)CursorTypes.Wait], new Vector2(Cursors[(int)CursorTypes.Wait].width / 2, Cursors[(int)CursorTypes.Wait].height / 2), CursorMode.Auto);
        oldCursor = Cursors[(int)CursorTypes.Wait];
      }
      return;
    }

    if (forcedCursor == CursorTypes.Item) return;

    if (forcedCursor == CursorTypes.None) {
      if (0 <= cursorTime && cursorTime <= .5f) {
        if (oldCursor != Cursors[0]) {
          Cursor.SetCursor(Cursors[0], new Vector2(Cursors[0].width / 2, Cursors[0].height / 2), CursorMode.Auto);
          oldCursor = Cursors[0];
        }
      }
      else if (.5f < cursorTime && cursorTime <= .75f) {
        if (oldCursor != Cursors[1]) {
          Cursor.SetCursor(Cursors[1], new Vector2(Cursors[1].width / 2, Cursors[1].height / 2), CursorMode.Auto);
          oldCursor = Cursors[1];
        }
      }
      else if (.75f < cursorTime && cursorTime <= .9f) {
        if (oldCursor != Cursors[2]) {
          Cursor.SetCursor(Cursors[2], new Vector2(Cursors[2].width / 2, Cursors[2].height / 2), CursorMode.Auto);
          oldCursor = Cursors[2];
        }
      }
      else if (.9f < cursorTime && cursorTime <= 1.05f) {
        if (oldCursor != Cursors[1]) {
          Cursor.SetCursor(Cursors[1], new Vector2(Cursors[1].width / 2, Cursors[1].height / 2), CursorMode.Auto);
          oldCursor = Cursors[1];
        }
      }
      else {
        cursorTime = 0;
        if (oldCursor != Cursors[0]) {
          Cursor.SetCursor(Cursors[0], new Vector2(Cursors[0].width / 2, Cursors[0].height / 2), CursorMode.Auto);
          oldCursor = Cursors[0];
        }
      }
      return;
    }

    if (oldCursor != Cursors[(int)forcedCursor]) {
      Cursor.SetCursor(Cursors[(int)forcedCursor], new Vector2(Cursors[(int)forcedCursor].width / 2, Cursors[(int)forcedCursor].height / 2), CursorMode.Auto);
      oldCursor = Cursors[(int)forcedCursor];
    }
  }

  internal static CursorTypes GetCursor() {
    Cursor.SetCursor(GD.c.Cursors[0], new Vector2(GD.c.Cursors[0].width / 2, GD.c.Cursors[0].height / 2), CursorMode.Auto);
    return GD.c.forcedCursor;
  }


  internal static void SetCursor(CursorTypes cur) {
    GD.c.forcedCursor = cur;
  }


  internal static void HandleToolbarClicks(IPointerClickHandler handler) {
    if (GD.status != GameStatus.NormalGamePlay || Options.IsActive()) return;
    PortraitClickHandler h = (PortraitClickHandler)handler;
    if (h == GD.c.ActorPortrait1) {
      SelectActor(GD.c.actor1);
    }
    else if (h == GD.c.ActorPortrait2) {
      SelectActor(GD.c.actor2);
    }
    else if (h == GD.c.ActorPortrait3) {
      SelectActor(GD.c.actor3);
    }
    else if (h == GD.c.InventoryPortrait) {
      if (GD.c.Inventory.activeSelf) { // Show/Hide inventory of current actor
        GD.c.Inventory.SetActive(false);
        GD.c.InventoryPortrait.GetComponent<UnityEngine.UI.RawImage>().color = new Color32(0x6D, 0x7D, 0x7C, 0xff);
        return;
      }
      else
        GD.c.ActivateInventory(GD.c.currentActor);
    }
  }


  #endregion



  #region *********************** Initialization

  private void Awake() {
    GD.c = this;
    cam = Camera.main;
    LoadSequences();

    foreach (Room r in allObjects.roomsList) {
      r.gameObject.SetActive(false);
    }
    foreach (Actor a in allEnemies)
      if (a != null) a.gameObject.SetActive(false);
    foreach (Actor a in allActors)
      if (a != null) a.gameObject.SetActive(false);
    ActorsButtons.SetActive(false);
  }

  private void Start() {
    SceneSelectionPoint.color = new Color32(0, 0, 0, 0);
    currentRoom = allObjects.roomsList[0];
    currentActor = actor1;
    ActorPortrait1.GetComponent<UnityEngine.UI.RawImage>().color = selectedActor;

    Options.GetOptions();
    GD.ReadyToStart();
  }

  void StartGame() {
    foreach (Room r in allObjects.roomsList) {
      r.gameObject.SetActive(r == currentRoom);
    }
    GD.status = GameStatus.NormalGamePlay;
    foreach (Actor a in allActors) {
      if (a == null) continue;
      a.gameObject.SetActive(a.currentRoom == currentRoom);
    }
    foreach (Actor a in allEnemies) {
      if (a == null) continue;
      a.gameObject.SetActive(a.currentRoom == currentRoom);
    }
    SkyBackground.enabled = true;

    actor1 = GetActor(GD.actor1);
    ActorPortrait1.portrait.sprite = GetActorPortrait(GD.actor1);
    actor2 = GetActor(GD.actor2);
    ActorPortrait2.portrait.sprite = GetActorPortrait(GD.actor2);
    actor3 = GetActor(GD.actor3);
    ActorPortrait3.portrait.sprite = GetActorPortrait(GD.actor3);
    kidnappedActor = GetActor(GD.kidnapped);
    currentActor = actor1;

    ActorsButtons.SetActive(true);
    StartIntroCutscene();
  }

  void OnApplicationQuit() {
    SceneSelectionPoint.color = new Color32(255, 255, 255, 255);
  }
  #endregion


  #region *********************** Cutscenes and Actions *********************** Cutscenes and Actions *********************** Cutscenes and Actions ***********************
  Cutscene currentCutscene;
  ContextualizedAction currentAction;
  public List<Cutscene> cutscenes;
  readonly SList<ContextualizedAction> actionsToPlay = new SList<ContextualizedAction>(16);
  readonly HashSet<GameAction> allKnownActions = new HashSet<GameAction>();

  void LoadSequences() {
    string path = Application.dataPath + "/Actions/";

    try {
      foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
        //FIXME Debug.Log(file);
        string json = System.IO.File.ReadAllText(file);

        try {
          JSONNode j = JSON.Parse(json);

          Cutscene seq = new Cutscene(j["id"].Value, j["name"].Value);
          // FIXME conditions
          // Actions
          JSONNode.ValueEnumerator vals = j["actions"].Values;
          foreach (JSONNode val in vals) {
            GameAction a = new GameAction(val["type"].Value);
            if (a.type == ActionType.Teleport) {
              a.SetActor(val["actor"].Value);
              a.SetPos(val["pos"][0].AsFloat, val["pos"][1].AsFloat);
              a.SetDir(val["dir"].Value);
              a.SetValue(val["room"].Value);
            }
            else if (a.type == ActionType.Move) {
              a.SetActor(val["actor"].Value);
              a.SetPos(val["pos"][0].AsFloat, val["pos"][1].AsFloat);
              a.SetDir(val["dir"].Value);
              a.SetValue(val["room"].Value);
            }
            else if (a.type == ActionType.Speak) {
              a.SetActor(val["actor"].Value);
              a.SetDir(val["dir"].Value);
              a.SetValue(val["msg"].Value);
            }
            else if (a.type == ActionType.Expression) {
              a.SetActor(val["actor"].Value);
              a.SetDir(val["dir"].Value);
              a.SetValue(val["expr"].Value);
            }
            else if (a.type == ActionType.ShowRoom) {
              a.SetPos(val["pos"][0].AsFloat, val["pos"][1].AsFloat);
              a.SetValue(val["room"].Value);
            }
            else if (a.type == ActionType.Sound) {
              a.SetActor(val["actor"].Value);
              a.SetDir(val["dir"].Value);
              a.SetSound(val["snd"].Value);
            }

            else if (a.type == ActionType.Open) {
              a.SetActor(val["actor"].Value);
              a.SetValue(val["item"].Value);
              a.SetMode(val["mode"].AsBool);
            }
            else if (a.type == ActionType.Lock) {
              a.SetActor(val["actor"].Value);
              a.SetValue(val["item"].Value);
              a.SetMode(val["mode"].AsBool);
            }
            else if (a.type == ActionType.Enable) {
              a.SetValue(val["item"].Value);
              a.SetMode(val["mode"].AsBool);
            }

            else if (a.type == ActionType.FadeIn || a.type == ActionType.FadeOut) {
            }

            else Debug.LogError("Not handled action type: " + a.type);
            a.SetWait(val["wait"].AsFloat);

            if (!val["norepeat"])
              a.Repeatable = true;

            seq.actions.Add(a);
          }
          cutscenes.Add(seq);

        } catch (System.Exception e) {
          Debug.Log("ERROR (" + file + "): " + e.Message);
        }
      }
    } catch (System.Exception e) {
      Debug.Log("Main ERROR reading " + path + ": " + e.Message);
      // FIXME here we need a better message
    }

    currentCutscene = null;
  }

  void StartIntroCutscene() {
    currentCutscene = GetCutscene("intro");
    forcedCursor = CursorTypes.Wait;
    oldCursor = null;
    GD.status = GameStatus.NormalGamePlay;
  }

  Cutscene GetCutscene(string id) {
    string val = id.ToLowerInvariant();
    foreach (Cutscene s in cutscenes) {
      if (s.id.ToLowerInvariant() == val) {
        return s;
      }
    }
    Debug.LogError("Cutscene not found: \"" + id + "\"");
    return null;
  }

  public static void AddAction(GameAction a, Actor perf, Actor sec, Item item) {
    GD.c.actionsToPlay.Add(new ContextualizedAction { action = a, performer = perf, secondary = sec, item = item });
    GD.c.allKnownActions.Add(a);
  }

  public static void KnowAction(GameAction a) {
    GD.c.allKnownActions.Add(a);
  }

  void PlayCurrentAction() {
    if (currentAction.NotStarted()) {
      RunCurrentAction();
    }
    else if (currentAction.IsPlaying()) {
      currentAction.action.CheckTime(Time.deltaTime);
    }
    else if (currentAction.IsCompleted()) {
      if (currentAction.action.Repeatable) currentAction.action.Reset();
      if (currentCutscene != null) {
        currentAction = currentCutscene.GetNextAction();
      }
      else if (actionsToPlay.Count > 0) {
        currentAction = actionsToPlay.GetFirst();
      }
      else
        currentAction = null;

      if (currentAction == null) {
        GD.status = GameStatus.NormalGamePlay;
      }
    }
  }

  internal static Running ActionStatus(ActionEnum action) {
    foreach(GameAction a in GD.c.allKnownActions) {
      if (a.action == action) return a.running;
    }
    return Running.NotStarted;
  }

  void RunCurrentAction() {
    switch (currentAction.action.type) {
      case ActionType.Teleport: {
        Actor a = GetActor(currentAction.action.actor);
        Room aroom = allObjects.GetRoom(currentAction.action.strValue);
        if (aroom != null) {
          a.currentRoom = aroom;
          a.gameObject.SetActive(aroom == currentRoom);
        }
        a.transform.position = currentAction.action.pos;
        a.SetDirection(currentAction.action.dir);
        RaycastHit2D hit = Physics2D.Raycast(currentAction.action.pos, cam.transform.forward, 10000, pathLayer);
        if (hit.collider != null) {
          PathNode p = hit.collider.GetComponent<PathNode>();
          a.SetScaleAndPosition(currentAction.action.pos, p);
        }
        else {
          a.SetScaleAndPosition(currentAction.action.pos);
        }
        currentAction.Complete();
      }
      break;

      case ActionType.Speak: {
        Actor a = GetActor(currentAction.action.actor);
        if (a != null) {
          a.Say(currentAction.action.strValue, currentAction.action);
          a.SetDirection(currentAction.action.dir);
          currentAction.Play();
        }
        else
          currentAction.Complete();
      }
      break;

      case ActionType.Move: {
        RaycastHit2D hit = Physics2D.Raycast(currentAction.action.pos, cam.transform.forward, 10000, pathLayer);
        if (hit.collider != null) {
          PathNode p = hit.collider.GetComponent<PathNode>();
          GameAction copy = currentAction.action;
          currentAction.Play();
          currentAction.performer.WalkTo(currentAction.action.pos, p,
          new System.Action<Actor, Item>((actor, item) => {
            actor.SetDirection(copy.dir);
            copy.Complete(); 
          }));
        }
      }
      break;

      case ActionType.Expression: {
        Actor a = GetActor(currentAction.action.actor);
        a.SetDirection(currentAction.action.dir);
        a.SetExpression(Enums.GetExp(currentAction.action.strValue));
        currentAction.Play();
      }
      break;

      case ActionType.Open: {
        // Find the actual Item from all the known items, pick it by enum
        Actor a = GetActor(currentAction.action.actor);
        Item item = currentAction.item;
        if (item == null) {
          Debug.LogError("Item not defined for Open");
          currentAction.Complete();
          return;
        }
        item.ForceOpen(currentAction.action.change);
        currentAction.Complete();
      }
      break;

      case ActionType.Enable: {
        // Find the actual Item from all the known items, pick it by enum
        Item item = currentAction.item;
        if (item == null) {
          Debug.LogError("Item not defined for Enable");
          currentAction.Complete();
          return;
        }
        if (currentAction.action.change == ChangeWay.SwapSwitch)
          item.gameObject.SetActive(!item.gameObject.activeSelf);
        else
          item.gameObject.SetActive(currentAction.action.change == ChangeWay.EnOpenLock);
        currentAction.Complete();
      }
      break;

      case ActionType.Lock: {
        // Find the actual Item from all the known items, pick it by enum
        Actor a = GetActor(currentAction.action.actor);
        Item item = currentAction.item;
        if (item == null) {
          Debug.LogError("Item not defined for Lock");
          currentAction.Complete();
          return;
        }
        item.ForceLock(currentAction.action.change);
        currentAction.Complete();
      }
      break;

      case ActionType.ShowRoom: {
        currentRoom = allObjects.GetRoom(currentAction.action.strValue);
        Vector3 pos = currentAction.action.pos;
        pos.z = -10;
        cam.transform.position = pos;
        foreach (Room r in allObjects.roomsList)
          r.gameObject.SetActive(false);
        currentRoom.gameObject.SetActive(true);
        currentAction.Complete();
      }
      break;

      case ActionType.Cutscene: {
        currentCutscene = GetCutscene(currentAction.action.strValue);
        if (currentCutscene != null) {
          currentCutscene.Reset();
          forcedCursor = CursorTypes.Wait;
          oldCursor = null;
          GD.status = GameStatus.NormalGamePlay;
        }
        currentAction.Complete();
      }
      break;

      case ActionType.Sound: {
        Actor a = GetActor(currentAction.action.actor);
        if (a != null) a.SetDirection(currentAction.action.dir);

        Sounds.Play(currentAction.action.sound, currentActor.transform.position);
        currentAction.Play();
      }
      break;

      case ActionType.ReceiveY: {
        currentAction.performer.inventory.Remove(currentAction.item);
        currentAction.secondary.inventory.Add(currentAction.item);
        currentAction.item.owner = GetCharFromActor(currentAction.secondary);
        UpdateInventory();
        if (currentAction.secondary != null) {
          currentAction.secondary.Say(currentAction.action.strValue, currentAction.action);
          currentAction.secondary.SetDirection(currentAction.action.dir);
          currentAction.Play();
        }
        else
          currentAction.Complete();
      }
      break;

      case ActionType.ReceiveN: {
        if (currentAction.secondary != null) {
          currentAction.secondary.Say(currentAction.action.strValue, currentAction.action);
          currentAction.secondary.SetDirection(currentAction.action.dir);
          currentAction.Play();
        }
        else
          currentAction.Complete();
      }
      break;


      case ActionType.FadeIn:
        Fader.FadeIn();
        currentAction.Play();
        break;

      case ActionType.FadeOut:
        Fader.FadeOut();
        currentAction.Play();
        break;

      default: {
        // FIXME do the other actions
        Debug.Log("Not implemented action: " + currentAction.ToString());
        currentAction.Complete(); // Just to avoid to block the game for action not yet done
      }
      break;
    }

  }

  #endregion

  #region *********************** Inventory and Items *********************** Inventory and Items *********************** Inventory and Items ***********************
  public GameObject Inventory;
  public GameObject InventoryItemTemplate;
  private Item overItem = null; // Items we are over with the mouse
  private Item overInventoryItem = null; // Items we are over with the mouse in the inventory
  private Item usedItem = null; // Item that is being used (and visible on the cursor)

  internal static void UpdateInventory() {
    if (GD.c.Inventory.activeSelf)
      GD.c.ActivateInventory(GD.c.currentActor);
  }

  private void ActivateInventory(Actor actor) {
    Inventory.SetActive(true);
    InventoryPortrait.GetComponent<UnityEngine.UI.RawImage>().color = new Color32(0x7D, 0x8D, 0xfC, 0xff);
    foreach (Transform t in Inventory.transform)
      GameObject.Destroy(t.gameObject);

    foreach (Item item in actor.inventory) {
      GameObject ii = Instantiate(InventoryItemTemplate, Inventory.transform);
      ii.gameObject.SetActive(true);
      InventoryItem it = ii.GetComponent<InventoryItem>();
      it.text.text = item.Name;
      it.front.sprite = item.iconImage;
      it.item = item;
    }
  }

  internal static void SetItem(Item item, bool fromInventory = false) {
    if (GD.status != GameStatus.NormalGamePlay) return;

    if (fromInventory) {
      if (item == null) {
        GD.c.overItem = null;
        GD.c.overInventoryItem = null;
        if (GD.c.TextMsg.text != "") GD.c.HideName();
        return;
      }
      GD.c.overInventoryItem = item;
      if (GD.c.usedItem == null) {
        GD.c.forcedCursor = CursorTypes.PickUp;
        GD.c.overInventoryItem = item;
        GD.c.ShowName(item.Name);
      }
      return;
    }

    if (item == null) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.None;
      GD.c.overItem = null;
      if (GD.c.TextMsg.text != "") GD.c.HideName();
      return;
    }
    if (item.owner != Chars.None) {
      GD.c.overItem = item;
      return;
    }

    // Right
    if (item.whatItDoesR == WhatItDoes.Walk) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.None;
      GD.c.overItem = item;
    }
    else if (item.whatItDoesR == WhatItDoes.Pick) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.PickUp;
      GD.c.overItem = item;
      GD.c.ShowName(item.Name);
    }
    else if (item.whatItDoesR == WhatItDoes.Use) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.Use;
      GD.c.overItem = item;
      GD.c.ShowName(item.Name);
    }
    else if (item.whatItDoesR == WhatItDoes.Read) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.Examine;
      GD.c.overItem = item;
      GD.c.ShowName(item.Name);
    }
    // Left
    else if (item.whatItDoesL == WhatItDoes.Walk) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.None;
      GD.c.overItem = item;
    }
    else if (item.whatItDoesL == WhatItDoes.Pick) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.PickUp;
      GD.c.overItem = item;
      GD.c.ShowName(item.Name);
    }
    else if (item.whatItDoesL == WhatItDoes.Use) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.Use;
      GD.c.overItem = item;
      GD.c.ShowName(item.Name);
    }
    else if (item.whatItDoesL == WhatItDoes.Read) {
      if (GD.c.forcedCursor != CursorTypes.Item) GD.c.forcedCursor = CursorTypes.Examine;
      GD.c.overItem = item;
      GD.c.ShowName(item.Name);
    }
  }

  internal static bool IsItemCollected(ItemEnum itemID) {
    foreach(Item item in GD.c.actor1.inventory) {
      if (item.Item == itemID) return true;
    }
    foreach(Item item in GD.c.actor2.inventory) {
      if (item.Item == itemID) return true;
    }
    foreach(Item item in GD.c.actor3.inventory) {
      if (item.Item == itemID) return true;
    }
    return false;
  }

  internal static Item GetItem(string item) {
    if (item == null) return null;
    return GD.c.allObjects.FindItemByID(item);
  }
  #endregion

  #region *********************** Actors *********************** Actors *********************** Actors *********************** Actors *********************** Actors ***********************
  public PortraitClickHandler ActorPortrait1;
  public PortraitClickHandler ActorPortrait2;
  public PortraitClickHandler ActorPortrait3;
  public PortraitClickHandler InventoryPortrait;
  Actor actor1;
  Actor actor2;
  Actor actor3;
  Actor kidnappedActor;
  Actor receiverActor;
  Actor currentActor = null;
  Color32 unselectedActor = new Color32(0x6D, 0x7D, 0x7C, 255);
  Color32 selectedActor = new Color32(200, 232, 152, 255);
  public Actor[] allEnemies;
  public Actor[] allActors;
  public Sprite[] Portraits;

  /// <summary>
  /// Gets the actual Actor from the Chars enum
  /// </summary>
  public static Actor GetActor(Chars actor) {
    switch (actor) {
      case Chars.None: return null;
      case Chars.Current: return GD.c.currentActor;
      case Chars.Actor1: return GD.c.actor1;
      case Chars.Actor2: return GD.c.actor2;
      case Chars.Actor3: return GD.c.actor3;
      case Chars.KidnappedActor: return GD.c.kidnappedActor;
      case Chars.Receiver: return GD.c.receiverActor;
      case Chars.Fred: return GD.c.allEnemies[0];
      case Chars.Edna: return GD.c.allEnemies[1];
      case Chars.Ted: return GD.c.allEnemies[2];
      case Chars.Ed: return GD.c.allEnemies[3];
      case Chars.Edwige: return GD.c.allEnemies[4];
      case Chars.GreenTentacle: return GD.c.allEnemies[5];
      case Chars.PurpleTentacle: return GD.c.allEnemies[6];
      case Chars.Dave: return GD.c.allActors[0];
      case Chars.Bernard: return GD.c.allActors[1];
      case Chars.Wendy: return GD.c.allActors[2];
      case Chars.Syd: return GD.c.allActors[3];
      case Chars.Hoagie: return GD.c.allActors[4];
      case Chars.Razor: return GD.c.allActors[5];
      case Chars.Michael: return GD.c.allActors[6];
      case Chars.Jeff: return GD.c.allActors[7];
      case Chars.Javid: return GD.c.allActors[8];
      case Chars.Laverne: return GD.c.allActors[9];
      case Chars.Ollie: return GD.c.allActors[10];
      case Chars.Sandy: return GD.c.allActors[11];
    }
    Debug.LogError("Invalid actor requested! " + actor);
    return null;
  }

  public Sprite GetActorPortrait(Chars actor) {
    int idx = (int)actor;
    if (idx < 10) return null;

    return Portraits[idx - 10];
  }

  public static Chars GetCharFromActor(Actor actor) {
    if (actor == null) return Chars.None;
    else if (actor == GD.c.actor1) return Chars.Actor1;
    else if (actor == GD.c.actor2) return Chars.Actor2;
    else if (actor == GD.c.actor3) return Chars.Actor3;
    else if (actor == GD.c.kidnappedActor) return Chars.KidnappedActor;
    else if (actor == GD.c.receiverActor) return Chars.Receiver;
    else if (actor == GD.c.allEnemies[0]) return Chars.Fred;
    else if (actor == GD.c.allEnemies[1]) return Chars.Edna;
    else if (actor == GD.c.allEnemies[2]) return Chars.Ted;
    else if (actor == GD.c.allEnemies[3]) return Chars.Ed;
    else if (actor == GD.c.allEnemies[4]) return Chars.Edwige;
    else if (actor == GD.c.allEnemies[5]) return Chars.GreenTentacle;
    else if (actor == GD.c.allEnemies[6]) return Chars.PurpleTentacle;
    else if (actor == GD.c.allActors[0]) return Chars.Dave;
    else if (actor == GD.c.allActors[1]) return Chars.Bernard;
    else if (actor == GD.c.allActors[2]) return Chars.Wendy;
    else if (actor == GD.c.allActors[3]) return Chars.Syd;
    else if (actor == GD.c.allActors[4]) return Chars.Hoagie;
    else if (actor == GD.c.allActors[5]) return Chars.Razor;
    else if (actor == GD.c.allActors[6]) return Chars.Michael;
    else if (actor == GD.c.allActors[7]) return Chars.Jeff;
    else if (actor == GD.c.allActors[8]) return Chars.Javid;
    else if (actor == GD.c.allActors[9]) return Chars.Laverne;
    else if (actor == GD.c.allActors[10]) return Chars.Ollie;
    else if (actor == GD.c.allActors[11]) return Chars.Sandy;
    else if (actor == GD.c.currentActor) return Chars.Current;
    Debug.LogError("Invalid actor requested! " + actor);
    return Chars.None;
  }

  /// <summary>
  /// Checks if the passed actor is an enemy (Fred, Edna, Ed, etc.)
  /// </summary>
  public static bool IsEnemy(Actor actor) {
    foreach (Actor a in GD.c.allEnemies)
      if (a == actor) return true;
    return false;
  }

  /// <summary>
  /// Checks if the actor is one of the actors of the playing trio
  /// </summary>
  internal static bool WeHaveActorPlaying(Chars actor) {
    return actor == GD.c.actor1.id || actor == GD.c.actor2.id || actor == GD.c.actor3.id;
  }
  float walkDelay = 0;

  /// <summary>
  /// Moves the actor to the destination and execute the action callback when the destination is reached
  /// </summary>
  void WalkAndAction(Actor actor, Item item, System.Action<Actor, Item> action) {
    Vector3 one = actor.transform.position;
    one.z = 0;
    Vector3 two = item.HotSpot;
    two.z = 0;
    float dist = Vector3.Distance(one, two);
    if (dist > .2f) { // Need to walk
      RaycastHit2D hit = Physics2D.Raycast(overItem.HotSpot, cam.transform.forward, 10000, pathLayer);
      if (hit.collider != null) {
        PathNode p = hit.collider.GetComponent<PathNode>();
        currentActor.WalkTo(overItem.HotSpot, p, action, item);
      }
      return;
    }
    else {
      action?.Invoke(currentActor, item);
    }
  }

  internal static void SelectActor(Actor actor) {
    if (GD.status != GameStatus.NormalGamePlay) return;

    GD.c.forcedCursor = CursorTypes.None;
    GD.c.oldCursor = null;
    GD.c.usedItem = null;

    GD.c.currentActor = actor;
    GD.c.ActorPortrait1.GetComponent<UnityEngine.UI.RawImage>().color = GD.c.unselectedActor;
    GD.c.ActorPortrait2.GetComponent<UnityEngine.UI.RawImage>().color = GD.c.unselectedActor;
    GD.c.ActorPortrait3.GetComponent<UnityEngine.UI.RawImage>().color = GD.c.unselectedActor;
    if (actor == GD.c.actor1) {
      GD.c.ActorPortrait1.GetComponent<UnityEngine.UI.RawImage>().color = GD.c.selectedActor;
    }
    else if (actor == GD.c.actor2) {
      GD.c.ActorPortrait2.GetComponent<UnityEngine.UI.RawImage>().color = GD.c.selectedActor;
    }
    if (actor == GD.c.actor3) {
      GD.c.ActorPortrait3.GetComponent<UnityEngine.UI.RawImage>().color = GD.c.selectedActor;
    }
    GD.c.ShowName("Selected: " + GD.c.currentActor.name);
    if (GD.c.currentActor.currentRoom != GD.c.currentRoom) { // Different room
      GD.c.StartCoroutine(GD.c.FadeToRoomActor());
    }
    if (GD.c.Inventory.activeSelf) GD.c.ActivateInventory(GD.c.currentActor);
  }

  Actor overActor = null;
  internal static void OverActor(Actor actor) {
    GD.c.overActor = actor;
  }


  #endregion

  #region *********************** Rooms and Transitions *********************** Rooms and Transitions *********************** Rooms and Transitions ***********************
  Room currentRoom;

  private IEnumerator ChangeRoom(Actor actor, Door door) {
    // Disable gameplay
    GD.status = GameStatus.Cutscene;
    yield return null;

    // Enable dst
    door.dst.gameObject.SetActive(true);

    // Get dst camera pos + door dst pos
    Vector3 orgp = cam.transform.position;
    Vector3 dstp = door.camerapos;

    // Move camera quickly from current to dst
    float time = 0;
    while (time < .125f) {
      // Fade black
      BlackFade.color = new Color32(0, 0, 0, (byte)(255 * (time * 8)));
      cam.transform.position = (1 - time * 4) * orgp + (time * 4) * dstp;
      time += Time.deltaTime;
      yield return null;
    }
    actor.transform.position = door.correspondingDoor.HotSpot;
    yield return null;
    while (time < .25f) {
      // Fade black
      BlackFade.color = new Color32(0, 0, 0, (byte)(255 * (1 - (8 * (time - .125f)))));
      cam.transform.position = (1 - time * 4) * orgp + (time * 4) * dstp;
      time += Time.deltaTime;
      yield return null;
    }
    cam.transform.position = dstp;

    // Disable src
    door.src.gameObject.SetActive(false);
    actor.Stop();

    // Move actor to dst door pos
    currentRoom = door.dst;
    currentActor = actor;
    actor.transform.position = door.correspondingDoor.HotSpot;
    actor.currentRoom = currentRoom;
    actor.SetDirection(door.correspondingDoor.arrivalDirection);
    RaycastHit2D hit = Physics2D.Raycast(door.correspondingDoor.HotSpot, cam.transform.forward, 10000, pathLayer);
    if (hit.collider != null) {
      PathNode p = hit.collider.GetComponent<PathNode>();
      currentActor.SetScaleAndPosition(door.correspondingDoor.HotSpot, p);
    }
    yield return null;

    // Disable actors not in current room
    foreach (Actor a in allActors) {
      if (a == null) continue;
      a.gameObject.SetActive(a.currentRoom == currentRoom);
    }
    foreach (Actor a in allEnemies) {
      if (a == null) continue;
      a.gameObject.SetActive(a.currentRoom == currentRoom);
    }

    // Enable gmaeplay
    GD.status = GameStatus.NormalGamePlay;
    forcedCursor = CursorTypes.None;
    overItem = null;
    ShowName(currentRoom.name);
  }

  private IEnumerator FadeToRoomActor() {
    // Disable gameplay
    GD.status = GameStatus.Cutscene;
    yield return null;

    Room prev = currentRoom;
    currentRoom = currentActor.currentRoom;

    // Get dst camera pos + door dst pos
    Vector3 dstp = currentActor.transform.position;
    dstp.y = currentRoom.CameraGround;
    dstp.z = -10;

    // Move camera quickly from current to dst
    float time = 0;
    while (time < .125f) {
      // Fade black
      BlackFade.color = new Color32(0, 0, 0, (byte)(255 * (time * 8)));
      time += Time.deltaTime;
      yield return null;
    }
    prev.gameObject.SetActive(false);
    currentRoom.gameObject.SetActive(true);
    cam.transform.position = dstp;
    GD.status = GameStatus.NormalGamePlay; // Enable gmaeplay, this will make the camera to adjust
    yield return null;

    // Disable actors not in current room
    foreach (Actor a in allActors) {
      if (a == null) continue;
      a.gameObject.SetActive(a.currentRoom == currentRoom);
    }
    foreach (Actor a in allEnemies) {
      if (a == null) continue;
      a.gameObject.SetActive(a.currentRoom == currentRoom);
    }


    while (time < .25f) {
      // Fade black
      BlackFade.color = new Color32(0, 0, 0, (byte)(255 * (1 - (8 * (time - .125f)))));
      time += Time.deltaTime;
      yield return null;
    }
    forcedCursor = CursorTypes.None;
    overItem = null;
  }

  #endregion

  #region *********************** UI and Options *********************** UI and Options *********************** UI and Options *********************** UI and Options ***********************
  public static float walkSpeed;
  public static float textSpeed;
  public Options options;

  public TextMeshProUGUI TextMsg;
  public RectTransform TextMsgRT;
  public RectTransform TextBackRT;
  TextMsgMode txtMsgMode = TextMsgMode.None;
  float textMsgTime = 0;
  float textSizeX = 0;

  public GameObject ActorsButtons;
  public Canvas SkyBackground;

  void ShowName(string name) {
    if (TextMsg.text == name) return;
    textSizeX = TextMsg.GetPreferredValues(name).x;
    TextMsgRT.sizeDelta = new Vector2(0, 50);
    TextBackRT.sizeDelta = TextMsgRT.sizeDelta;
    TextMsg.text = name;
    textMsgTime = .25f;
    txtMsgMode = TextMsgMode.Appearing;
  }

  void HideName() {
    if (txtMsgMode == TextMsgMode.Disappearing || txtMsgMode == TextMsgMode.None) return;
    textMsgTime = .25f;
    txtMsgMode = TextMsgMode.Disappearing;
  }

  #endregion




  public static void PlayMusic(AudioClip clip) {
    GD.c.MusicPlayer.Stop();
    GD.c.MusicPlayer.clip = clip;
    GD.c.MusicPlayer.Play();
  }

  public static void StopMusic() {
    GD.c.MusicPlayer.Stop();
  }

  public static void PauseMusic() {
    if (GD.c.MusicPlayer.isPlaying)
      GD.c.MusicPlayer.Pause();
    else
      GD.c.MusicPlayer.UnPause();
  }



}


