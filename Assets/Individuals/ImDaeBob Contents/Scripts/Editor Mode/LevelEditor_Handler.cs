using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class LevelEditor_Handler : MonoBehaviour
{
    //---------------------------------------------------------------- < VARIABLES > ----------------------------------------------------------------//
    // [ Json ] //
    [Header("Toggle for Testing Mode (Editor Only)")]
    [SerializeField] bool _jsonReadInEditor = true;
    private string _jsonDirectory;
    private string _pathDirectory;

    // [ Data Process ] //
    private List<string> _jsonFiles = new List<string>();
    private LevelData _curlevel = new LevelData();

    private List<LevelData> _curData = new List<LevelData>(); // Just store 1 item but needed to serialize

    private List<string> _paths = new List<string>();

    // [ References ] //
    [Header("References")]
    [SerializeField] GameObject _selectionPanel;
    [Header("")]
    [SerializeField] Button _return;
    [Header("")]
    [SerializeField] TMP_Text _debugLog;
    private Coroutine _clearTextCoroutine;
    [Header("")]
    [SerializeField] TMP_Dropdown _level;
    [SerializeField] TMP_InputField _saveAs;
    [Header("")]
    [SerializeField] TMP_Dropdown _wave;
    [SerializeField] Toggle _isBoss;
    [Header("")]
    [SerializeField] TMP_Dropdown _group;
    [SerializeField] TMP_InputField _gName;
    [SerializeField] TMP_Dropdown _gPath;
    [SerializeField] Toggle _gLoop;
    [SerializeField] TMP_InputField _gLoopAmount;
    [SerializeField] TMP_InputField _gDelayBeforeSpawn;
    [SerializeField] TMP_InputField _gDelayAfterSpawn;
    [Header("")]
    [SerializeField] TMP_Dropdown _enemy;
    [SerializeField] TMP_Dropdown _eType;
    [SerializeField] TMP_InputField _eAmount;
    [SerializeField] TMP_InputField _eInterval;
    [Header("")]
    [SerializeField] GameObject _eList;
    [SerializeField] GameObject _anEnemyData;

    // [ Transition ] //
    [Header("Transition")]
    [SerializeField] GameObject _transition;

    //-------------------------------------------------------------- < MAIN FUNCTIONS > --------------------------------------------------------------//
    void OnEnable()
    {
        this.gameObject.GetComponent<CanvasGroup>().alpha = 1;

        _pathDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Paths");
        #if UNITY_EDITOR
            if (_jsonReadInEditor)
                _jsonDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Levels"); // Editors
            else
                _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Final Build
        #else
            _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Works in Final Build
        #endif

        SearchFiles();
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------------- < MINI FUNCTIONS > --------------------------------------------------------------//
    // [ BUTTON HANDLERS ] // 
    public void ReturnToSelection()
    {
        _transition.SetActive(true);
        StartCoroutine(EndTransition());
    }

    // [ DATA HANDLERS ] //
    void SearchFiles()
    {
        _gPath.ClearOptions();
        _paths.Clear();
        if (Directory.Exists(_pathDirectory))
        {
            string[] files = Directory.GetFiles(_pathDirectory, "*.json"); // Get all JSON files

            foreach (string f in files)
            {
                string fileName = Path.GetFileName(f);
                _paths.Add(fileName.ToUpper().Replace(".JSON", ""));
            }
        }
        _gPath.AddOptions(new List<string>(_paths));

        _level.ClearOptions();
        _jsonFiles.Clear();
        if (Directory.Exists(_jsonDirectory))
        {
            string[] files = Directory.GetFiles(_jsonDirectory, "*.json"); // Get all JSON files

            foreach (string f in files)
            {
                //Debug.Log($"Loaded: {f}");

                string fileName = Path.GetFileName(f);
                _jsonFiles.Add(fileName.ToUpper().Replace(".JSON", ""));
            }
        }
        _level.AddOptions(new List<string>(_jsonFiles));

        LoadLevel();
    }

    // Level Hanlders
    public void LoadLevel()
    {
        // Load level
        _curData.Clear();
        _curlevel.Waves.Clear();

        string _getFile = $"{_jsonDirectory}/{_jsonFiles[_level.value].ToString()}.JSON";
        _curData = JsonConvert.DeserializeObject<List<LevelData>>(File.ReadAllText(_getFile));

        DebugLog($"Loaded data from {_level.options[_level.value].text}.json");

        // Load Wave
        _wave.ClearOptions();

        List<string> _waveList = new List<string>();
        List<int> _bossWaveIndex = new List<int>();
        for (int i = 0; i < _curData[0].Waves.Count; i++)
        {
            _waveList.Add($"Wave {i + 1}");
            if (_curData[0].Waves[i].IsBossWave)
                _bossWaveIndex.Add(i);
        }
        _wave.AddOptions(_waveList);
        foreach (int i in _bossWaveIndex)
            _wave.options[i].text = $"<color=red>Wave {i}</color>";
        _wave.RefreshShownValue();

        LoadWave();
    }

    public void SaveLevel()
    {
        if (string.IsNullOrEmpty(_saveAs.text)) // No Input -> Save as the same current selected file
        {
            string _curPath = $"{_jsonDirectory}/{_level.options[_level.value].text.ToUpper()}.JSON";
            File.WriteAllText(_curPath, JsonConvert.SerializeObject(_curData, Formatting.Indented));

            DebugLog($"Saved {_level.options[_level.value].text}.json");
        }
        else
        {
            _saveAs.text = _saveAs.text.ToUpper();
            string _getPath = $"{_jsonDirectory}/{_saveAs.text}.JSON";
            if (_jsonFiles.Contains($"{_saveAs.text}")) // Have input but the file already existed
            {
                File.WriteAllText(_getPath, JsonConvert.SerializeObject(_curData, Formatting.Indented));

                DebugLog($"Saved current data as {_saveAs.text}.json");
            }
            else // Have input but the file isn't existed
            {
                if (_jsonFiles.Count == 0)
                {
                    List<LevelData> newData = new List<LevelData>()
                    {
                        new LevelData(new List<WaveData>()
                        {
                            new WaveData(new List<GroupData>()
                            {
                                new GroupData(new List<EnemyData>()
                                {
                                    new EnemyData()
                                })
                            })
                        })
                    };
                    File.WriteAllText(_getPath, JsonConvert.SerializeObject(newData, Formatting.Indented));

                    DebugLog($"Create the first level ever: {_saveAs.text}");
                }
                else
                {
                    File.WriteAllText(_getPath, JsonConvert.SerializeObject(_curData, Formatting.Indented));

                    DebugLog($"Saved current data as a new file: {_saveAs.text}.json");
                }
                SearchFiles();
                this.gameObject.GetComponent<Animator>().SetTrigger("New");
            }
        }
        _saveAs.text = null;
    }

    public void CreateNewLevel()
    {
        if (!string.IsNullOrEmpty(_saveAs.text))
        {
            _saveAs.text = _saveAs.text.ToUpper();
            string _getPath = $"{_jsonDirectory}/{_saveAs.text}.JSON";
            if (!_jsonFiles.Contains($"{_saveAs.text}"))
            {
                List<LevelData> newData = new List<LevelData>()
                {
                    new LevelData(new List<WaveData>()
                    {
                        new WaveData(new List<GroupData>()
                        {
                            new GroupData(new List<EnemyData>()
                            {
                                new EnemyData()
                            })
                        })
                    })
                };
                File.WriteAllText(_getPath, JsonConvert.SerializeObject(newData, Formatting.Indented));

                DebugLog($"Created a brand new level: {_saveAs.text}");

                SearchFiles();
                this.gameObject.GetComponent<Animator>().SetTrigger("New");
                _saveAs.text = null;
            }
            else
                DebugLog("A level with the same name has already existed");
        }
        else
            DebugLog("There is no name for the new level file!!");
    }

    public void DeleteCurLevel()
    {
        if (_jsonFiles.Count == 0)
        {
            DebugLog("No level available to delete.");
            return;
        }

        string _recyleBin = $"{_jsonDirectory}/RecycleBin/";
        if (!Directory.Exists(_recyleBin))
            Directory.CreateDirectory(_recyleBin);

        string _curPath = $"{_jsonDirectory}/{_jsonFiles[_level.value]}.JSON";
        if (File.Exists(_curPath))
        {
            string _deletedPath = $"{_recyleBin}/{_jsonFiles[_level.value]}.JSON";
            if (File.Exists(_deletedPath))
                File.Delete(_deletedPath);
            File.Move(_curPath, _deletedPath); // Move the file to the "Recycle Bin" instead of permanently deleting it

            _jsonFiles.RemoveAt(_level.value);
            _level.options.RemoveAt(_level.value);
            _level.RefreshShownValue();

            if (_jsonFiles.Count > 0)
            {
                _level.value = 0;
                LoadLevel();
            }
        }
        else
            DebugLog("Selected level file does not exist.");
    }

    // Wave Handlers
    public void AddWave()
    {
        DebugLog($"Added new Wave - Wave {_wave.options.Count + 1}");

        _curData[0].Waves.Add(new WaveData(new List<GroupData>() { new GroupData(new List<EnemyData>())}));

        _wave.AddOptions(new List<string>() { $"Wave {_wave.options.Count + 1}"});
        _wave.RefreshShownValue();
    }

    public void RemoveLastWave()
    {
        if (_wave.options.Count > 1)
        {
            DebugLog($"Remove last Wave - Wave {_wave.options.Count}");

            _curData[0].Waves.RemoveAt(_wave.options.Count - 1);
            _wave.options.RemoveAt(_wave.options.Count - 1);
            _wave.RefreshShownValue();

            if (_wave.value >= _wave.options.Count - 1)
            {
                _wave.value = _wave.options.Count - 1;
                LoadWave();
            }
        }
        else
            DebugLog("There is not enough wave to initiate removal.");
    }

    public void EnableBossWave(bool isBoss)
    {
        DebugLog($"Enable Boss Wave for Wave {_wave.value + 1}");

        if (_curData.Count > 0)
        {
            _curData[0].Waves[_wave.value].IsBossWave = isBoss;
            if (isBoss)
                _wave.options[_wave.value].text = $"<color=red>Wave {_wave.value + 1}</color>";
            else
                _wave.options[_wave.value].text = $"<color=#323232>Wave {_wave.value + 1}</color>";
            _wave.RefreshShownValue();
        }
    }

    public void LoadWave()
    {
        //Load Wave
        if (_curData[0].Waves[_wave.value].IsBossWave)
            _isBoss.isOn = true;
        else
            _isBoss.isOn = false;

        //Load Group
        LoadGroup(true);
    }

    //Group Handlers
    public void AddGroup()
    {
        DebugLog($"Added new Group - Group {_group.options.Count + 1}");

        _curData[0].Waves[_wave.value].Groups.Add(new GroupData(new List<EnemyData>(), $"Group {_group.options.Count + 1}"));

        _group.AddOptions(new List<string>() { $"Group {_group.options.Count + 1}" });
        _group.RefreshShownValue();
    }

    public void RemoveLastGroup()
    {
        if (_group.options.Count > 1)
        {
            DebugLog($"Remove last group - {_group.options[_group.options.Count - 1].text}");

            _curData[0].Waves[_wave.value].Groups.RemoveAt(_group.options.Count - 1);
            _group.options.RemoveAt(_group.options.Count - 1);
            _group.RefreshShownValue();

            if (_group.value >= _group.options.Count - 1)
                _group.value = _group.options.Count - 1;
        }
        else
            DebugLog("There is not enough group to initiate removal.");
    }

    public void NamedGroup(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            DebugLog("Changed Group name!");

            _curData[0].Waves[_wave.value].Groups[_group.value].Name = newName.ToUpper();
            _group.options[_group.value].text = newName;
            _group.RefreshShownValue();

            _gName.text = string.Empty;
        }
    }

    public void ChangeGroupPath()
    {
        DebugLog($"Direct {_group.options[_group.value].text} to follow {_gPath.options[_gPath.value].text}!");

        _curData[0].Waves[_wave.value].Groups[_group.value].Path = _gPath.options[_gPath.value].text;
    }

    public void LoadGroup(bool resest)
    {
        if (resest)
        {
            _group.ClearOptions();

            List<string> _groupList = new List<string>();
            for (int i = 0; i < _curData[0].Waves[_wave.value].Groups.Count; i++)
                _groupList.Add(_curData[0].Waves[_wave.value].Groups[i].Name);
            _group.AddOptions(_groupList);
            _group.RefreshShownValue();
        }

        _gPath.value = _paths.IndexOf(_curData[0].Waves[_wave.value].Groups[_group.value].Path);
        _gPath.RefreshShownValue();
    }

    // [ ACCESSIBILITIES ] // 
    private void DebugLog(string message, float duration = 3f, float interval = 0.02f)
    {
        _debugLog.text = $">/ {message}";

        if (_clearTextCoroutine != null)
            StopCoroutine(_clearTextCoroutine);
        _clearTextCoroutine = StartCoroutine(ClearDebugLog(duration, interval));
    }

    // [ IENUMERATOR HANDLERS ] //
    IEnumerator EndTransition()
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        _selectionPanel.SetActive(true);
        _transition.GetComponent<Animator>().SetTrigger("Out");

        yield return new WaitForSeconds(0.5f);
        _transition.SetActive(false);
        this.gameObject.SetActive(false);
    }

    IEnumerator ClearDebugLog(float duration, float interval)
    {
        yield return new WaitForSeconds(duration);

        while (_debugLog.text.Length > 2)
        {
            _debugLog.text = _debugLog.text.Substring(0, _debugLog.text.Length - 1);
            yield return new WaitForSeconds(interval);
        }
    }

    //-------------------------------------------------------------- < Class & Struct > --------------------------------------------------------------//
    public class LevelData //For Json
    {
        public List<WaveData> Waves { get; set; } = new List<WaveData>();

        public LevelData() { }

        public LevelData(List<WaveData> waves)
        {
            Waves = waves;
        }
    }

    public class WaveData //For Json
    {
        public List<GroupData> Groups { get; set; } = new List<GroupData>();
        public bool IsBossWave { get; set; } = false;

        public WaveData() { }

        public WaveData(List<GroupData> groups, bool isBoss = false)
        {
            Groups = groups;
            IsBossWave = isBoss;
        }
    }

    public class GroupData //For Json
    {
        public string Name { get; set; } = "Group 1";
        public List<EnemyData> Enemies { get; set; } = new List<EnemyData>();
        public string Path { get; set; } = "";
        public bool IsLoop { get; set; } = false;
        public int LoopAmount { get; set; } = 0;
        public float DelayBefore { get; set; } = 0f;
        public float DelayAfter { get; set; } = 0f;

        public GroupData() { } 

        public GroupData(List<EnemyData> enemies, string name = "Group 1", string path = "", bool isLoop = false, int amount = 0, float delayBefore = 0f, float delayAfter = 0f)
        {
            Name = name;
            Enemies = enemies;
            Path = path;
            IsLoop = isLoop;
            LoopAmount = amount;
            DelayBefore = delayBefore;
            DelayAfter = delayAfter;
        }
    }

    public class EnemyData //For Json
    {
        public string Type { get; set; } = "Grounded";
        public int Amount { get; set; } = 1;
        public float SpawnInterval { get; set; } = 0f;

        public EnemyData(string type = "Grounded", int amount = 1, float spawnInterval = 0f)
        {
            Type = type;
            Amount = amount;
            SpawnInterval = spawnInterval;
        }
    }
}
