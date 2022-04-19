using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSimulationUpdater : MonoBehaviour
{
  [SerializeField] BoidsParameters parameters;

  [SerializeField] GameObject prefab;

  [SerializeField] int num = 20;

  [SerializeField] Transform goal;
  [SerializeField] Vector3 SpawnRange;
  private void Start()
  {
    BoidsSimulation.Instance.SetParameters(parameters);
    for (int i = 0; i < num; i++)
    {
      GameObject.Instantiate(prefab, new Vector3(Random.Range(-SpawnRange.x, SpawnRange.x), Random.Range(-SpawnRange.y, SpawnRange.y), 0), Quaternion.identity);
    }
  }

  // Update is called once per frame
  void Update()
  {
    parameters.Goal = goal.position;
    BoidsSimulation.Instance.SetParameters(parameters);
    BoidsSimulation.Instance.UpdateBoids(Time.deltaTime);
  }

}
