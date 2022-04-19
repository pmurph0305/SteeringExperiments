using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System;
using System.Linq;
using RVO;
public class EdgeFinder : MonoBehaviour
{


  void Awake()
  {
    List<List<RVO.Vector2>> v2Lists = GetObstaclesFromNavMesh();
    Debug.Log("# of obstacles:" + v2Lists.Count);
    foreach (var item in v2Lists)
    {
      // item.Reverse();
      Simulator.Instance.addObstacle(item);
    }
    Simulator.Instance.processObstacles();
  }

  private void OnDrawGizmosSelected()
  {
    DrawEdges();
  }

  /// <summary>
  /// Gets the RVO2 ready polygons for the nav mesh.
  /// </summary>
  /// <returns></returns>
  public List<List<RVO.Vector2>> GetObstaclesFromNavMesh()
  {
    // NavMeshEdges edges = GetEdges();
    // List<List<Vector3>> v3s = ToPolygons(edges);
    // List<List<RVO.Vector2>> v2s = ToRVOObstacles(v3s);
    // return v2s;
    return ToRVOObstacles(ToPolygons(GetEdges(0)));
  }

  /// <summary>
  /// Gets the edges by using the first "Area" layer of a navmesh, by default this is the "Walkable" layer.
  /// </summary>
  /// <returns></returns>
  private NavMeshEdges GetEdges(int baseWalkableAreaLayer)
  {
    NavMeshTriangulation nmt = NavMesh.CalculateTriangulation();
    HashSet<Edge> allEdges = new HashSet<Edge>();
    HashSet<Edge> duplicatedEdges = new HashSet<Edge>();
    for (int i = 0; i < nmt.indices.Length; i += 3)
    {
      if (nmt.areas[i / 3] != baseWalkableAreaLayer) continue;
      int i0 = nmt.indices[i];
      int i1 = nmt.indices[i + 1];
      int i2 = nmt.indices[i + 2];
      Vector3 v0 = nmt.vertices[i0];
      Vector3 v1 = nmt.vertices[i1];
      Vector3 v2 = nmt.vertices[i2];
      Edge e0 = new Edge(v0, v1, i0, i1);
      Edge e1 = new Edge(v1, v2, i1, i2);
      Edge e2 = new Edge(v2, v0, i2, i0);

      if (!allEdges.Add(e0))
      {
        duplicatedEdges.Add(e0);
      }
      if (!allEdges.Add(e1))
      {
        duplicatedEdges.Add(e1);
      }
      if (!allEdges.Add(e2))
      {
        duplicatedEdges.Add(e2);
      }
    }
    // remove duplicated edges to make them the unique edges.
    allEdges.ExceptWith(duplicatedEdges);
    NavMeshEdges edges = new NavMeshEdges(allEdges, duplicatedEdges);
    return edges;
  }

  float epsilon = 0.0001f;
  private bool Approx(Vector3 v1, Vector3 v2)
  {

    return (Mathf.Abs(v1.x - v2.x) < epsilon) && (Mathf.Abs(v1.y - v2.y) < epsilon) && (Mathf.Abs(v1.z - v2.z) < epsilon);
  }

  private List<List<RVO.Vector2>> ToRVOObstacles(List<List<Vector3>> v3Lists)
  {
    List<List<RVO.Vector2>> rvoLists = new List<List<RVO.Vector2>>();

    foreach (var list in v3Lists)
    {
      List<RVO.Vector2> rvoList = new List<RVO.Vector2>();
      foreach (Vector3 v in list)
      {
        rvoList.Add(new RVO.Vector2(v.x, v.z));
      }
      rvoLists.Add(rvoList);
    }
    return rvoLists;
  }
  private bool isLeft(Vector3 a, Vector3 b, Vector3 c)
  {
    return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
  }


  /// <summary>
  /// Turns edges of a nav mesh into polygons that can be used by RVO2 as obstacles. 
  /// The outer edges of the nav mesh should correctly be oriented in a CW order so that it is an inverted obstacle, while normal obstacles are in CCW order.
  /// Known issues: Can cause problems when using a NavMesh that has multiple NavMeshSurfaces, as there's no way to distinguish them
  /// Possible solution: use a single area layer like "Walkable" for finding the edges, and use a different layer
  /// for the actual agent pathfinding.
  /// </summary>
  /// <param name="navEdges"></param>
  /// <returns></returns>
  private List<List<Vector3>> ToPolygons(NavMeshEdges navEdges)
  {
    List<List<Vector3>> VerticesLists = new List<List<Vector3>>();
    HashSet<Edge> uniqueEdges = new HashSet<Edge>(navEdges.UniqueEdges);
    HashSet<Edge> usedEdges = new HashSet<Edge>();
    foreach (var edge in uniqueEdges)
    {

      if (usedEdges.Contains(edge)) continue;
      usedEdges.Add(edge);
      List<Edge> connectedEdges = new List<Edge>();
      List<Vector3> VerticesList = new List<Vector3>();
      VerticesList.Add(edge.p0);
      VerticesList.Add(edge.p1);
      Vector3 vert = edge.p1;

      bool foundConnected = true;
      // essentially just while we continue to find pairing vertices, keep pairing them
      // by adding the vertex that isn't the same and use that as the comparison until no more are found.
      while (foundConnected)
      {
        foundConnected = false;
        foreach (var otherEdge in uniqueEdges)
        {
          if (usedEdges.Contains(otherEdge)) continue;
          if (Approx(otherEdge.p0, vert))
          {
            VerticesList.Add(otherEdge.p1);
            vert = otherEdge.p1;
            usedEdges.Add(otherEdge);
            foundConnected = true;
          }
          else if (Approx(otherEdge.p1, vert))
          {
            VerticesList.Add(otherEdge.p0);
            vert = otherEdge.p0;
            usedEdges.Add(otherEdge);
            foundConnected = true;
          }
        }
      }
      VerticesList.RemoveAt(VerticesList.Count - 1);
      VerticesLists.Add(VerticesList);
    }

    //CCW polygons.
    List<List<Vector3>> ccwVertices = new List<List<Vector3>>();
    foreach (var vertList in VerticesLists)
    {
      List<Vector3> ccwVerts = new List<Vector3>();
      int startIndex = 0;
      Vector3 c = vertList[0];
      for (int i = 0; i < vertList.Count; i++)
      {
        Vector3 n = vertList[i];
        if (n.z >= c.z)
        {
          if (n.x >= c.x)
          {
            c = n;
            startIndex = i;
          }
          else if (n.z > c.z)
          {
            c = n;
            startIndex = i;
          }
        }
        else if (Mathf.Abs(n.z - c.z) < epsilon && n.x >= c.x)
        {
          c = n;
          startIndex = i;
        }
      }
      List<Vector3> orderedVerts = new List<Vector3>();
      // easier debugging.
      orderedVerts.AddRange(vertList.GetRange(startIndex, vertList.Count - startIndex));
      orderedVerts.AddRange(vertList.GetRange(0, startIndex));
      int prev = orderedVerts.Count - 1;
      Vector3 previousVert = orderedVerts[orderedVerts.Count - 1];
      Vector3 nextVert = orderedVerts[1];
      // if (previousVert.x <= nextVert.x && previousVert.z >= nextVert.z)
      // {
      if (isLeft(nextVert, orderedVerts[0], previousVert))
      {
        orderedVerts.Reverse();
        // for debugging
        // foreach (var v in vertList)
        // {
        //   Debug.DrawLine(v, v + Vector3.up * 10f, Color.red, 1f);
        // }
      }
      // for debugging.
      // Debug.DrawLine(orderedVerts[0], orderedVerts[0] + Vector3.up, Color.black);
      // Debug.DrawLine(previousVert, previousVert + Vector3.up, Color.white);
      // Debug.DrawLine(nextVert, nextVert + Vector3.up, Color.cyan);
      ccwVertices.Add(orderedVerts);
    }


    //this works.
    int listToReverse = 0;
    // find bounding box set and reverse it so it's an inverted obstacle.
    foreach (var l in ccwVertices)
    {
      Bounds b = new Bounds(l[0], Vector3.zero);
      bool containsAll = true;
      foreach (var v in l)
      {
        b.Encapsulate(v);
      }
      Vector3 c = b.center;
      b.Encapsulate(c + Vector3.up * 10);
      b.Encapsulate(c - Vector3.up * 10);
      // b.Encapsulate(c - Vector3.up * 10);
      foreach (var ol in ccwVertices)
      {
        if (ol == l) { continue; }
        foreach (var ov in ol)
        {
          if (!b.Contains(ov))
          {
            containsAll = false;
            break;
          }
        }
        if (!containsAll)
        {
          break;
        }
      }

      if (containsAll)
      {
        listToReverse = ccwVertices.IndexOf(l);
        // Debug.DrawLine(b.center, b.center + b.extents, Color.green);
        // Debug.DrawLine(b.center, b.center - b.extents, Color.green);
        break;
      }
      // Debug.DrawLine(b.center, b.center + b.extents, Color.yellow);
      // Debug.DrawLine(b.center, b.center - b.extents, Color.yellow);
    }
    // Debug.Log("List to reverse:" + listToReverse);
    if (listToReverse < ccwVertices.Count)
    {
      ccwVertices[listToReverse].Reverse();
    }
    return ccwVertices;
  }


  void DrawEdges()
  {
    // triangulate, split up the edges into commona nd unique, and then turn the unique ones into polygons in CCW order (except an outer bounding box, which is in CW order so it's interior is not an obstacle)
    NavMeshTriangulation nmt = NavMesh.CalculateTriangulation();
    NavMeshEdges edges = GetEdges(0);
    List<List<Vector3>> lists = ToPolygons(edges);
    Color colorStart = Color.green;
    Color colorEnd = Color.red;
    Vector3 upset = Vector3.zero;
    foreach (var l in lists)
    {

      for (int i = 0; i < l.Count; i++)
      {
        Gizmos.color = Color.Lerp(colorStart, colorEnd, (float)i / (float)l.Count);
        Gizmos.DrawLine(l[i] + upset, l[i + 1 >= l.Count ? 0 : i + 1] + upset);
      }
    }

    // Draw non-shared edges.
    // Gizmos.color = Color.magenta;
    // foreach (var e in edges.UniqueEdges)
    // {
    //   Gizmos.DrawLine(e.p0, e.p1);
    // }

    // Gizmos.color = Color.cyan;
    // Vector3 offset = Vector3.up * 0.1f;
    // foreach (var e in edges.SharedEdges)
    // {
    //   Gizmos.DrawLine(e.p0 + offset, e.p1 + offset);
    // }
  }
}
