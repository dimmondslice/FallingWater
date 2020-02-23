using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridComp : MonoBehaviour
{
  public int m_gridWidth;
  public int m_gridHeight;
  public float m_DistApart;
  public ETileOrientation m_tileOrientation;

  public GameObject m_navVertPrefab;
  public GameObject m_hexPrefab;

  public enum ETileOrientation
  {
    vertical,
    horizontal
  }

  public void Generate()
  {
    //kill all the children
    List<Transform> kiddos = new List<Transform>();
    GetComponentsInChildren<Transform>(true /*includeInactive*/, kiddos);
    foreach (Transform rChild in kiddos)
    {
      if (rChild && rChild != transform)
      {
        DestroyImmediate(rChild.gameObject);
      }
    }

    //Start tile placement such that hexgrid transform is the top center of the grid
    float startingXPos = -1.0f * m_DistApart * (int)(m_gridWidth / 2) + ((m_gridWidth + 1) % 2 ) * m_DistApart / 2.0f;
    Vector3 currentSpawnPos = new Vector3(startingXPos, 0.0f, 0.0f);
    Quaternion spawnRot = m_tileOrientation == ETileOrientation.vertical ? Quaternion.identity : Quaternion.Euler(0, 0, 30);
    float hexTriangleHeight = (m_DistApart / 2.0f) * Mathf.Sqrt(3);
    for (int i = 0; i < m_gridHeight; ++i)
    {
      int oddRowMinusOne = i % 2;
      for (int j = 0; j < m_gridWidth - oddRowMinusOne; ++j)
      {
        //spawn hex and top/bottom NavVerts
        GameObject rNewHex = Instantiate(m_hexPrefab, transform);
        rNewHex.transform.localPosition = currentSpawnPos;
        rNewHex.transform.localRotation = spawnRot;

        GameObject rTopNavVert = Instantiate(m_navVertPrefab, transform);
        rTopNavVert.transform.localPosition = currentSpawnPos + new Vector3(0, (2.0f/3.0f) * hexTriangleHeight, 0); //new Vector3(0, (hexTriangleHeight) / 2.0f, 0);
        rTopNavVert.layer = 12;

        GameObject rBottomNavVert = Instantiate(m_navVertPrefab, transform);
        rBottomNavVert.transform.localPosition = currentSpawnPos - new Vector3(0, (2.0f / 3.0f) * hexTriangleHeight, 0);//new Vector3(0, (hexTriangleHeight) / 2.0f, 0);
        rBottomNavVert.layer = 13;

        //set spawn location for next tile in this row
        if (m_tileOrientation == ETileOrientation.vertical)
        {
          currentSpawnPos.x += m_DistApart;
        }
        else if (m_tileOrientation == ETileOrientation.horizontal)
        {
          currentSpawnPos.x += m_DistApart * Mathf.Sqrt(3);
        }
      }

      //update spawn location every row
      if (m_tileOrientation == ETileOrientation.vertical)
      {
        currentSpawnPos.y -= hexTriangleHeight;
        currentSpawnPos.x = startingXPos + ((i + 1) % 2) * (m_DistApart / 2.0f); //offset every other row
      }
      else if (m_tileOrientation == ETileOrientation.horizontal)
      {
        currentSpawnPos.y -= (m_DistApart / 2.0f);
        currentSpawnPos.x = startingXPos + ((i + 1) % 2) * hexTriangleHeight; //offset every other row
      }
    }
  }

  public void GenerateNavGraph()
  {

  }
}
