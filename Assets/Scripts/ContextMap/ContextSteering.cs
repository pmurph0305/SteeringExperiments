using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextSteering : MonoBehaviour
{
  public float Speed = 5f;
  public Transform goal;

  public Transform[] goals;

  [SerializeField] float[] InterestContext = new float[8];

  public float MinDangerRange = 5f;
  public float MinCollisionRange = 5f;
  [SerializeField] float[] DangerContext = new float[8];

  [SerializeField] float[] CollisionContext = new float[8];

  [SerializeField] float[] CombinedContext = new float[8];


  Vector3 basisDirection = Vector3.right;
  Vector3 axisDirection = Vector3.up;



  [Header("Debug Visualization")]
  public float SeekLength = 10;

  // Start is called before the first frame update
  void Start()
  {

  }

  Vector3 MovementDirection;
  public Vector3 GetMovementDirection()
  {
    return MovementDirection;
  }
  private void Update()
  {
    CalculateInterestContext();
    CalculateDangerContext();
    CalculateCollisionContext();

    Copy(CombinedContext, InterestContext);
    MaskNonMiminums(CombinedContext, DangerContext);
    Subtract(CombinedContext, CollisionContext);
    Vector3 scaledDirection = CalculateScaledMovementVector(CombinedContext);
    // movement is steered over time.
    // MovementDirection += scaledDirection;
    // MovementDirection = Vector3.ClampMagnitude(MovementDirection, Speed);

    // context informs movement instantaneously
    // without normalization, its veyr slow when the goal is far awway, but you want a more consistent speed.
    MovementDirection = scaledDirection.normalized * Speed;
    transform.position += MovementDirection * Time.deltaTime;
  }

  [SerializeField] Vector3[] neighbours;
  Vector3 CalculateScaledMovementVector(float[] context)
  {
    int maxIndex = GetMaxIndex(context);
    if (maxIndex == -1)
    {
      Debug.Log("No good direction.", this.gameObject);
      return Vector3.zero;
    }
    Vector3 primary = ToScaledWorldDirection(maxIndex, context);
    // return primary;
    neighbours = new Vector3[4];
    GetNeighbours(maxIndex, context, neighbours);

    for (int i = 0; i < neighbours.Length; i++)
    {
      primary += neighbours[i];
    }

    return primary / (neighbours.Length + 1);
    // return primary;
  }

  void GetNeighbours(int index, float[] context, Vector3[] array)
  {
    int num = array.Length / 2;
    for (int i = 0; i < array.Length / 2; i++)
    {
      int i1 = (index + i) % (array.Length);
      int i2 = (index - i) % (array.Length);
      i2 = i2 < 0 ? i2 + array.Length : i2;
      if (i1 < 0 || i1 >= array.Length)
      {
        Debug.LogError("Error:" + index + " i1:" + i1 + " len:" + array.Length + " i:" + i);
      }
      if (i2 < 0 || i2 >= array.Length)
      {
        Debug.LogError("Error:" + index + " i2:" + i2 + " len:" + array.Length + " i:" + i);
      }
      if (context[i1] > 0)
      {
        array[i * 2] = ToScaledWorldDirection(i1, context);
      }
      if (context[i2] > 0)
      {
        array[i * 2 + 1] = ToScaledWorldDirection(i2, context);
      }
    }
  }

  /// <summary>
  /// scales the world direction vector at the index by the amount in the context map for that index.
  /// </summary>
  /// <param name="index"></param>
  /// <param name="context"></param>
  /// <returns></returns>
  Vector3 ToScaledWorldDirection(int index, float[] context)
  {
    return ToWorldDirection(index) * context[index];
  }
  Vector3 ToWorldDirection(int index)
  {
    return Quaternion.AngleAxis(index * 360f / 8f, axisDirection) * basisDirection;
  }

  int ToContextMapSlot(Vector3 worldDirection)
  {
    float angle = Vector3.SignedAngle(basisDirection, worldDirection, axisDirection);
    float a = Mathf.Abs(angle);
    int index = Mathf.RoundToInt(1f * a / (360f / InterestContext.Length));
    if (angle < 0 && index > 0 && index < InterestContext.Length / 2)
    {
      index = InterestContext.Length - index;
    }
    return index;
  }


  float CalculateFalloffInterest(int from, int to, int length, float amount)
  {
    if (from == to) return amount;
    int distance = CalculateDistance(from, to, length);
    // linear falloff.
    float val = (length / 2f - distance) / (length / 2f) * amount;
    val = val > 0 ? val : 0;
    if (float.IsNaN(val))
    {
      Debug.LogError("NAN:" + from + ":" + to + ":" + length + ":" + amount);
    }
    return val;
  }

  float CalculateFalloffDanger(int from, int to, int length, float amount)
  {
    return CalculateFalloffInterest(from, to, length, amount);
    // if (from == to) return amount;
    // int distance = CalculateDistance(from, to, length);
    // // inv sq falloff
    // float val = 1f / ((distance + 1) * (distance + 1)) * amount;
    // return val > 0 ? val : 0;
  }

  int CalculateDistance(int from, int to, int length)
  {
    if (from == to) return 0;
    int distance = Mathf.Abs(to - from);
    // wrap around
    if (distance > length / 2)
    {
      distance = length - distance;
    }
    return distance;
  }



  void Copy(float[] copyInto, float[] copyFrom)
  {
    for (int i = 0; i < copyFrom.Length; i++)
    {
      copyInto[i] = copyFrom[i];
    }
  }
  void Clear(float[] array)
  {
    for (int i = 0; i < array.Length; i++)
    {
      array[i] = 0;
    }
  }

  void Normalize(float[] array)
  {
    float max = GetMax(array);
    if (max <= 0) return;
    for (int i = 0; i < array.Length; i++)
    {
      array[i] = array[i] / max;
    }
  }


  float CalculateFalloffAngled(int from, int to, int length, float amount, Vector3 direction)
  {

    int distance = CalculateDistance(from, to, length);
    // linear falloff.
    float val = (length / 2f - distance) / (length / 2f) * amount;
    // esssentially we are making the falloff based on the angle form the actual world direction of the index, and the direction the goal is towards.
    float angle = Vector3.Angle(ToWorldDirection(to), direction);
    // 180f is the furthest away a direction can be angle-wise
    float m = (180f - angle) / 180f;
    val *= m;
    val = val > 0 ? val : 0;
    // Debug.Log("From:" + from + " To:" + to + " M:" + m + " Angle:" + angle + " Val:" + val);
    if (float.IsNaN(val))
    {
      Debug.LogError("NAN:" + from + ":" + to + ":" + length + ":" + amount);
    }
    return val;

  }

  void CalculateInterestContext()
  {
    Clear(InterestContext);
    foreach (var g in goals)
    {
      Vector3 directionToGoal = g.position - transform.position;
      float distance = Vector3.Distance(g.position, transform.position);
      // we want to invert the distance, so things closer are MORE interesting.
      float interest = 1 / distance;
      interest = Mathf.Clamp01(interest);
      int slot = ToContextMapSlot(directionToGoal);

      for (int i = 0; i < InterestContext.Length; i++)
      {
        // float amount = CalculateFalloffInterest(slot, i, InterestContext.Length, interest);
        float amount = CalculateFalloffAngled(slot, i, InterestContext.Length, interest, directionToGoal);
        InterestContext[i] = InterestContext[i] > amount ? InterestContext[i] : amount;
      }
    }
  }

  void CalculateCollisionContext()
  {
    Clear(CollisionContext);
    var others = GameObject.FindObjectsOfType<ContextSteering>();
    collisionCount = 0;
    collisionPos.Clear();
    collisionWith.Clear();
    withMovementDir.Clear();
    collisionDistances.Clear();
    movementDir = MovementDirection;
    foreach (var item in others)
    {
      if (item == this) continue;
      float angleToObject = Vector3.Angle(MovementDirection, item.transform.position - transform.position);
      if (angleToObject > 45f) continue;
      Vector3 collision = Vector3.zero;
      if (!WillCollide(transform.position, MovementDirection, item.transform.position, item.GetMovementDirection(), out collision)) continue;
      Vector3 directionToCollision = collision - transform.position;
      if (Vector3.Angle(MovementDirection, directionToCollision) > 30f) return;
      float distance = Vector3.Distance(collision, transform.position);
      if (distance > MinCollisionRange) continue;
      collisionCount++;
      collisionPos.Add(collision);
      collisionWith.Add(item.transform.position);
      withMovementDir.Add(item.GetMovementDirection());
      collisionDistances.Add(distance);
      int slot = ToContextMapSlot(directionToCollision);
      float danger = 1f / distance;
      danger = Mathf.Clamp01(danger);
      for (int i = 0; i < CollisionContext.Length; i++)
      {
        float amount = CalculateFalloffDanger(slot, i, CollisionContext.Length, danger);
        CollisionContext[i] = CollisionContext[i] > amount ? CollisionContext[i] : amount;
      }
    }
  }

  void CalculateDangerContext()
  {
    Clear(DangerContext);
    var others = GameObject.FindObjectsOfType<ContextSteering>();
    foreach (var item in others)
    {
      if (item == this) continue;
      float distance = Vector3.Distance(item.transform.position, transform.position);
      if (distance > MinDangerRange) continue;
      Vector3 directionToOther = item.transform.position - transform.position;
      int slot = ToContextMapSlot(directionToOther);
      float danger = 1 / distance;
      danger = Mathf.Clamp01(danger);
      for (int i = 0; i < DangerContext.Length; i++)
      {
        float amount = CalculateFalloffDanger(slot, i, DangerContext.Length, danger);
        DangerContext[i] = DangerContext[i] > amount ? DangerContext[i] : amount;
      }
    }
  }

  [SerializeField] int collisionCount;
  Vector3 movementDir;
  [SerializeField] List<Vector3> collisionPos = new List<Vector3>();
  [SerializeField] List<Vector3> collisionWith = new List<Vector3>();
  [SerializeField] List<Vector3> withMovementDir = new List<Vector3>();
  [SerializeField] List<float> collisionDistances = new List<float>();

  bool WillCollide(Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2, out Vector3 collision)
  {
    // // ax +by = c.
    // Vector3 p1b = p1 + v1;
    // Vector3 p2b = p2 + v2;

    // //l1 ax+by=c

    // //l2 ax+by=c

    // float a1 = p1b.z - p1.z;
    // float b1 = p1.x - p1b.x;
    // float c1 = a1 * p1.x + b1 * p1.z;

    // float a2 = p2b.z - p2.z;
    // float b2 = p2.x - p2b.x;
    // float c2 = a1 * p2.x + b2 * p2.z;

    // float det = a1 * b2 - a2 * b1;
    // if (det == 0)
    // {
    //   collision = Vector3.zero;
    //   return false;
    // }
    // else
    // {
    //   float x = (b2 * c1 - b1 * c2) / det;
    //   float z = (a1 * c2 - a2 * c1) / det;
    //   collision = new Vector3(x, 0, z);
    //   return true;
    // }

    // intersection for y=mx+b lines
    // ax +c = bx + d
    float a = v1.z / v1.x;
    float c = p1.z - p1.x * a;


    float b = v2.z / v2.x;
    float d = p2.z - p2.x * b;
    if (a == b)
    {
      collision = Vector3.zero;
      return false;
    }

    float x = (d - c) / (a - b);
    float y = a * x + c;
    if (float.IsNaN(x) || float.IsNaN(y))
    {
      collision = Vector3.zero;
      return false;
    }
    collision = new Vector3(x, 0, y);
    return true;

  }


  void Subtract(float[] map, float[] subtract)
  {
    if (map.Length != subtract.Length) throw new System.Exception("Maps are not the same length.");
    for (int i = 0; i < map.Length; i++)
    {
      map[i] -= subtract[i];
    }
  }

  /// <summary>
  /// Masks the map. Making values -1 in the map if the same index in the mask is larger than the minimum value in the mask.
  /// </summary>
  /// <param name="map">Map to modify</param>
  /// <param name="mask">Map to get the minimum of and use as a mask</param>
  void MaskNonMiminums(float[] map, float[] mask)
  {
    if (map.Length != mask.Length) { Debug.LogError("Danger and interest map are not the same length."); }
    float minDanger = GetMin(mask);
    // mask out anything above min danger.
    for (int i = 0; i < mask.Length; i++)
    {
      // above the min danger, and the danger is closer than the interest.
      if (mask[i] > minDanger) //&& interest[i] < danger[i])
      {
        map[i] = -1;
      }
    }
  }



  public float GetMin(float[] context)
  {
    float min = Mathf.Infinity;
    for (int i = 0; i < context.Length; i++)
    {
      min = context[i] < min ? context[i] : min;
    }
    return min;
  }
  public float GetMax(float[] context)
  {
    float max = -Mathf.Infinity;
    for (int i = 0; i < context.Length; i++)
    {
      max = context[i] > max ? context[i] : max;
    }
    return max;
  }

  public int GetMaxIndex(float[] context)
  {
    float max = -Mathf.Infinity;
    int index = -1;
    for (int i = 0; i < context.Length; i++)
    {
      if (context[i] > max && context[i] >= 0)
      {
        max = context[i];
        index = i;
      }
    }
    return index;
  }





  public void DrawContextMap(float[] map, float drawLength, Vector3 offset)
  {
    float max = GetMax(map);
    for (int i = 0; i < map.Length; i++)
    {
      if (map[i] < 0) continue;
      float length = map[i] / max * drawLength;
      Gizmos.DrawLine(transform.position + offset, transform.position + offset + ToWorldDirection(i) * length);
    }
  }

  private void OnDrawGizmosSelected()
  {
    // CalculateInterestContext();
    // Gizmos.color = Color.white;
    // DrawContextMap(InterestContext, SeekLength, Vector3.zero);

    // // CalculateDangerContext();
    // Gizmos.color = Color.black;
    // DrawContextMap(DangerContext, SeekLength, Vector3.up);

    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + movementDir);
    for (int i = 0; i < collisionWith.Count; i++)
    {
      Vector3 collision = collisionPos[i];
      Vector3 withPos = collisionWith[i];
      Gizmos.color = Color.cyan;
      Gizmos.DrawLine(transform.position, collision);
      Gizmos.color = Color.magenta;
      Gizmos.DrawLine(withPos, collision);
      Gizmos.color = Color.red;
      Gizmos.DrawLine(collision, collision + Vector3.up);
      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(withPos, withPos + withMovementDir[i]);
    }
  }

  private void OnDrawGizmos()
  {
    // CalculateInterestContext();
    // CalculateDangerContext();


    // MaskDanger(InterestContext, DangerContext);
    // Gizmos.color = Color.blue;
    // DrawContextMap(InterestContext, SeekLength, Vector3.up * 2);

    // Vector3 scaledDirection = CalculateScaledMovementVector(InterestContext);
    // MovementDirection = scaledDirection.normalized * Speed; //m/s
    // Gizmos.color = Color.green;
    // Gizmos.DrawLine(transform.position, transform.position + MovementDirection);
  }
}
