using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileComp : MonoBehaviour
{
  public enum ERotateDir
  {
    eRight,
    eLeft,
  }
  public void RotateTile(ERotateDir dir)
  {
    int sign = dir == ERotateDir.eRight ? 1 : -1;
    transform.Rotate(-Vector3.forward, 60.0f * sign, Space.Self);
    //transform.RotateAround(transform.forward, 60.0f * sign);
  }
}
