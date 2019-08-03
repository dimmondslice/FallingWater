using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NubComp : MonoBehaviour
{
  public void Start()
  {
    int onOff = Random.Range(0, 2);
    gameObject.SetActive(onOff == 0);

    HexTileComp hex = GetComponentInParent<HexTileComp>();
    if (hex)
    {
      int rotations = Random.Range(0, 6);
      hex.transform.Rotate(Vector3.forward, 60.0f * rotations, Space.Self);
    }
  }
}
