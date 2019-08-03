﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerComp : MonoBehaviour
{
  void Update()
  {

    bool leftClickPressed = Input.GetMouseButtonUp(0);
    bool rightClickPressed = Input.GetMouseButtonUp(1);
    bool leftClickHeld = Input.GetMouseButton(0);
    bool rightClickHeld = Input.GetMouseButton(1);

    bool bothPressed = (leftClickPressed && rightClickHeld) || (rightClickPressed && leftClickHeld);

    if (leftClickPressed || rightClickPressed) //always do ray if any mouse was clicked
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
      RaycastHit[] hits = new RaycastHit[5];
      Physics.RaycastNonAlloc(ray, hits);

      for (int i = 0; i < 5; i++)
      {
        if (hits[i].transform)
        {
          HexTileComp hex = hits[i].transform.GetComponentInParent<HexTileComp>();
          if (hex)
          {
            //resolve for doubleclick first
            if (bothPressed)
            {
              hex.FlipTile();
            }
            else
            {
              HexTileComp.ERotateDir dir = leftClickPressed ? HexTileComp.ERotateDir.eLeft : HexTileComp.ERotateDir.eRight;
              hex.RotateTile(dir);
            }

            break;
          }
        }
      }
    }
  }
}
