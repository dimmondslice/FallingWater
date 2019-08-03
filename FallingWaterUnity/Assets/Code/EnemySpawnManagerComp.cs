using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManagerComp : MonoBehaviour
{
  public GameObject m_enemyPrefab;
  public Transform[] m_spawners;
  public int m_spawnSetSize;
  public float m_spawnFrequencySec;

  private int m_currentSpawnSetIndex;
  private int m_currentSpawner;
  private float m_timeToNextSpawn;

  private void Start()
  {
    m_timeToNextSpawn = m_spawnFrequencySec;
  }
  void Update()
  {
    m_timeToNextSpawn -= Time.deltaTime;

    if(m_timeToNextSpawn <= 0.0f)
    {
      Instantiate(m_enemyPrefab, m_spawners[m_currentSpawner].transform.position, Quaternion.identity, m_spawners[m_currentSpawner].transform);
      m_currentSpawnSetIndex++;

      if (m_currentSpawnSetIndex >= m_spawnSetSize)
      {
        m_currentSpawner = Random.Range(0, m_spawners.Length);
        m_currentSpawnSetIndex = 0;
      }

      m_timeToNextSpawn = m_spawnFrequencySec;
    }
  }
}
