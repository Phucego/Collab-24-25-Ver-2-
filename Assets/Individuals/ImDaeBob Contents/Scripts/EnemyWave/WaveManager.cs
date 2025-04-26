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
    private List<BaseEnemySO> _enemySOs;

    [Header("Level Data")]
    public string _selectedLevel;
    private List<string> _lvList = new List<string>();
    private string _jsonDirectory;
    [HideInInspector] public List<LevelData> _curData = new List<LevelData>();

    [Header("Wave Control")]
    [SerializeField] private int _timerBetweenWave = 30;
    [HideInInspector] public int _remTime = 0;
    [HideInInspector] public bool _waitingForNextWave = false;

    [HideInInspector] public int _curWave = 0;
    [HideInInspector] public int _allEnemies = 0, _summoned = 0, _despawned = 0;
    [HideInInspector] public int _allEnemiesInWave = 0, _summonedInWave = 0, _despawnedInWave = 0;
    [HideInInspector] public bool _isSpawning = false, _waveFinished = false;

    [Header("Pooling")]
    [SerializeField] private GameObject _enemyPool;
    [SerializeField] private GameObject _visualPool;

    //-------------------------------------------------------------- < Public Functions > --------------------------------------------------------------//
    // For general gameplay coding
    public float GetLevelProgress()
    {
        return (_summoned / (float)_allEnemies) * 100f;
    }

    public float GetWaveProgress()
    {
        return (_summonedInWave / (float)_allEnemiesInWave) * 100f;
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

    public int GetTimeBetweenWave()
    {
        return _remTime;
    }

    public void ResetLevel()
    {
        LoadLevel();
        _curWave = 0;
        _summoned = 0;
        _despawned = 0;
        _summonedInWave = 0;
        _despawnedInWave = 0;
        _isSpawning = false;
        _waitingForNextWave = false;
    }

    // For testing in Level Editor
    public void TestCurWave(WaveData wave)
    {
        _despawnedInWave = 0;
        _allEnemiesInWave = 0;
        foreach (var group in wave.Groups)
        {
            foreach (var enemy in group.Enemies)
                _allEnemiesInWave += enemy.Amount * (group.IsLoop ? group.LoopAmount : 1);
        }
        _summonedInWave = 0;

        StartCoroutine(SpawnWave(wave));
    }

    public void KillAllTest()
    {
        _isSpawning = false;
        StopAllCoroutines();

        foreach (var pool in _enemyPools)
        {
            string _enemyType = pool.Key;
            foreach (Transform child in _poolParents[_enemyType].transform)
            {
                if (child.gameObject.activeSelf)
                    child.gameObject.SetActive(false);
            }
        }
    }

    public void ResetTest(WaveData wave)
    {
        KillAllTest();
        TestCurWave(wave);
    }
    //-------------------------------------------------------------------- < Main > --------------------------------------------------------------------//
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            SpawnEnemy("GOLEM", "TUT_PATH1");
        else if (Input.GetKeyDown(KeyCode.Keypad2))
            SpawnEnemy("DEMON", "TUT_PATH1");
        else if (Input.GetKeyDown(KeyCode.Keypad3))
            SpawnEnemy("CLOAK", "TUT_PATH1");
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            SpawnEnemy("GOLEM", "TUT_PATH2");
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            SpawnEnemy("DEMON", "TUT_PATH2");
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            SpawnEnemy("CLOAK", "TUT_PATH2");
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
            string[] _files = Directory.GetFiles(_jsonDirectory, "*.json");

            foreach (string f in _files)
            {
                string _fileName = Path.GetFileNameWithoutExtension(f);
                _lvList.Add(_fileName.ToUpper());
            }
        }
    }

    void Start()
    {
        if (string.IsNullOrEmpty(_selectedLevel))
            _selectedLevel = "DEFAULT";
        LoadLevel();
        CreateEnemyPools();
    }

    private void LoadLevel()
    {
        if (!_lvList.Contains(_selectedLevel.ToUpper()))
        {
            Debug.LogError($"Level '{_selectedLevel}' is not available in the database!!");
            return;
        }

        string _filePath = Path.Combine(_jsonDirectory, _selectedLevel + ".json");
        _curData = JsonConvert.DeserializeObject<List<LevelData>>(File.ReadAllText(_filePath));

        int _totalEnemies = 0;
        foreach (var wave in _curData[0].Waves)
        {
            foreach (var group in wave.Groups)
            {
                int groupCount = 0;
                foreach (var enemy in group.Enemies)
                    groupCount += enemy.Amount;

                _totalEnemies += group.IsLoop ? groupCount * group.LoopAmount : groupCount;
            }
        }
        _allEnemies = _totalEnemies;
        _summoned = 0;
        _summonedInWave = 0;
    }

    private void CreateEnemyPools()
    {
        _enemySOs = new List<BaseEnemySO>(Resources.LoadAll<BaseEnemySO>("EnemySO"));
        _enemySOs.AddRange(Resources.LoadAll<BaseEnemySO>("BossSO"));

        Dictionary<string, int> _maxNeeded = CalculateMaxNeededEnemies();

        foreach (var e in _enemySOs)
        {
            string _name = e.name.ToUpper();

            GameObject poolParent = new GameObject($"{_name} POOL");
            poolParent.transform.SetParent(_enemyPool.transform);

            _poolParents[_name] = poolParent;
            _enemyPools[_name] = new Queue<GameObject>();

            int _amountToPreInstantiate = _maxNeeded.ContainsKey(_name) ? _maxNeeded[_name] : 1;
            for (int i = 0; i < _amountToPreInstantiate; i++)
            {
                GameObject _enemy = Instantiate(_enemyPrefab, poolParent.transform);
                _enemy.SetActive(false);
                _enemyPools[_name].Enqueue(_enemy);
            }
        }
    }

    private Dictionary<string, int> CalculateMaxNeededEnemies()
    {
        Dictionary<string, int> _maxNeeded = new Dictionary<string, int>();

        foreach (var wave in _curData[0].Waves)
        {
            Dictionary<string, int> _curWaveCount = new Dictionary<string, int>();

            foreach (var group in wave.Groups)
            {
                foreach (var enemyData in group.Enemies)
                {
                    int _totalForGroup = enemyData.Amount * (group.IsLoop ? group.LoopAmount : 1);

                    if (!_curWaveCount.ContainsKey(enemyData.Name))
                        _curWaveCount[enemyData.Name] = 0;

                    _curWaveCount[enemyData.Name] += _totalForGroup;
                }
            }

            foreach (var kvp in _curWaveCount)
            {
                if (!_maxNeeded.ContainsKey(kvp.Key) || kvp.Value > _maxNeeded[kvp.Key])
                    _maxNeeded[kvp.Key] = kvp.Value;
            }
        }

        return _maxNeeded;
    }

    private IEnumerator HandleWaveSpawning()
    {
        while (_curWave < _curData[0].Waves.Count)
        {
            _isSpawning = true;
            _waveFinished = false;
            _waitingForNextWave = false;

            _summonedInWave = 0;
            _despawnedInWave = 0;
            _allEnemiesInWave = 0;
            foreach (var group in _curData[0].Waves[_curWave].Groups)
            {
                foreach (var enemy in group.Enemies)
                    _allEnemiesInWave += enemy.Amount * (group.IsLoop ? group.LoopAmount : 1);
            }

            Debug.Log($"Initiate Wave {_curWave + 1}");
            yield return StartCoroutine(SpawnWave(_curData[0].Waves[_curWave]));

            while (!_waveFinished)
            {
                yield return null;
            }
            _curWave++;
            if (_curWave < _curData[0].Waves.Count)
            {
                Debug.Log($"Wave {_curWave} finished! Commencing timer for Wave {_curWave + 1} to start!");
                _waitingForNextWave = true;
                _remTime = _timerBetweenWave;
                while (_waitingForNextWave && _remTime > 0)
                {
                    yield return new WaitForSeconds(1f);
                    _remTime -= 1;
                }
            }
        }

        _isSpawning = false;
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        foreach (var group in wave.Groups)
            StartCoroutine(SpawnGroup(group, group.Path));

        yield return null;
    }

    private IEnumerator SpawnGroup(GroupData group, string path)
    {
        int _loopCount = group.IsLoop ? group.LoopAmount : 1;

        for (int loop = 0; loop < _loopCount; loop++)
        {
            List<Coroutine> _enemySpawns = new List<Coroutine>();

            yield return new WaitForSeconds(group.Delay);

            foreach (var enemyData in group.Enemies)
                _enemySpawns.Add(StartCoroutine(SpawnEnemyBatch(enemyData, path)));

            foreach (Coroutine enemySpawn in _enemySpawns) // Wait for all enemy types in the group to finish spawning before looping again
                yield return enemySpawn;
        }
    }

    private IEnumerator SpawnEnemyBatch(EnemyData enemyData, string path)
    {
        if (enemyData.Amount > 0)
        {
            SpawnEnemy(enemyData.Name, path);

            for (int i = 1; i < enemyData.Amount; i++)
            {
                yield return new WaitForSeconds(enemyData.SpawnInterval);
                SpawnEnemy(enemyData.Name, path);
            }
        }
    }

    private void SpawnEnemy(string enemyName, string path)
    {
        if (!_enemyPools.ContainsKey(enemyName))
        {
            Debug.LogError($"No pool found for enemy type: {enemyName}");
            return;
        }

        GameObject _enemy = _enemyPrefab;
        if (_enemyPools[enemyName].Count > 0)
            _enemy = _enemyPools[enemyName].Dequeue();
        else
            _enemy = Instantiate(_enemyPrefab, _poolParents[enemyName].transform);

        _enemy.SetActive(true);
        _enemy.GetComponent<EnemyBehavior>().Spawn(_enemySOs.Find(e => e.name.ToUpper() == enemyName), path);

        _summoned++;
        _summonedInWave++;
    }

    public void ReturnToPool(GameObject enemy, string name)
    {
        name = name.ToUpper();
        if (!_enemyPools.ContainsKey(name))
        {
            Debug.LogError($"No pool found for enemy type: {name}");
            Destroy(enemy);
            return;
        }

        enemy.SetActive(false);
        _enemyPools[name].Enqueue(enemy);

        _despawned++;
        _despawnedInWave++;

        if (_despawnedInWave >= _allEnemiesInWave)
        {
            _waveFinished = true;
            OnWaveComplete?.Invoke();
        }

        if (_despawned >= _allEnemies)
        {
            Debug.Log("Level finished!");

            OnLevelComplete?.Invoke();
        }
    }
}
