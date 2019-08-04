using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerComp : MonoBehaviour
{
  public int m_Health;
  public Transform m_levelLerpToLocation;
  public Transform m_root;
  public GameObject[] m_levels;
  public GameObject m_nextLevelButton;



  private int m_currentLevelIndex;
  private Transform m_currentSpawnedLevel;
  private Vector3 m_initalLevelPos;

  private void Start()
  {
    m_currentLevelIndex = 0;
    m_currentSpawnedLevel = m_levels[m_currentLevelIndex].transform;
    m_initalLevelPos = m_currentSpawnedLevel.position;
  }
  void Update()
  {
    //level done yet?
    CheckForLevelComplete();

    //all the input in the game basically
    ProcessPlayerInput();
  }

  void CheckForLevelComplete()
  {
    if(BugEnemyComp.s_totalEnemiesAlive <= 0 && EnemySpawnManagerComp.s_done )
    {
      m_nextLevelButton.SetActive(true);
    }
  }

  void ProcessPlayerInput()
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

  private void OnTriggerEnter(Collider other)
  {
    if(other.gameObject.layer == 10) //bug
    {
      BugEnemyComp bug = other.GetComponentInParent<BugEnemyComp>();
      if (bug)
      {
        bug.Kill();
      }

      m_Health-= 5;
      if(m_Health <= 0)
      {
        print("gameover sad");
      }
    }
  }

  public void QuitGame()
  {
    Application.Quit();
  }

  public void BeginGame()
  {
    LoadNextLevel();
  }

  public void LoadNextLevel()
  {
    StartCoroutine("FlyOutLevel");
  }

  IEnumerator FlyOutLevel()
  {
    while (Vector3.Distance(m_currentSpawnedLevel.position, m_levelLerpToLocation.position) > 4f)
    {
      m_currentSpawnedLevel.position = Vector3.Lerp(m_currentSpawnedLevel.position, m_levelLerpToLocation.position, Time.deltaTime * 2);
      yield return null;
    }

    Destroy(m_currentSpawnedLevel.gameObject);

    m_currentLevelIndex++;
    GameObject nextLevel = Instantiate(m_levels[m_currentLevelIndex], m_levelLerpToLocation.position, m_root.rotation, m_root);
    m_currentSpawnedLevel = nextLevel.transform;
    StartCoroutine("FlyInLevel");
  }
  IEnumerator FlyInLevel()
  {
    while (Vector3.Distance(m_currentSpawnedLevel.position, m_initalLevelPos) > .3f)
    {
      m_currentSpawnedLevel.position = Vector3.Lerp(m_currentSpawnedLevel.position, m_initalLevelPos, Time.deltaTime * 4);
      yield return null;
    }
    m_currentSpawnedLevel.position = m_initalLevelPos;
    StartCurrentLevel();
  }
  void StartCurrentLevel()
  {

  }
}
