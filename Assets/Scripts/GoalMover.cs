using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalMover : MonoBehaviour
{

  // Start is called before the first frame update
  void Start()
  {
    SetNewMovementDirection();
  }
  [SerializeField] float Speed = 5f;
  [SerializeField] float MovementTime = 5f;

  Vector3 movementDirection = Vector3.zero;
  float t = 0.0f;

  [SerializeField] Bounds b;
  // Update is called once per frame
  void Update()
  {
    t += Time.deltaTime;
    if (t > MovementTime)
    {
      t -= MovementTime;
      SetNewMovementDirection();
    }
    Vector3 expectedPosition = transform.position + movementDirection * Speed * Time.deltaTime;
    if (!b.Contains(expectedPosition))
    {
      expectedPosition = b.ClosestPoint(expectedPosition);
    }
    transform.position = expectedPosition;
  }

  public void SetNewMovementDirection()
  {
    movementDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
  }
}
