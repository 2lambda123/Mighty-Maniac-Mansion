﻿using UnityEngine;

public class Dog : Actor {
  public Animator SamAnim;
  public Animator HeadAnim;
  public Animator TailAnim;
  public Animator BodyAnim;
  public SpriteRenderer HeadSR;
  public SpriteRenderer TailSR;
  public SpriteRenderer BodySR;
  public AudioSource Audio;

  public AudioClip[] Barks;

  public int friendly = 0;
  float friendCheck = 1f;

  float timeout = 1f;
  bool sit = false;
  bool tail = false;
  bool tongue = false;
  bool isWalking = false;
  float walkTime = 0;
  float walkedTime = 0;
  Vector3 startpos, endpos;

  float dist = 0;
  float animDelay = 0;

  private void Awake() {
    // Do nothing
    inventory = new System.Collections.Generic.List<Item>();
  }

  private void Start() {
    ScaleByPosition(transform.position.y);
  }


  void Update() {
    if (GD.c == null || GD.c.currentActor == null) return;

    if (animDelay > 0) {
      animDelay -= Time.deltaTime;
      if (animDelay <= 0) {
        SamAnim.enabled = false;
        BodyAnim.enabled = true;
        TailAnim.enabled = true;
        HeadAnim.enabled = true;
        HeadSR.transform.localPosition = Vector3.zero;
        TailSR.transform.localPosition = Vector3.zero;
        BodySR.transform.localPosition = Vector3.zero;
        HeadSR.transform.localRotation = Quaternion.identity;
        TailSR.transform.localRotation = Quaternion.identity;
        BodySR.transform.localRotation = Quaternion.identity;
      }
      return;
    }

    IsVisible = true;
    if (friendly != 3) {
      friendCheck -= Time.deltaTime;
      if (friendCheck < 0) {
        friendly = AllObjects.GetFlag(GameFlag.SamIsFriend);
        friendCheck = .5f;
      }
    }
    dist = Mathf.Abs(GD.c.currentActor.transform.position.x - transform.position.x);

    /*
    SamIsFriend
    0 -> not friend panning not done
    1 -> not friend panning done
    2 -> throwing bone
    3 -> friend
    */

    if (friendly == 3) { // Friend. If made friend, just stay close to the home, and move tail, toungue randomly
      if (isWalking) {
        transform.localPosition = Vector3.Lerp(startpos, endpos, walkedTime / walkTime);
        ScaleByPosition(transform.position.y);
        walkedTime += Time.deltaTime;
        if (walkedTime >= walkTime) {
          BodyAnim.Play("Body Idle");
          isWalking = false;
        }
      }

      timeout -= Time.deltaTime;
      if (timeout > 0) return;
      timeout = 1;

      int rnd = Random.Range(0, 5);
      if (rnd == 0 && !isWalking) { // Stay idle --------------------------------------------------------------------------------------------------
        if (sit) {
          sit = false;
          Vector3 pos = transform.localPosition;
          pos.y += .3f;
          transform.localPosition = pos;
        }
        BodyAnim.Play("Body Idle");
      }
      else if (rnd == 1 && !isWalking) { // Walk to random pos --------------------------------------------------------------------------------------------------
        if (sit) {
          sit = false;
          Vector3 pos = transform.localPosition;
          pos.y += .3f;
          transform.localPosition = pos;
        }
        BodyAnim.Play("Body Walk");

        startpos = transform.localPosition;
        endpos = Vector3.zero;
        endpos.x += Random.Range(-2.5f, 1.5f);
        endpos.y += Random.Range(-.9f, .5f);
        bool flip = endpos.x < startpos.x;
        HeadSR.flipX = flip;
        BodySR.flipX = flip;
        TailSR.flipX = flip;
        isWalking = true;
        walkTime = (endpos - startpos).magnitude;
        walkedTime = 0;
      }
      else if (rnd == 2 && !isWalking) { // Stay put --------------------------------------------------------------------------------------------------
        if (sit) return;
        sit = true;
        Vector3 pos = transform.localPosition;
        pos.y -= .3f;
        transform.localPosition = pos;
        BodyAnim.Play("Body Sit");
      }
      else if (rnd == 3) { // Wiggle tail on/off --------------------------------------------------------------------------------------------------
        tail = !tail;
        if (tail)
          TailAnim.Play("Tail Idle");
        else
          TailAnim.Play("Tail Wiggle");
      }
      else if (rnd == 4) { // Tongue on/off --------------------------------------------------------------------------------------------------
        tongue = !tongue;
        if (tongue)
          HeadAnim.Play("Head Tongue");
        else
          HeadAnim.Play("Head Idle");
      }

      CheckActorBlocking(GD.c.actor1, false);
      CheckActorBlocking(GD.c.actor2, false);
      CheckActorBlocking(GD.c.actor3, false);

    }
    else if (friendly == 2) { // Throwing bone (or any other anim)
      if (isWalking) {
        transform.localPosition = Vector3.Lerp(startpos, endpos, walkedTime / walkTime);
        ScaleByPosition(transform.position.y);
        walkedTime += Time.deltaTime;
        if (walkedTime >= walkTime) {
          BodyAnim.Play("Body Idle");
          isWalking = false;
        }
      }

      HeadSR.flipX = false;
      BodySR.flipX = false;
      TailSR.flipX = false;
      timeout -= Time.deltaTime;
      if (timeout > 0) return;
      timeout = 1;

    }
    else {
      CheckActorBlocking(GD.c.actor1, true);
      CheckActorBlocking(GD.c.actor2, true);
      CheckActorBlocking(GD.c.actor3, true);

      if (dist > 4f) { // Not friend and too close. Point actor and grind. Stop wiggle. Bark from time to time
        if (!Audio.isPlaying) {
          BodyAnim.Play("Body Idle");
          TailAnim.Play("Tail Idle");
          HeadAnim.Play("Head Grind");
          isWalking = false;
          bool flip = transform.position.x > GD.c.currentActor.transform.position.x;
          HeadSR.flipX = flip;
          BodySR.flipX = flip;
          TailSR.flipX = flip;

          Audio.clip = Barks[0];
          Audio.Play();
        }

      }
      else { // Not friend and too close, point actor and bark strong
        if (!Audio.isPlaying) {
          BodyAnim.Play("Body Idle");
          TailAnim.Play("Tail Idle");
          HeadAnim.Play("Head Chow");
          isWalking = false;
          bool flip = transform.position.x > GD.c.currentActor.transform.position.x;
          HeadSR.flipX = flip;
          BodySR.flipX = flip;
          TailSR.flipX = flip;

          Audio.clip = Barks[Random.Range(1, Barks.Length)];
          Audio.Play();
        }
      }
    }
  }

  void OnMouseEnter() {
    if (Controller.NotItemUsed() || Options.IsActive() || Controller.OverActor(this)) return;
    Material m = GD.Outline();
    HeadSR.material = m;
    TailSR.material = m;
    BodySR.material = m;
  }

  void OnMouseExit() {
    if (Options.IsActive()) return;
    Controller.OverActor(null);
    Material m = GD.Normal();
    HeadSR.material = m;
    TailSR.material = m;
    BodySR.material = m;
  }


  private void CheckActorBlocking(Actor a, bool block) {
    if (a.currentRoom == currentRoom) {
      if (block) {
        if (a.transform.position.x > transform.position.x)
          a.SetMinMaxX(-126, -113);
        else
          a.SetMinMaxX(-142, -135);
      }
      else
        a.SetMinMaxX(float.NegativeInfinity, float.PositiveInfinity);
    }
  }

  void ScaleByPosition(float y) {
    float ty = y;
    if (ty < currentRoom.minY) ty = currentRoom.minY;
    if (ty > currentRoom.maxY) ty = currentRoom.maxY;
    float scaley = -.05f * (ty - currentRoom.minY - 1.9f) + .39f;

    int zpos = (int)(scaley * 10000);
    scaley *= currentRoom.scalePerc * mainScale;
    transform.localScale = new Vector3(scaley, scaley, 1);
    TailSR.sortingOrder = zpos;
    HeadSR.sortingOrder = zpos + 1;
    BodySR.sortingOrder = zpos + 2;
  }

  internal void WalkToPos(Vector2 dest) {
    startpos = transform.localPosition;
    endpos = dest;
    isWalking = true;
    walkedTime = 0;
    walkTime = (endpos - startpos).magnitude * .5f;
  }

  internal void PlayAnim(string anim, float delay) {
    SamAnim.enabled = true;
    BodyAnim.enabled = false;
    TailAnim.enabled = false;
    HeadAnim.enabled = false;
    SamAnim.StopPlayback();
    SamAnim.Play(anim, -1, 0);
    animDelay = delay;
  }
}
