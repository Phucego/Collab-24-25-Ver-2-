using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using static LevelEditor_Handler;

public class WaveManager : MonoBehaviour
{
    [HideInInspector] public static WaveManager Instance;

    [SerializeField] GameObject _enemyPrefab;
    private List<BaseEnemySO> _enemyTypes;

    [Header("Level Data:")]
    [SerializeField] string _selectedLevel;
    private List<string> _lvList = new List<string>();
    private string _jsonDirectory;
    [Tooltip("Timer before the next wave starts (Seconds)")]
    [SerializeField] float _timerBetweenWave = 30f;

    private List<LevelData> _curData = new List<LevelData>(); // Just store 1 item but needed to serialize

    [Header("Level Pooling")]
    [SerializeField] GameObject _enemyPool;
    [SerializeField] GameObject _visualPool;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        _enemyTypes = new List<BaseEnemySO>(Resources.LoadAll<BaseEnemySO>("EnemySO"));
        _enemyTypes.AddRange(Resources.LoadAll<BaseEnemySO>("BossSO"));

        #if UNITY_EDITOR
             _jsonDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Levels"); // Editors
        #else
            _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Works in Final Build
        #endif

        if (Directory.Exists(_jsonDirectory))
        {
            string[] files = Directory.GetFiles(_jsonDirectory, "*.json"); // Get all JSON files

            foreach (string f in files)
            {
                //Debug.Log($"Loaded: {f}");

                string fileName = Path.GetFileName(f);
                _lvList.Add(fileName.ToUpper().Replace(".JSON", ""));
            }
        }
    }

    void Start()
    {
        if (!_lvList.Contains(_selectedLevel.ToUpper()))
            Debug.LogError("Level is not available or created yet in the data base!");
        else
            _curData = JsonConvert.DeserializeObject<List<LevelData>>(File.ReadAllText($"{_jsonDirectory}/{_selectedLevel.ToString()}.JSON"));
    }

    public void GetLevelProgress()
    {

    }

    public void StartLevel()
    {

    }

    public void ContinueNextWave()
    {

    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Keypad1))
            SpawnEnemy("Tut_Path1", 0);
        else if (Input.GetKeyDown(KeyCode.Keypad2))
            SpawnEnemy("Tut_Path1", 1);
        else if (Input.GetKeyDown(KeyCode.Keypad3))
            SpawnEnemy("Tut_Path1", 2);
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            SpawnEnemy("Tut_Path2", 0);
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            SpawnEnemy("Tut_Path2", 1);
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            SpawnEnemy("Tut_Path2", 2);
        */
    }

    private void SpawnEnemy(string path, int type)
    {
        Instantiate(_enemyPrefab).GetComponent<EnemyBehavior>().Spawn();
    }
}