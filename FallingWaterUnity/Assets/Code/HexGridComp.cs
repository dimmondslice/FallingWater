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
    //create container children
    GameObject emptyGOTemplate = new GameObject();
    Transform rTiles = transform.Find("Tiles");
    if (!rTiles)
    {
      GameObject tilesGO = Instantiate<GameObject>(emptyGOTemplate, transform);
      tilesGO.name = "Tiles";
      rTiles = tilesGO.transform;
    }

    List<GameObject> gridObjectsToDestroy = new List<GameObject>();

    //Start tile placement such that hexgrid transform is the top center of the grid
    float startingXPos = -1.0f * m_DistApart * (int)(m_gridWidth / 2) + ((m_gridWidth + 1) % 2 ) * m_DistApart / 2.0f;
    Vector3 currentSpawnPos = new Vector3(startingXPos, 0.0f, 0.0f);
    Quaternion spawnRot = m_tileOrientation == ETileOrientation.vertical ? Quaternion.identity : Quaternion.Euler(0, 0, 30);
    float hexTriangleHeight = (m_DistApart / 2.0f) * Mathf.Sqrt(3);

    //for each row
    for (int i = 0; i < m_gridHeight; ++i)
    {
      //find/create empty gameobject to contain this row
      Transform rRow_i_trans;
      int ogRowChildCount = 0;
      if (i < rTiles.childCount)
      {
        rRow_i_trans = rTiles.GetChild(i);
        if (rRow_i_trans)
        {
          ogRowChildCount = rRow_i_trans.childCount;
        }
      }
      else
      {
        GameObject row_i_GO = Instantiate<GameObject>(emptyGOTemplate, rTiles);
        row_i_GO.name = "row_" + i;
        rRow_i_trans = row_i_GO.transform;
      }

      //foreach hex in row
      int oddRowMinusOne = i % 2;
      int numHexes = Mathf.Max(m_gridWidth - oddRowMinusOne, ogRowChildCount);
      for (int j = 0; j < numHexes; ++j)
      {
        //don't stomp already existing tiles, only spawn new ones after iterating over existing ones
        if (j >= ogRowChildCount)
        {
          //spawn new hex
          GameObject rNewHex = Instantiate(m_hexPrefab, rRow_i_trans);
          rNewHex.transform.localPosition = currentSpawnPos;
          rNewHex.transform.localRotation = spawnRot;
        }
        else if(j < m_gridWidth - oddRowMinusOne)
        {
          Transform rAlreadyExistingHex = rRow_i_trans.GetChild(j);
          rAlreadyExistingHex.localPosition = currentSpawnPos; 
        }
        else
        {
          gridObjectsToDestroy.Add(rRow_i_trans.GetChild(j).gameObject);
        }

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

    //kill the excess rows from previous generation
    for (int i = m_gridHeight; i < rTiles.childCount; ++i)
    {
      Transform rChild = rTiles.GetChild(i);
      if(rChild)
      {
        gridObjectsToDestroy.Add(rChild.gameObject);
      }
    }

    //kill them all
    foreach(GameObject obj in gridObjectsToDestroy)
    {
      DestroyImmediate(obj);
    }

    GenerateNavGraph();
    DestroyImmediate(emptyGOTemplate);
  }

  public void GenerateNavGraph()
  {
    Transform rNavGraph = transform.Find("NavGraph");
    if (rNavGraph)
    {
      DestroyImmediate(rNavGraph.gameObject);
    }
    GameObject emptyGOTemplate = new GameObject();
    GameObject navGraphGO = Instantiate<GameObject>(emptyGOTemplate, transform);
    navGraphGO.name = "NavGraph";
    rNavGraph = navGraphGO.transform;
    DestroyImmediate(emptyGOTemplate);

    //generate navVerts
    float hexTriangleHeight = (m_DistApart / 2.0f) * Mathf.Sqrt(3);
    HexTileComp[] hexes = transform.GetComponentsInChildren<HexTileComp>(false); 
    foreach(HexTileComp hex in hexes)
    {
      GameObject rTopNavVert = Instantiate(m_navVertPrefab, rNavGraph);
      rTopNavVert.transform.localPosition = hex.transform.localPosition + new Vector3(0, (2.0f / 3.0f) * hexTriangleHeight, 0); //new Vector3(0, (hexTriangleHeight) / 2.0f, 0);
      rTopNavVert.layer = 12;

      GameObject rBottomNavVert = Instantiate(m_navVertPrefab, rNavGraph);
      rBottomNavVert.transform.localPosition = hex.transform.localPosition - new Vector3(0, (2.0f / 3.0f) * hexTriangleHeight, 0);//new Vector3(0, (hexTriangleHeight) / 2.0f, 0);
      rBottomNavVert.layer = 13;
    }

    //add one more final row, but only the top nav points, so bugs can ai properly at the bottom
    int rowWidth = m_gridWidth - (m_gridHeight % 2);
    float startingXPos = -1.0f * m_DistApart * (int)(rowWidth / 2) + ((rowWidth + 1) % 2) * m_DistApart / 2.0f;
    Vector3 currentSpawnPos = (-Vector3.up * m_gridHeight * hexTriangleHeight) + (Vector3.right * startingXPos);

    for (int i = 0; i < rowWidth; ++i)
    {
      GameObject rTopNavVert = Instantiate(m_navVertPrefab, rNavGraph);
      rTopNavVert.transform.localPosition = currentSpawnPos + new Vector3(0, (2.0f / 3.0f) * hexTriangleHeight, 0);
      rTopNavVert.layer = 12;

      //set spawn location for next Nav Vert in this row
      if (m_tileOrientation == ETileOrientation.vertical)
      {
        currentSpawnPos.x += m_DistApart;
      }
      else if (m_tileOrientation == ETileOrientation.horizontal)
      {
        currentSpawnPos.x += m_DistApart * Mathf.Sqrt(3);
      }
    }
  }
}
