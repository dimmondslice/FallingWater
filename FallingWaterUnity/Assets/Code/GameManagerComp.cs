using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerComp : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {
        
  }

  // Update is called once per frame
  void Update()
  {
    bool leftClick = Input.GetMouseButtonDown(0);
    bool rightClick = Input.GetMouseButtonDown(1);
    if (leftClick || rightClick)
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
            HexTileComp.ERotateDir dir = leftClick ? HexTileComp.ERotateDir.eLeft : HexTileComp.ERotateDir.eRight;
            hex.RotateTile(dir);
            break;
          }
        }
      }
    }
  }
}
