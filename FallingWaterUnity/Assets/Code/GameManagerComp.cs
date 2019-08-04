using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;

public class GameManagerComp : MonoBehaviour
{
  public int m_health;
  public Transform m_levelLerpToLocation;
  public Transform m_root;
  public GameObject[] m_levels;
  public GameObject m_nextLevelButton;
  public GameObject m_restartLevelButton;
  public Text m_healthText;
  public AudioClip m_levelFlyInSound;
  public AudioClip m_levelFlyOutSound;


  private int m_currentLevelIndex;
  private Transform m_currentSpawnedLevel;
  private Vector3 m_initalLevelPos;

  private void Start()
  {
    m_currentLevelIndex = 0;
    m_currentSpawnedLevel = m_levels[m_currentLevelIndex].transform;
    m_initalLevelPos = m_currentSpawnedLevel.position;

    //m_healthText.text = "butts";
  }
  void Update()
  {
    //level done yet?
    CheckForLevelComplete();

    //all the input in the game basically
    ProcessPlayerInput();

    //UI
    m_healthText.text = "Health: " + m_health;
  }

  void CheckForLevelComplete()
  {
    if(BugEnemyComp.s_totalEnemiesAlive <= 0 && EnemySpawnManagerComp.s_done && m_health > 0)
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

      m_health-= 5;
      if(m_health <= 0)
      {
        m_restartLevelButton.SetActive(true);
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

  public void RestartCurrentLevel()
  {
    m_currentLevelIndex--;
    LoadNextLevel();
  }

  IEnumerator FlyOutLevel()
  {
    GetComponent<AudioSource>().PlayOneShot(m_levelFlyOutSound, .75f);

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
    while (Vector3.Distance(m_currentSpawnedLevel.position, m_initalLevelPos) > .2f)
    {
      m_currentSpawnedLevel.position = Vector3.Lerp(m_currentSpawnedLevel.position, m_initalLevelPos, Time.deltaTime * 4);
      yield return null;
    }
    m_currentSpawnedLevel.position = m_initalLevelPos;

    GetComponent<AudioSource>().PlayOneShot(m_levelFlyInSound, .75f);

    StartCurrentLevel();
  }
  void StartCurrentLevel()
  {
    m_restartLevelButton.SetActive(false);
    m_nextLevelButton.SetActive(false);
  }
}
