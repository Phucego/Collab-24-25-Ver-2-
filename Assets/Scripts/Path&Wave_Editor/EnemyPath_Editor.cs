using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class EnemyPath_Editor : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////
    //Variables:
    private string _jsonDirectory;

    [Header("Loader")]
    [SerializeField] TMP_Text _announcer;
    private Tween _announcerTween;
    [SerializeField] GameObject _createButtonIfEmpty;
    private PathDataList _pathDataList = new PathDataList();

    [Header("The Editable Editor")]
    [SerializeField] GameObject _editablePanel;
    [SerializeField] GameObject _savePanel;

    [Header("Waypoints")]
    [SerializeField] GameObject _beginPoint;
    [SerializeField] GameObject _endPoint;
    private string _jsonPath;

    // Save .json File
    private string _savePath;

    [Header("How to Set Position")]
    [SerializeField] GameObject _howToUse;

    /////////////////////////////////////////////////////////////////////////////
    //Main Functions:
    private bool _active = true;

    void Awake()
    {
        //Only for in Editor!
        _jsonDirectory = Application.dataPath + "/Enemies/Paths";
        if (!Directory.Exists(_jsonDirectory))
            Directory.CreateDirectory(_jsonDirectory);

        //Debug.Log(_jsonDirectory);
    }

    private void Start()
    {
        if (this.GetComponent<CanvasGroup>().alpha == 0)
            gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_pathDataList != null && _pathDataList.data != null && _pathDataList.data.Count > 0)
            {
                Debug.Log($"PathDataList contains {_pathDataList.data.Count} paths:");

                for (int i = 0; i < _pathDataList.data.Count; i++)
                {
                    PathData path = _pathDataList.data[i];
                    Debug.Log($"Path {i + 1}: Position (x: {path.pos[0]}, y: {path.pos[1]}, z: {path.pos[2]}), ScaleMultiplier: {path.scale}");
                }
            }
        }
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
        if (!string.IsNullOrEmpty(json))
        {
            _jsonPath = $"{_jsonDirectory}/{json}";
            string _getFile = $"{_jsonPath}.json";
            //Debug.Log(_getFile);

            if (File.Exists(_getFile))
            {
                fileProcessPulse($"[{json}.json] FILE LOADED!");
                _createButtonIfEmpty.SetActive(false);
                _editablePanel.SetActive(true);
                _savePanel.SetActive(true);

                _pathDataList.data = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText(_getFile));
            }
            else
            {
                fileProcessPulse("NO FILE FOUND!");
                _createButtonIfEmpty.SetActive(true);
                _editablePanel.SetActive(false);
                _savePanel.SetActive(false);
            }
        }
        else
        {
            _jsonPath = "";

            _editablePanel.SetActive(false);
            _savePanel.SetActive(false);
            _createButtonIfEmpty.SetActive(false);
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

            fileProcessPulse($"New .json file created", 2f);
            _editablePanel.SetActive(true);
            _savePanel.SetActive(true);
            _createButtonIfEmpty.SetActive(false);
        }
    }


    public void addWaypoint()
    {
        //Debug.Log("Added");
    }

    public void readSavePath(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            _savePath = $"{_jsonDirectory}/{path}";
        }
        else
            _savePath = _jsonPath;

        //Debug.Log(_savePath);
    }

    public void saveJson()
    {
        if (string.IsNullOrEmpty(_savePath))
            _savePath = _jsonPath;

        if (!string.IsNullOrEmpty(_savePath))
        {
            //Debug.Log($"Saved as {_savePath}.json");

            File.WriteAllText($"{_savePath}.json", JsonConvert.SerializeObject(_pathDataList.data, Formatting.Indented));

            fileProcessPulse($"Saved as {_savePath}.json");
        }
    }

    private void fileProcessPulse(string text, float fadeTime = 1.5f)
    {
        if (_announcerTween != null && _announcerTween.IsActive())
            _announcerTween.Kill();

        _announcer.text = "-> " + text;
        _announcer.GetComponent<CanvasGroup>().alpha = 1;
        _announcerTween = _announcer.GetComponent<CanvasGroup>().DOFade(0, fadeTime);
    }

    /////////////////////////////////////////////////////////////////////////////
    //Class
    public class PathData
    {
        [JsonProperty("Position")]
        public float[] pos { get; set; }

        [JsonProperty("Scale Multiplier")]
        public float scale { get; set; }

        public PathData(Vector3 _position = new Vector3(), float _scaleMultiplier = 1f)
        {
            pos = new float[] {_position.x, _position.y, _position.z };
            scale = _scaleMultiplier;
        }
    }

    public class PathDataList
    {
        public List<PathData> data = new List<PathData>();
    }
}