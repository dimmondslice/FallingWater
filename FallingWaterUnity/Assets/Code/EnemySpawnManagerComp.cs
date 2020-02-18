using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManagerComp : MonoBehaviour
{
  public GameObject[] m_enemyPrefabs;
  public Transform[] m_spawners;
  public float m_spawnFrequencySec;
  public SpawnSet[] m_sets;

  private int m_currentSpawnSetIndex;
  private int m_currentSet;
  private float m_timeToNextSpawn;

  public static bool s_done = false;

  private GameObject m_currentBugType;

  [System.Serializable]
  public struct SpawnSet
  {
    public Transform spawner;
    public int numEnemies;
    public bool friendly;
  };

  private void Start()
  {
    s_done = false; //static state, need to reinstate this after this spawns
    BugEnemyComp.s_totalEnemiesAlive = 0;

    m_timeToNextSpawn = m_spawnFrequencySec * 5;
    m_currentSet = 0;
    //m_currentSpawner = Random.Range(0, m_spawners.Length);
    m_currentBugType = m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Length - 1)];
  }

  void Update()
  {
    if (s_done)
      return;

    m_timeToNextSpawn -= Time.deltaTime;

    if(m_timeToNextSpawn <= 0.0f && m_sets.Length > 0)
    {
      Transform spawner = m_sets[m_currentSet].spawner; //m_spawners[m_currentSpawner].transform;
      BugEnemyComp.s_totalEnemiesAlive++;
      GameObject freshy = Instantiate(m_currentBugType, spawner.position, spawner.rotation, spawner);
      int leftorRight = m_currentSpawnSetIndex % 2 == 0 ? -1 : 1;
      freshy.GetComponent<BugEnemyComp>().SetNextDirection(leftorRight); //alternate which side of the hexes the enemies prefer

      m_currentSpawnSetIndex++;

      if (m_currentSpawnSetIndex >= m_sets[m_currentSet].numEnemies)
      {
        m_currentSet++; //m_currentSpawner = Random.Range(0, m_spawners.Length);

        m_currentBugType = m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Length)];
        if (m_currentSet >= m_sets.Length)
        {
          s_done = true;
          return;
        }

        m_currentSpawnSetIndex = 0;
        m_timeToNextSpawn = m_spawnFrequencySec * m_sets[m_currentSet].numEnemies; //one set's worth of time with no enemies
      }

      m_timeToNextSpawn = m_spawnFrequencySec;
    }
  }
}
