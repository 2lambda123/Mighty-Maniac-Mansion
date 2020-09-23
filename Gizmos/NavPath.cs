﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NavPath : MonoBehaviour {
  // List of PathNodes
  public List<PathNode> nodes;
  public bool ShowSubNodes;
  public bool DoAStar;

  public Vector2 start = Vector2.zero;
  public Vector2 end = Vector2.zero;
  public List<Vector2> gizmoLines;

#if UNITY_EDITOR
  void OnDrawGizmos() {
    if (!DoAStar) return;

    PathNode p = FindPathNodeFromPoint(start);
    if (p == null)
      Gizmos.DrawIcon(start, "PointOut.png", false);
    else
      Gizmos.DrawIcon(start, "PointInStart.png", false);
    p = FindPathNodeFromPoint(end);
    if (p == null)
      Gizmos.DrawIcon(end, "PointOut.png", false);
    else
      Gizmos.DrawIcon(end, "PointInEnd.png", false);


    if (gizmoLines == null || gizmoLines.Count < 2) return;
    //    ShowPaths = false;

    Gizmos.color = Color.red;
    for (int i = 0; i < gizmoLines.Count - 1; i++) {
      Vector3 a = gizmoLines[i];
      Vector3 b = gizmoLines[i + 1];
      Gizmos.DrawLine(a, b);
      if (i > 0) Gizmos.DrawIcon(a, "PointPath.png", false);
    }
  }
#endif

  private PathNode FindPathNodeFromPoint(Vector2 point) {
    foreach (PathNode p in nodes)
      if (IsInsideTri(p.tl, p.tr, p.br, point) || IsInsideTri(p.br, p.bl, p.tl, point)) return p;
    return null;
  }

  float Area(Vector2 v1, Vector2 v2, Vector2 v3) {
    return Mathf.Abs((v1.x * (v2.y - v3.y) + v2.x * (v3.y - v1.y) + v3.x * (v1.y - v2.y)) / 2.0f);
  }

  /* A function to check whether point P(x, y) lies inside the triangle formed by vertices V1, V2, and V3 */
  bool IsInsideTri(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p) {
    float A = Area(v1, v2, v3); /* Calculate area of triangle ABC */
    float A1 = Area(p, v2, v3); /* Calculate area of triangle PBC */
    float A2 = Area(v1, p, v3); /* Calculate area of triangle PAC */
    float A3 = Area(v1, v2, p); /* Calculate area of triangle PAB */

    /* Check if sum of A1, A2 and A3 is same as A */
    return Mathf.Abs(A - (A1 + A2 + A3)) < .1f;
  }

  public void EditorCalculatePath() {
    gizmoLines = PathFind(start, end);
  }


  public List<Vector2> PathFind(Vector2 start, Vector2 end) {
    PathNode pstart = FindPathNodeFromPoint(start);
    PathNode pend = FindPathNodeFromPoint(end);
    if (pstart == null || pend == null) {
      Debug.LogWarning("Path not inside nodes!");
      return null;
    }

    List<Vector2> res = new List<Vector2>();
    List<PathNode> openSet = new List<PathNode> { pstart };
    pstart.prev = null;
    pend.prev = null;
    foreach (PathNode n in nodes) {
      n.g = float.PositiveInfinity;
      n.h = float.PositiveInfinity;
      n.prev = null;
    }
    pstart.g = 0;
    pstart.h = 0;

    bool found = false;
    while (openSet.Count > 0) {
      PathNode q = GetMinPath(openSet);
      if (q == pend) {
        found = true;
        break;
      }
      openSet.Remove(q);

      if (q.left != null) {
        // d(current,neighbor) is the weight of the edge from current to neighbor
        // tentative_gScore is the distance from start to the neighbor through current

        q.left.h = Vector2.Distance(q.left.Center(), pend.Center());
        float tentativeScore = q.g + q.left.h;
        if (tentativeScore < q.left.g) {
          // This path to neighbor is better than any previous one. Record it!
          q.left.prev = q;
          q.left.g = tentativeScore;
          if (!openSet.Contains(q.left)) openSet.Add(q.left);
        }
      }
      if (q.right != null) {
        q.right.h = Vector2.Distance(q.right.Center(), pend.Center());
        float tentativeScore = q.g + q.right.h;
        if (tentativeScore < q.right.g) {
          // This path to neighbor is better than any previous one. Record it!
          q.right.prev = q;
          q.right.g = tentativeScore;
          if (!openSet.Contains(q.right)) openSet.Add(q.right);
        }
      }
      if (q.top != null) {
        q.top.h = Vector2.Distance(q.top.Center(), pend.Center());
        float tentativeScore = q.g + q.top.h;
        if (tentativeScore < q.top.g) {
          // This path to neighbor is better than any previous one. Record it!
          q.top.prev = q;
          q.top.g = tentativeScore;
          if (!openSet.Contains(q.top)) openSet.Add(q.top);
        }
      }
      if (q.down != null) {
        q.down.h = Vector2.Distance(q.down.Center(), pend.Center());
        float tentativeScore = q.g + q.down.h;
        if (tentativeScore < q.down.g) {
          // This path to neighbor is better than any previous one. Record it!
          q.down.prev = q;
          q.down.g = tentativeScore;
          if (!openSet.Contains(q.down)) openSet.Add(q.down);
        }
      }
    }

    if (!found) return null;
    if (pend.prev == null) { // All in the same node
      res.Add(start);
      res.Add(end);
      return res;
    }

    // reconstruct the path and Create the result of vector2s
    // Minimize the paths

    res.Add(end);
    PathNode p1 = pend;
    PathNode p2 = pend.prev;
    while (p1 != null && p2 != null) {
      Vector2 c1 = p1 == pend ? end : (p1 == pstart ? start : p1.Center());
      Vector2 c2 = p2 == pend ? end : (p2 == pstart ? start : p2.Center());

      // Check if the line cross the edge or not
      Vector2 intersection = FindIntersection(c1, c2, GetEdge(p1, p2, true), GetEdge(p1, p2, false));
      if (intersection.x != float.NaN)
        res.Add(intersection);

      p1 = p2;
      p2 = p2.prev;

      if (p1 == pstart) {
        res.Add(start);
        break;
      }
      if (p2 == pstart) {
        c1 = p1 == pend ? end : (p1 == pstart ? start : p1.Center());
        c2 = p2 == pend ? end : (p2 == pstart ? start : p2.Center());

        // Check if the line cross the edge or not
        intersection = FindIntersection(c1, c2, GetEdge(p1, p2, true), GetEdge(p1, p2, false));
        if (intersection.x != float.NaN) {
          res.Add(intersection);
        }

        res.Add(start);
        break;
      }
    }

    // We need to check if the "line" from prev to current will pass on the merged edge. If not we need to add a point on the edge
    res.Reverse();
    return res;
  }

  PathNode GetMinPath(List<PathNode> list) {
    float min = float.MaxValue;
    PathNode res = null;
    foreach (PathNode p in list)
      if (p.g + p.h < min) {
        res = p;
        min = p.g + p.h;
      }
    return res;
  }

  private Vector2 FindIntersection(Vector2 p1s, Vector2 p1e, Vector2 p2s, Vector2 p2e) {
    // Get the segments' parameters.
    float dx12 = p1e.x - p1s.x;
    float dy12 = p1e.y - p1s.y;
    float dx34 = p2e.x - p2s.x;
    float dy34 = p2e.y - p2s.y;

    // Solve for t1 and t2
    float denominator = (dy12 * dx34 - dx12 * dy34);

    float t1 = ((p1s.x - p2s.x) * dy34 + (p2s.y - p1s.y) * dx34) / denominator;
    if (float.IsInfinity(t1)) {
      // The lines are parallel (or close enough to it).
      return new Vector2(float.NaN, float.NaN);
    }

    float t2 = ((p2s.x - p1s.x) * dy12 + (p1s.y - p2s.y) * dx12) / -denominator;

    bool segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));
    if (segments_intersect)
      return new Vector2(p1s.x + dx12 * t1, p1s.y + dy12 * t1); // Find the point of intersection.

    // Find the closest points on the segments.
    if (t2 < 0) {
      t2 = 0;
    }
    else if (t2 > 1) {
      t2 = 1;
    }
    return new Vector2(p2s.x + dx34 * t2, p2s.y + dy34 * t2);
  }

  Vector2 GetEdge(PathNode p1, PathNode p2, bool first) {
    if (p1.left == p2) {
      if (first) return p1.tl;
      return p1.bl;
    }
    if (p1.right == p2) {
      if (first) return p1.tr;
      return p1.br;
    }
    if (p1.top == p2) {
      if (first) return p1.tl;
      return p1.tr;
    }
    if (p1.down == p2) {
      if (first) return p1.bl;
      return p1.br;
    }
    return p1.tl;
  }


}


[CustomEditor(typeof(NavPath))]
public class NavPathEditor : Editor {
  NavPath t = null;
  float size;
  Vector3 snap;

  void OnEnable() {
    t = target as NavPath;
    size = HandleUtility.GetHandleSize(t.transform.position) * 0.02f;
    snap = Vector3.one * 0.75f;
  }


  public override void OnInspectorGUI() {

    // Nodes as list
    serializedObject.Update();
    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodes"));


    GUILayout.BeginHorizontal();
    // Button to fix nodes
    if (GUILayout.Button("Fix all nodes")) {
      if (t.nodes == null) t.nodes = new List<PathNode>();
      t.nodes.Clear();
      foreach(Transform tr in t.transform) {
        PathNode n = tr.GetComponent<PathNode>();
        if (n != null) t.nodes.Add(n);
      }
    }
    if (GUILayout.Button("Translate all nodes")) {
      Vector2 pos = t.transform.parent.position;
      foreach(PathNode n in t.nodes) {
        n.tl += pos;
        n.tr += pos;
        n.bl += pos;
        n.br += pos;
      }
    }
    GUILayout.EndHorizontal();

    // Toggle to show/hide nodes
    bool newval = GUILayout.Toggle(t.ShowSubNodes, "Show paths");
    if (newval != t.ShowSubNodes) {
      foreach (PathNode p in t.nodes)
        p.showMeshLocal = newval;
      t.ShowSubNodes = newval;
    }

    // Toggle to show/hide path and path handlers
    newval = GUILayout.Toggle(t.DoAStar, "A*");
    if (newval != t.DoAStar) {
      t.DoAStar = newval;
    }

    GUILayout.BeginHorizontal();
    // A*
    if (GUILayout.Button("Calculate A*")) {
      t.DoAStar = true;
      t.EditorCalculatePath();
    }
    if (GUILayout.Button("Clean A*")) {
      t.DoAStar = false;
      t.start = t.transform.parent.transform.position + Vector3.right;
      t.end = t.transform.parent.transform.position + Vector3.left;
      t.gizmoLines.Clear();
    }
    GUILayout.EndHorizontal();

  }

  void OnSceneGUI() {
    Handles.color = Color.cyan;

    if (!t.DoAStar) return;
    EditorGUI.BeginChangeCheck();
    Vector2 nps = Handles.FreeMoveHandle(t.start, Quaternion.identity, size, snap, Handles.RectangleHandleCap);
    Vector2 npe = Handles.FreeMoveHandle(t.end, Quaternion.identity, size, snap, Handles.RectangleHandleCap);
    if (EditorGUI.EndChangeCheck()) {
      Undo.RecordObject(t, "AStar");
      t.start = nps;
      t.end = npe;
    }
  }

}