using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
public class EdgesToObstacles : MonoBehaviour
{
  [SerializeField] EdgeFinder finder;
  // Start is called before the first frame update
  void Awake()
  {
    NavMeshEdges edges = finder.GetEdges();
    List<List<Vector3>> v3Lists = finder.ToPolygons(edges);
    List<List<RVO.Vector2>> v2Lists = finder.ToRVOObstacles(v3Lists);
    Debug.Log("# of obstacles:" + v2Lists.Count);
    foreach (var item in v2Lists)
    {
      // item.Reverse();
      Simulator.Instance.addObstacle(item);
    }
    Simulator.Instance.processObstacles();
  }

  // Update is called once per frame
  void Update()
  {

  }
}
