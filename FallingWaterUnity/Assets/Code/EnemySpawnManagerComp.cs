﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManagerComp : MonoBehaviour
{
  public GameObject m_enemyPrefab;
  public Transform[] m_spawners;
  public float m_spawnFrequencySec;
  public SpawnSet[] m_sets;

  private int m_currentSpawnSetIndex;
  private int m_currentSet;
  private float m_timeToNextSpawn;
  private bool m_done = false;

  [System.Serializable]
  public struct SpawnSet
  {
    public Transform spawner;
    public int numEnemies;
    public bool friendly;
  };

  private void Start()
  {
    m_timeToNextSpawn = m_spawnFrequencySec;
    m_currentSet = 0;
    //m_currentSpawner = Random.Range(0, m_spawners.Length);
  }

  void Update()
  {
    if (m_done)
      return;

    m_timeToNextSpawn -= Time.deltaTime;

    if(m_timeToNextSpawn <= 0.0f)
    {
      Transform spawner = m_sets[m_currentSet].spawner; //m_spawners[m_currentSpawner].transform;
      GameObject freshy = Instantiate(m_enemyPrefab, spawner.position, spawner.rotation, spawner);
      freshy.GetComponent<BugEnemyComp>().m_nextDir = m_currentSpawnSetIndex % 2 == 0 ? -1 : 1; //alternate which side of the hexes the enemies prefer

      m_currentSpawnSetIndex++;

      if (m_currentSpawnSetIndex >= m_sets[m_currentSet].numEnemies)
      {
        m_currentSet++; //m_currentSpawner = Random.Range(0, m_spawners.Length);
        m_currentSpawnSetIndex = 0;
        m_timeToNextSpawn = m_spawnFrequencySec * m_sets[m_currentSet].numEnemies; //one set's worth of time with no enemies

        if(m_currentSet >= m_sets.Length)
        {
          m_done = true;
        }
      }

      m_timeToNextSpawn = m_spawnFrequencySec;
    }
  }
}
