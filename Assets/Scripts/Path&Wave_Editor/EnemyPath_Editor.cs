using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using static EnemyPath_Editor;

public class EnemyPath_Editor : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////
    //Variables:
    private string _jsonDirectory;

    [Header("Loader")]
    [SerializeField] TMP_Text _fileLoaded;
    private PathDataList _pathDataList;

    [Header("The Editable Editor")]
    [SerializeField] CanvasGroup _editablePanel;
    [SerializeField] CanvasGroup _savePanel;

    [Header("Waypoints")]
    private string _jsonPath;

    // Save .json File
    private string _savePath;

    [Header("How to Set Position")]
    [SerializeField] RectTransform _h2u;

    /////////////////////////////////////////////////////////////////////////////
    //Main Functions:
    private bool _active = false;

    void Awake()
    {
        //Only for in Editor!
        _jsonDirectory = Path.Combine(Application.dataPath, "Enemies/Paths");
        if (!Directory.Exists(_jsonDirectory))
            Directory.CreateDirectory(_jsonDirectory);
    }

    private void Update()
    {

    }

    public void toggleEditor(bool mode)
    {
        _active = mode;
    }
    
    public bool getActive()
    {
        return _active; 
    }

    /////////////////////////////////////////////////////////////////////////////
    // Buttons and On Clicked functions:
    public void readJSON(string json)
    {
        _jsonPath = json;
        if (!string.IsNullOrEmpty(_jsonPath))
        {
            Debug.Log(_jsonPath);

            if (File.Exists(_jsonPath + ".json"))
                _pathDataList.pathDataList = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText(_jsonPath + ".json"));
        } 
    }

    public void createJSON()
    {
        if (!string.IsNullOrEmpty(_jsonPath))
        {
            Debug.Log($"Created {_jsonPath}.json");

            List<PathData> newData = new List<PathData>
            {
                new PathData(),
                new PathData()
            };

            File.WriteAllText(_jsonPath + ".json", JsonConvert.SerializeObject(newData, Formatting.Indented));
        }
    }

    public void addWaypoint()
    {
        Debug.Log("Added");
    }

    public void readSavePath(string path)
    {
        _savePath = path;
        if (!string.IsNullOrEmpty(_savePath))
        {
            Debug.Log(_savePath);
        }   
    }

    public void saveJson()
    {
        if (!string.IsNullOrEmpty(_savePath))
        {
            Debug.Log($"Saved to {_savePath}.json");

            File.WriteAllText(_savePath + ".json", JsonConvert.SerializeObject(_pathDataList.pathDataList, Formatting.Indented));
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    //Class
    public class PathData
    {
        [JsonProperty("pos")]
        public Vector3 Location { get; set; }

        [JsonProperty("scale")]
        public float ScaleMultiplier { get; set; }

        public PathData(Vector3 _location = default(Vector3), float _scaleMultiplier = 1f)
        {
            if (Vector3.Approximately(Location, Vector3.zero))
                Location = new Vector3(0, 0, 0);
            ScaleMultiplier = _scaleMultiplier;
        }
    }

    public class PathDataList
    {
        public List<PathData> pathDataList = new List<PathData>();
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }