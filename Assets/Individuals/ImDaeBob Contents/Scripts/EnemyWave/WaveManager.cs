using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static LevelEditor_Handler;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    public event Action OnWaveComplete;
    public event Action OnLevelComplete;

    [Header("Enemy Settings")]
    [SerializeField] private GameObject _enemyPrefab;
    private Dictionary<string, Queue<GameObject>> _enemyPools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> _poolParents = new Dictionary<string, GameObject>();
    private List<BaseEnemySO> _enemyTypes;

    [Header("Level Data")]
    [SerializeField] private string _selectedLevel;
    private List<string> _lvList = new List<string>();
    private string _jsonDirectory;
    private List<LevelData> _curData = new List<LevelData>();

    [Header("Wave Control")]
    [SerializeField] private float _timerBetweenWave = 30f;
    private bool _waitingForNextWave = false;

    private int _curWave = 0;
    private int _allEnemies = 0, _summoned = 0;
    private bool _isSpawning = false;

    [Header("Pooling")]
    [SerializeField] private GameObject _enemyPool;
    [SerializeField] private GameObject _visualPool;

    //-------------------------------------------------------------- < Public Functions > --------------------------------------------------------------//
    public float GetLevelProgress()
    {
        return (_summoned / (float)_allEnemies) * 100f;
    }

    public void StartWave()
    {
        if (!_isSpawning)
            StartCoroutine(HandleWaveSpawning());
    }

    public void SkipToNextWave()
    {
        if (_waitingForNextWave)
        {
            Debug.Log("Skipping to next wave!");
            _waitingForNextWave = false;
        }
    }

    public void ResetLevel()
    {
        LoadLevel();
        _curWave = 0;
        _isSpawning = false;
        _waitingForNextWave = false;
    }

    //-------------------------------------------------------------------- < Main > --------------------------------------------------------------------//
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartWave();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SkipToNextWave();
        }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        #if UNITY_EDITOR
            _jsonDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Levels");
        #else
            _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData");
        #endif

        if (Directory.Exists(_jsonDirectory))
        {
            string[] files = Directory.GetFiles(_jsonDirectory, "*.json");

            foreach (string f in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(f);
                _lvList.Add(fileName.ToUpper());
            }
        }
    }

    void Start()
    {
        LoadLevel();
        CreateEnemyPools();
    }

    private void CreateEnemyPools()
    {
        _enemyTypes = new List<BaseEnemySO>(Resources.LoadAll<BaseEnemySO>("EnemySO"));
        _enemyTypes.AddRange(Resources.LoadAll<BaseEnemySO>("BossSO"));

        Dictionary<string, int> maxNeeded = CalculateMaxNeededEnemies();

        foreach (var enemyType in _enemyTypes)
        {
            string typeName = enemyType.name.ToUpper();

            GameObject poolParent = new GameObject($"{typeName} POOL");
            poolParent.transform.SetParent(_enemyPool.transform);

            _poolParents[typeName] = poolParent;
            _enemyPools[typeName] = new Queue<GameObject>();

            int amountToPreInstantiate = maxNeeded.ContainsKey(typeName) ? maxNeeded[typeName] : 1;
            for (int i = 0; i < amountToPreInstantiate; i++)
            {
                GameObject enemy = Instantiate(_enemyPrefab, poolParent.transform);
                enemy.SetActive(false);
                _enemyPools[typeName].Enqueue(enemy);
            }
        }
    }

    private Dictionary<string, int> CalculateMaxNeededEnemies()
    {
        Dictionary<string, int> maxNeeded = new Dictionary<string, int>();

        foreach (var wave in _curData[0].Waves)
        {
            Dictionary<string, int> currentWaveCount = new Dictionary<string, int>();

            foreach (var group in wave.Groups)
            {
                foreach (var enemyData in group.Enemies)
                {
                    int totalForGroup = enemyData.Amount * (group.IsLoop ? group.LoopAmount : 1);

                    if (!currentWaveCount.ContainsKey(enemyData.Type))
                        currentWaveCount[enemyData.Type] = 0;

                    currentWaveCount[enemyData.Type] += totalForGroup;
                }
            }

            foreach (var kvp in currentWaveCount)
            {
                if (!maxNeeded.ContainsKey(kvp.Key) || kvp.Value > maxNeeded[kvp.Key])
                    maxNeeded[kvp.Key] = kvp.Value;
            }
        }

        return maxNeeded;
    }



    private void LoadLevel()
    {
        if (!_lvList.Contains(_selectedLevel.ToUpper()))
        {
            Debug.LogError($"Level '{_selectedLevel}' is not available in the database!!");
            return;
        }

        string filePath = Path.Combine(_jsonDirectory, _selectedLevel + ".json");
        _curData = JsonConvert.DeserializeObject<List<LevelData>>(File.ReadAllText(filePath));

        int totalEnemies = 0;
        foreach (var wave in _curData[0].Waves)
        {
            foreach (var group in wave.Groups)
            {
                int groupCount = 0;
                foreach (var enemy in group.Enemies)
                    groupCount += enemy.Amount;

                totalEnemies += group.IsLoop ? groupCount * group.LoopAmount : groupCount;
            }
        }
        _allEnemies = totalEnemies;
        _summoned = 0;
    }


    private IEnumerator HandleWaveSpawning()
    {
        while (_curWave < _curData[0].Waves.Count)
        {
            _isSpawning = true;
            _waitingForNextWave = false;

            Debug.Log($"Starting Wave {_curWave + 1}");
            yield return StartCoroutine(SpawnWave(_curData[0].Waves[_curWave]));

            Debug.Log($"Wave {_curWave + 1} completed!");
            _curWave++;

            if (_curWave < _curData[0].Waves.Count)
            {
                OnWaveComplete?.Invoke();

                _waitingForNextWave = true;
                float remainingTime = _timerBetweenWave;
                while (_waitingForNextWave && remainingTime > 0)
                {
                    yield return new WaitForSeconds(1f);
                    remainingTime -= 1f;
                }
            }
        }

        Debug.Log("All waves completed!");
        _isSpawning = false;
        
        OnLevelComplete?.Invoke();
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        foreach (var group in wave.Groups)
            StartCoroutine(SpawnGroup(group, group.Path));

        yield return null;
    }

    private IEnumerator SpawnGroup(GroupData group, string path)
    {
        int loopCount = group.IsLoop ? group.LoopAmount : 1;

        for (int loop = 0; loop < loopCount; loop++)
        {
            List<Coroutine> enemySpawns = new List<Coroutine>();

            // Spawn all enemy types in the group
            foreach (var enemyData in group.Enemies)
                enemySpawns.Add(StartCoroutine(SpawnEnemyBatch(enemyData, path)));

            // Wait for all enemy types in the group to finish spawning before looping again
            foreach (Coroutine enemySpawn in enemySpawns)
                yield return enemySpawn;

            // Apply group delay only if it's looping and not the first iteration
            if (loop < loopCount - 1 && group.Delay > 0)
                yield return new WaitForSeconds(group.Delay);
        }
    }



    private IEnumerator SpawnEnemyBatch(EnemyData enemyData, string path)
    {
        if (enemyData.Amount > 0)
        {
            SpawnEnemy(enemyData.Type, path);

            for (int i = 1; i < enemyData.Amount; i++)
            {
                yield return new WaitForSeconds(enemyData.SpawnInterval);
                SpawnEnemy(enemyData.Type, path);
            }
        }
    }


    private void SpawnEnemy(string enemyType, string path)
    {
        if (!_enemyPools.ContainsKey(enemyType))
        {
            Debug.LogError($"No pool found for enemy type: {enemyType}");
            return;
        }

        GameObject enemy;
        if (_enemyPools[enemyType].Count > 0)
            enemy = _enemyPools[enemyType].Dequeue();
        else
            enemy = Instantiate(_enemyPrefab, _poolParents[enemyType].transform);

        enemy.SetActive(true);
        enemy.GetComponent<EnemyBehavior>().Spawn(_enemyTypes.Find(e => e.name.ToUpper() == enemyType), path);
    }

    public void ReturnToPool(GameObject enemy, string enemyType)
    {
        if (!_enemyPools.ContainsKey(enemyType))
        {
            Debug.LogError($"No pool found for enemy type: {enemyType}");
            Destroy(enemy);
            return;
        }

        enemy.SetActive(false);
        _enemyPools[enemyType].Enqueue(enemy);
    }
}
