﻿using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour {
  public Sprite[] facesF;
  public Sprite[] facesB;
  public Sprite[] facesL;
  public Sprite[] facesR;
  public SpriteRenderer Face;
  public SpriteRenderer Arms;
  public SpriteRenderer Legs;
  public Material Normal;
  public Material Outline;
  public Room currentRoom;
  Animator anim;
  readonly Parcour3 destination = new Parcour3(Vector3.zero, null);
  System.Action<Actor, Item> callBack = null;
  Item callBackItem = null;
  bool walking = false;
  Dir dir = Dir.F;
  private AudioSource audios;
  public List<Item> inventory;
  public Chars id;
  public float mainScale = 1f;
  public List<Skill> skills;
  List<Parcour> parcour;
  [TextArea(3, 12)] public string Description;
  FloorType floor = FloorType.None;

  private void Awake() {
    anim = GetComponent<Animator>();
    audios = GetComponent<AudioSource>();

    if (currentRoom != null)
      SetScaleAndPosition(new Vector3((currentRoom.maxR + currentRoom.minL) / 2, (currentRoom.maxY - currentRoom.minY), 0));
  }

  internal bool HasItem(ItemEnum item) {
    foreach (Item i in inventory)
      if (i.Item == item) return true;
    return false;
  }

  public string HasSkill(Skill skill) {
    foreach (Skill s in skills)
      if (s == skill) return null;
    switch (skill) {
      case Skill.Strenght: return "I am not strong enough!";
      case Skill.Courage: return "It is scary!\nI will not do it!";
      case Skill.Chef: return "I am not a chef";
      case Skill.Handyman: return "I don't know how to do it";
      case Skill.Geek: return "I am not a geek";
      case Skill.Nerd: return "I am not a nerd";
      case Skill.Music: return "I don't know how to play";
      case Skill.Writing: return "I am not a writer";
      case Skill.None:
        break;
    }
    return "I cannot do it";
  }

  public void AddSkill(Skill skill) {
    foreach (Skill s in skills)
      if (s == skill) return;
    skills.Add(skill);
  }

  bool isSpeaking = false;
  int faceNum = 0;
  float speakt = 0;
  GameAction fromAction = null;

  public bool Say(string message, GameAction action = null) {
    if (message == null) return true;
    isSpeaking = true;
    faceNum = 0;
    speakt = 0;
    fromAction = action;
    Balloon.Show(message, transform, CompleteSpeaking);
    return false;
  }

  public void CompleteSpeaking() {
    isSpeaking = false;
    faceNum = 0;
    if (fromAction != null) fromAction.Complete();
  }

  internal void SetDirection(Dir d) {
    if (d == Dir.None) return;
    if (d != Dir.L && d != Dir.R && d != Dir.F && d != Dir.B) {
      Debug.LogError("Found the culprit: " + d);
      return;
    }
    dir = d;
  }

  internal void SetExpression(Expression exp) {
    faceNum = (int)exp;
  }

  internal bool IsWalking() {
    return walking;
  }

  void OnMouseEnter() {
    Controller.OverActor(this);
    Face.material = Outline;
    Arms.material = Outline;
    Legs.material = Outline;
  }

  void OnMouseExit() {
    Controller.OverActor(null);
    Face.material = Normal;
    Arms.material = Normal;
    Legs.material = Normal;
  }

  public void Stop() {
    isSpeaking = false;
    walking = false;
  }

  Dir CalculateDirection(Vector3 point) {
    Vector3 ap = transform.position;
    Vector3 mp = point;

    float dx = ap.x - mp.x;
    float dy = ap.y - mp.y;

    // Vert or Horiz?
    if (Mathf.Abs(dx) * 1.2f < Mathf.Abs(dy)) {
      // Vert
      if (dy < 0) return Dir.B;
      return Dir.F;
    }
    else {
      // Horiz
      if (dx < 0) return Dir.R;
      return Dir.L;
    }
  }


  internal void WalkTo(Vector2 dest, PathNode p, System.Action<Actor, Item> action = null, Item item = null) {
    destination.pos = dest;
    destination.node = p;
    destination.pos.z = transform.position.z;
    Vector2 wdir = destination.pos - transform.position;
    float dist = Vector2.Distance(transform.position, dest);
    if (wdir.sqrMagnitude < .1f || dist < .15f) {
      action?.Invoke(this, item);
      return;
    }

    // Calculate the path
    parcour = p.parent.PathFind(transform.position, dest);
    if (parcour == null) {
      destination.pos = dest;
      floor = p.floorType;
    }
    else {
      destination.pos = parcour[1].pos;
      destination.node = parcour[1].node;
      floor = parcour[1].node.floorType;
      parcour.RemoveRange(0, 2);
    }
    destination.pos.z = (destination.pos.y - currentRoom.CameraGround) / 10f;

    callBack = action;
    callBackItem = item;
    dir = CalculateDirection(destination.pos);
    anim.Play("Walk" + dir);
    walking = true;
  }

  internal void PlaySound(AudioClip audioClip) {
    audios.clip = audioClip;
    audios.Play();
  }

  private void Update() {
    if (isSpeaking) {
      speakt += Time.deltaTime;
      if (speakt > .15f) {
        speakt = 0;
        faceNum = Random.Range(2, 5);
      }
    }

    switch (dir) {
      case Dir.F: Face.sprite = facesF[faceNum]; break;
      case Dir.B: Face.sprite = facesB[faceNum]; break;
      case Dir.L: Face.sprite = facesL[faceNum]; break;
      case Dir.R: Face.sprite = facesR[faceNum]; break;
    }
    if (!walking) {
      if (dir == Dir.None) dir = Dir.F;
      anim.Play("Idle" + dir);
      if (audios.isPlaying) audios.Stop();
      return;
    }

    if (!audios.isPlaying) {
      audios.clip = GD.GetStepSound(floor); // FIXME understand where are we walking. Probably from the Path
      audios.Play();
    }
    anim.speed = Controller.walkSpeed * .8f;
    Vector2 walkDir = (destination.pos - transform.position);
    Vector3 wdir = walkDir.normalized;
    wdir.y *= .65f;
    Vector3 np = transform.position + wdir * 4f * Controller.walkSpeed * Time.deltaTime;
    np.z = 0;

    float ty = transform.position.y;
    if (ty < currentRoom.minY) ty = currentRoom.minY;
    if (ty > currentRoom.maxY) ty = currentRoom.maxY;
    float scaley = -.05f * (ty - currentRoom.minY - 1.9f) + .39f;
    if (!destination.node.isStair) {
      scaley *= mainScale;
      transform.localScale = new Vector3(scaley, scaley, 1);

      int zpos = (int)(scaley * 10000);
      Face.sortingOrder = zpos;
      Arms.sortingOrder = zpos;
      Legs.sortingOrder = zpos;
    }

    if (walkDir.sqrMagnitude < .05f) {
      if (parcour == null || parcour.Count == 0) {
        transform.position = destination.pos;
        walking = false;
        callBack?.Invoke(this, callBackItem);
        callBack = null;
        return;
      }
      destination.pos = parcour[0].pos;
      destination.node = parcour[0].node;
      floor = parcour[0].node.floorType;
      parcour.RemoveAt(0);
      dir = CalculateDirection(destination.pos);
      anim.Play("Walk" + dir);
    }
    transform.position = np;
  }

  public void SetScaleAndPosition(Vector3 pos, PathNode p = null) {
    float ty = pos.y;
    if (ty < currentRoom.minY) ty = currentRoom.minY;
    if (ty > currentRoom.maxY) ty = currentRoom.maxY;
    if (p == null) {
      if (destination.node == null || !destination.node.isStair) {
        float scaley = -.05f * (ty - currentRoom.minY - 1.9f) + .39f;
        scaley *= mainScale;
        transform.localScale = new Vector3(scaley, scaley, 1);
      }
    }
    else if (!p.isStair) {
      float scaley = -.05f * (ty - currentRoom.minY - 1.9f) + .39f;
      scaley *= mainScale;
      transform.localScale = new Vector3(scaley, scaley, 1);
    }
    pos.y = ty;
    transform.position = pos;
  }
}
