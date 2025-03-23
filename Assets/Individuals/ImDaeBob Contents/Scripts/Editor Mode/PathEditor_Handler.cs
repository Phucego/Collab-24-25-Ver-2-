using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
using System;
using Unity.VisualScripting;

public class PathEditor_Handler : MonoBehaviour
{
    //---------------------------------------------------------------- < VARIABLES > ----------------------------------------------------------------//
    // [ Json ] //
    [Header("Toggle for Testing Mode (Editor Only)")]
    [SerializeField] bool _jsonReadInEditor = true;
    private string _jsonDirectory;

    // [ Data Process ] //
    private List<string> _jsonFiles = new List<string>();
    private PathDataList _daeJson = new PathDataList();
    private List<dataStruct> _dataList = new List<dataStruct>();

    // [ References ] //
    [Header("References")]
    [SerializeField] GameObject _selectionPanel;
    [Header("")]
    [SerializeField] Button _return;
    [Header("")]
    [SerializeField] TMP_Dropdown _path;
    [SerializeField] TMP_InputField _saveAs;
    [Header("")]
    [SerializeField] TMP_Text _debugLog;
    private Coroutine _clearTextCoroutine;
    [Header("")]
    [SerializeField] TMP_Dropdown _dropdownBegin;
    [SerializeField] TMP_InputField _xBegin;
    [SerializeField] TMP_InputField _yBegin;
    [SerializeField] TMP_InputField _zBegin;
    [SerializeField] TMP_InputField _sBegin;
    [SerializeField] Button _pickPointBegin;
    [Header("")]
    [SerializeField] TMP_Dropdown _dropdownEnd;
    [SerializeField] TMP_InputField _xEnd;
    [SerializeField] TMP_InputField _yEnd;
    [SerializeField] TMP_InputField _zEnd;
    [SerializeField] TMP_InputField _sEnd;
    [SerializeField] Button _pickPointEnd;

    // [ Accessibility ] //
    List<TMP_InputField> _inputFieldList = new List<TMP_InputField>();
    int _iField = 0;
    bool _isEditing = false;

    // [ Gizmo ] //
    [Header("Visualizer")]
    [SerializeField] GameObject _pointObject;
    List<GameObject> _pointObjectList = new List<GameObject>();
    private bool _drawGizmo = false;
    [SerializeField] CanvasGroup _instruction;
    [SerializeField] Toggle _allGizmo;
    private bool _worldSelect = false;
    private int _pointType = 0;
    private int _totalPoint = 0;

    // [ Transition ] //
    [Header("Transition")]
    [SerializeField] GameObject _transition;

    //-------------------------------------------------------------- < MAIN FUNCTIONS > --------------------------------------------------------------//
    private void Start()
    {
        _inputFieldList.Add(_xBegin);
        _inputFieldList.Add(_yBegin);
        _inputFieldList.Add(_zBegin);
        _inputFieldList.Add(_sBegin);
        _inputFieldList.Add(_xEnd);
        _inputFieldList.Add(_yEnd);
        _inputFieldList.Add(_zEnd);
        _inputFieldList.Add(_sEnd);
    }

    void OnEnable()
    {
        this.gameObject.GetComponent<CanvasGroup>().alpha = 1;

        #if UNITY_EDITOR
            if (_jsonReadInEditor)
                _jsonDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Paths"); // Editors
            else
                _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Final Build
        #else
            _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Works in Final Build
        #endif

        SearchFiles();
    }

    void Update()
    {
        if (_worldSelect)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                _worldSelect = false;
                _instruction.DOFade(0f, 0.25f);
                return;
            }

            Vector3 _getCusorToWorld = CursorToWorld();
            float _xF = (float)Math.Round(_getCusorToWorld.x, 3);
            float _yF = (float)Math.Round(_getCusorToWorld.y, 3);
            float _zF = (float)Math.Round(_getCusorToWorld.z, 3);
            switch (_pointType)
            {
                case 0:
                    _xBegin.text = _xF.ToString();
                    _yBegin.text = _yF.ToString();
                    _zBegin.text = _zF.ToString();
                    break;
                case 1:
                    _xEnd.text = _xF.ToString();
                    _yEnd.text = _yF.ToString();
                    _zEnd.text = _zF.ToString();
                    break;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
            {
                _worldSelect = false;
                this.gameObject.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
                _instruction.DOFade(0f, 0.25f);

                switch (_pointType)
                {
                    case 0:
                        _dataList[_dropdownBegin.value].Data[0] = _xF;
                        _dataList[_dropdownBegin.value].Data[1] = _yF;
                        _dataList[_dropdownBegin.value].Data[2] = _zF;
                        _pointObjectList[_dropdownBegin.value].transform.position = new Vector3(_xF, _yF, _zF);
                        break;
                    case 1:
                        _dataList[_dropdownEnd.value + 1].Data[0] = _xF;
                        _dataList[_dropdownEnd.value + 1].Data[1] = _yF;
                        _dataList[_dropdownEnd.value + 1].Data[2] = _zF;
                        _pointObjectList[_dropdownEnd.value + 1].transform.position = new Vector3(_xF, _yF, _zF);
                        break;
                }
            }
        }

        if (_isEditing)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                EventSystem.current.SetSelectedGameObject(null);

                if (Input.GetKey(KeyCode.LeftShift))
                    _iField = (_iField - 1) % _inputFieldList.Count;
                else
                    _iField = (_iField + 1) % _inputFieldList.Count;
                EventSystem.current.SetSelectedGameObject(_inputFieldList[_iField].gameObject);
            }
        }
    }

    //-------------------------------------------------------------- < MINI FUNCTIONS > --------------------------------------------------------------//
    // [ BUTTON HANDLERS ] //
    public void ReturnToSelection()
    {
        _transition.SetActive(true);
        StartCoroutine(EndTransition());
    }

    bool _isFollowed = false;
    public void BeginSelected(int v)
    {
        if (!_isFollowed)
        {
            _isFollowed = true;

            _xBegin.text = _dataList[v].Data[0].ToString();
            _yBegin.text = _dataList[v].Data[1].ToString();
            _zBegin.text = _dataList[v].Data[2].ToString();
            _sBegin.text = _dataList[v].Data[3].ToString();

            _dropdownEnd.value = v;
            _dropdownEnd.RefreshShownValue();
            _xEnd.text = _dataList[v + 1].Data[0].ToString();
            _yEnd.text = _dataList[v + 1].Data[1].ToString();
            _zEnd.text = _dataList[v + 1].Data[2].ToString();
            _sEnd.text = _dataList[v + 1].Data[3].ToString();

            VisualizeSelectedPoints();

            _isFollowed = false;
        }
    }

    public void EndSelected(int v)
    {
        v++;
        if (!_isFollowed)
        {
            _isFollowed = true;

            _dropdownBegin.value = v - 1;
            _dropdownBegin.RefreshShownValue();
            _xBegin.text = _dataList[v - 1].Data[0].ToString();
            _yBegin.text = _dataList[v - 1].Data[1].ToString();
            _zBegin.text = _dataList[v - 1].Data[2].ToString();
            _sBegin.text = _dataList[v - 1].Data[3].ToString();

            _xEnd.text = _dataList[v].Data[0].ToString();
            _yEnd.text = _dataList[v].Data[1].ToString();
            _zEnd.text = _dataList[v].Data[2].ToString();
            _sEnd.text = _dataList[v].Data[3].ToString();

            VisualizeSelectedPoints();

            _isFollowed = false;
        }
    }

    public void SelectWorldPoint()
    {
        if (!_worldSelect)
        {
            UnityEngine.UI.Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>();
            if (selectedButton == _pickPointBegin)
                _pointType = 0;
            else if (selectedButton == _pickPointEnd)
                _pointType = 1;

            _worldSelect = true;
            _instruction.DOFade(1f, 0.25f);
        }
        else
        {
            _worldSelect = false;
            _instruction.DOFade(0f, 0.25f);
        }
    }

    public void AddWaypoint()
    {
        //Debug.Log("Added");
        DebugLog("Added new Waypoint!");

        _dataList.Add(new dataStruct($"Point {_dataList.Count.ToString()}", 0, 0, 0, 1));
        _dropdownBegin.AddOptions(new List<string>() { "Point " + (_dropdownBegin.options.Count).ToString() });
        _dropdownEnd.AddOptions(new List<string>() { "Point " + (_dropdownEnd.options.Count + 1).ToString() });

        dataStruct data = _dataList[_dataList.Count - 1];
        if (_pointObjectList.Count < _dataList.Count)
        {
            GameObject _point = Instantiate(_pointObject);
            _pointObjectList.Add(_point);
            _totalPoint++;
        }

        _pointObjectList[_dataList.Count - 1].transform.position = new Vector3(data.Data[0], data.Data[1], data.Data[2]);
        _pointObjectList[_dataList.Count - 1].transform.localScale = new Vector3(3f, 3f, 3f) * data.Data[3];
        _pointObjectList[_dataList.Count - 1].name = data.Name;

        MatchData();
        VisualizeSelectedPoints();
    }

    public void RemoveLastWaypoint()
    {
        if (_dataList.Count > 2)
        {
            DebugLog("Removed the last Waypoint!");

            _pointObjectList[_dataList.Count - 1].transform.position = new Vector3(0, 0, 0);
            _pointObjectList[_dataList.Count - 1].transform.localScale = new Vector3(3f, 3f, 3f);
            _pointObjectList[_dataList.Count - 1].SetActive(false);

            _dataList.RemoveAt(_dataList.Count - 1);
            _dropdownBegin.options.RemoveAt(_dropdownBegin.options.Count - 1);
            _dropdownEnd.options.RemoveAt(_dropdownEnd.options.Count - 1);

            if (_dropdownEnd.value >= _dropdownEnd.options.Count - 1)
            {
                _dropdownBegin.value = _dropdownBegin.options.Count - 1;
                _dropdownEnd.value = _dropdownEnd.options.Count - 1;
                MatchData();
            }
        }
        else
            DebugLog("There is not enough waypoint to initiate removal.");
    }

    // [ DATA HANDLERS ] //
    private void SearchFiles()
    {
        _path.ClearOptions();
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

        //Add all found path.json files to dropdown
        _path.AddOptions(new List<string>(_jsonFiles));

        LoadPath();
    }

    public void LoadPath()
    {
        _daeJson.list.Clear();
        _dataList.Clear();
        _dropdownBegin.ClearOptions();
        _dropdownEnd.ClearOptions();

        string _getFile = $"{_jsonDirectory}/{_jsonFiles[_path.value].ToString()}.JSON";
        _daeJson.list = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText(_getFile));

        DebugLog($"Loaded data from {_path.options[_path.value].text}.json");

        List<string> _beginOptions = new List<string>();
        List<string> _endOptions = new List<string>();

        for (int i = 0; i < _daeJson.list.Count; i++)
        {
            PathData path = _daeJson.list[i];
            _dataList.Add(new dataStruct(path.name, path.data[0], path.data[1], path.data[2], path.data[3]));

            if (i < _daeJson.list.Count - 1)
                _beginOptions.Add(path.name);
            if (i > 0)
                _endOptions.Add(path.name);
        }

        _dropdownBegin.AddOptions(_beginOptions);
        _dropdownEnd.AddOptions(_endOptions);

        while (_totalPoint < _dataList.Count)
        {
            GameObject _point = Instantiate(_pointObject);

            _pointObjectList.Add(_point);
            _totalPoint++;
        }

        for (int i = 0; i < _dataList.Count; i++)
        {
            dataStruct data = _dataList[i];
            _pointObjectList[i].transform.position = new Vector3(data.Data[0], data.Data[1], data.Data[2]);
            _pointObjectList[i].transform.localScale = new Vector3(3f, 3f, 3f) * data.Data[3];
            _pointObjectList[i].name = data.Name;
        }
        VisualizeSelectedPoints();

        _xBegin.text = _dataList[0].Data[0].ToString();
        _yBegin.text = _dataList[0].Data[1].ToString();
        _zBegin.text = _dataList[0].Data[2].ToString();
        _sBegin.text = _dataList[0].Data[3].ToString();

        _xEnd.text = _dataList[1].Data[0].ToString();
        _yEnd.text = _dataList[1].Data[1].ToString();
        _zEnd.text = _dataList[1].Data[2].ToString();
        _sEnd.text = _dataList[1].Data[3].ToString();

        _drawGizmo = true;
    }

    public void SavePath()
    {

        if (string.IsNullOrEmpty(_saveAs.text)) // No Input -> Save as the same current selected file
        {
            string _curPath = $"{_jsonDirectory}/{_path.options[_path.value].text.ToUpper()}.JSON";
            File.WriteAllText(_curPath, JsonConvert.SerializeObject(_dataList, Formatting.Indented));
            
            DebugLog($"Saved {_path.options[_path.value].text}.json");
        }
        else
        {
            _saveAs.text = _saveAs.text.ToUpper();
            string _getPath = $"{_jsonDirectory}/{_saveAs.text}.JSON";
            if (_jsonFiles.Contains($"{_saveAs.text}")) // Have input but the file already existed
            {
                File.WriteAllText(_getPath, JsonConvert.SerializeObject(_dataList, Formatting.Indented));

                DebugLog($"Saved current data as {_saveAs.text}.json");
            }
            else // Have input but the file isn't existed
            {
                if (_jsonFiles.Count == 0)
                {
                    List<PathData> newData = new List<PathData>
                    {
                        new PathData("Point 0"),
                        new PathData("Point 1")
                    };
                    File.WriteAllText(_getPath, JsonConvert.SerializeObject(newData, Formatting.Indented));

                    DebugLog($"Create the first path ever: {_saveAs.text}");
                }
                else 
                {
                    File.WriteAllText(_getPath, JsonConvert.SerializeObject(_dataList, Formatting.Indented));

                    DebugLog($"Saved current data as a new file: {_saveAs.text}.json");
                }
                SearchFiles();
                this.gameObject.GetComponent<Animator>().SetTrigger("New");
            }
        }
        _saveAs.text = null;
    }

    public void CreateNewPath()
    {
        if (!string.IsNullOrEmpty(_saveAs.text))
        {
            _saveAs.text = _saveAs.text.ToUpper();
            string _getPath = $"{_jsonDirectory}/{_saveAs.text}.JSON";
            if (!_jsonFiles.Contains($"{_saveAs.text}"))
            {
                List<PathData> newData = new List<PathData>
                {
                    new PathData("Point 0"),
                    new PathData("Point 1")
                };
                File.WriteAllText(_getPath, JsonConvert.SerializeObject(newData, Formatting.Indented));

                DebugLog($"Created a brand new path: {_saveAs.text}");

                SearchFiles();
                this.gameObject.GetComponent<Animator>().SetTrigger("New");
                _saveAs.text = null;
            }
            else
                DebugLog("A path with the same name has already existed");
        }
        else
            DebugLog("There is no name for the new path file!!");
    }

    public void DeleteCurPath()
    {
        if (_jsonFiles.Count == 0)
        {
            DebugLog("No paths available to delete.");
            return;
        }

        string _recyleBin = $"{_jsonDirectory}/RecycleBin/";
        if (!Directory.Exists(_recyleBin))        
            Directory.CreateDirectory(_recyleBin);

        string _curPath = $"{_jsonDirectory}/{_jsonFiles[_path.value]}.JSON";
        if (File.Exists(_curPath))
        {
            string _deletedPath = $"{_recyleBin}/{_jsonFiles[_path.value]}.JSON";
            if (File.Exists(_deletedPath))
                File.Delete(_deletedPath);
            File.Move(_curPath, _deletedPath); // Move the file to the "Recycle Bin" instead of permanently deleting it

            _jsonFiles.RemoveAt(_path.value);
            _path.options.RemoveAt(_path.value);
            _path.RefreshShownValue();

            if (_jsonFiles.Count > 0)
            {
                _path.value = 0;
                LoadPath();
            }
        }
        else
            DebugLog("Selected path file does not exist.");
    }

    // [ ACCESSIBILITIES ] //
    private string _cachedValue = "";
    public void CacheCurValue(string value)
    {
        _isEditing = true;

        if (!string.IsNullOrEmpty(value))
            _cachedValue = value;
    }

    public void GetNewValue(string value)
    {
        _isEditing = false;

        TMP_InputField selectedField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
        if (!string.IsNullOrEmpty(value) && float.TryParse(value, out float result))
        {
            CacheCurValue(value);
            float _numValue = float.Parse(value);

            //Is there no better way to do this? T-T
            if (selectedField == _xBegin)
            {
                _iField = 0;
                _dataList[_dropdownBegin.value].Data[0] = _numValue;
                _pointObjectList[_dropdownBegin.value].transform.position = new Vector3(_numValue, _pointObjectList[_dropdownBegin.value].transform.position.y, _pointObjectList[_dropdownBegin.value].transform.position.z);
            }
            else if (selectedField == _yBegin)
            {
                _iField = 1;
                _dataList[_dropdownBegin.value].Data[1] = _numValue;
                _pointObjectList[_dropdownBegin.value].transform.position = new Vector3(_pointObjectList[_dropdownBegin.value].transform.position.x, _numValue, _pointObjectList[_dropdownBegin.value].transform.position.z);
            }
            else if (selectedField == _zBegin)
            {
                _iField = 2;
                _dataList[_dropdownBegin.value].Data[2] = _numValue;
                _pointObjectList[_dropdownBegin.value].transform.position = new Vector3(_pointObjectList[_dropdownBegin.value].transform.position.x, _pointObjectList[_dropdownBegin.value].transform.position.y, _numValue);
            }
            else if (selectedField == _sBegin)
            {
                _iField = 3;
                _dataList[_dropdownBegin.value].Data[3] = _numValue;
                _numValue *= 3;
                _pointObjectList[_dropdownBegin.value].transform.localScale = new Vector3(_numValue, _numValue, _numValue);
            }
            else if (selectedField == _xEnd)
            {
                _iField = 4;
                _dataList[_dropdownEnd.value + 1].Data[0] = _numValue;
                _pointObjectList[_dropdownEnd.value + 1].transform.position = new Vector3(_numValue, _pointObjectList[_dropdownEnd.value + 1].transform.position.y, _pointObjectList[_dropdownEnd.value + 1].transform.position.z);
            }
            else if (selectedField == _yEnd)
            {
                _iField = 5;
                _dataList[_dropdownEnd.value + 1].Data[1] = _numValue;
                _pointObjectList[_dropdownEnd.value + 1].transform.position = new Vector3(_pointObjectList[_dropdownEnd.value + 1].transform.position.x, _numValue, _pointObjectList[_dropdownEnd.value + 1].transform.position.z);
            }
            else if (selectedField == _zEnd)
            {
                _iField = 6;
                _dataList[_dropdownEnd.value + 1].Data[2] = _numValue;
                _pointObjectList[_dropdownEnd.value + 1].transform.position = new Vector3(_pointObjectList[_dropdownEnd.value + 1].transform.position.x, _pointObjectList[_dropdownEnd.value + 1].transform.position.y, _numValue);
            }
            else if (selectedField == _sEnd)
            {
                _iField = 7;
                _dataList[_dropdownEnd.value + 1].Data[3] = _numValue;
                _numValue *= 3;
                _pointObjectList[_dropdownEnd.value + 1].transform.localScale = new Vector3(_numValue, _numValue, _numValue);
            }

            return;
        }
        selectedField.text = _cachedValue;
    }

    private void DebugLog(string message, float duration = 3f, float interval = 0.02f)
    {
        _debugLog.text = $">/ {message}";

        if (_clearTextCoroutine != null)
            StopCoroutine(_clearTextCoroutine);
        _clearTextCoroutine = StartCoroutine(ClearDebugLog(duration, interval));
    }

    // [ PROCCESSORS ] //
    private void MatchData()
    {
        int v = _dataList.Count - 1;
        _dropdownBegin.value = v - 1;
        _dropdownBegin.RefreshShownValue();
        _xBegin.text = _dataList[v - 1].Data[0].ToString();
        _yBegin.text = _dataList[v - 1].Data[1].ToString();
        _zBegin.text = _dataList[v - 1].Data[2].ToString();
        _sBegin.text = _dataList[v - 1].Data[3].ToString();

        _dropdownEnd.value = v - 1;
        _dropdownEnd.RefreshShownValue();
        _xEnd.text = _dataList[v].Data[0].ToString();
        _yEnd.text = _dataList[v].Data[1].ToString();
        _zEnd.text = _dataList[v].Data[2].ToString();
        _sEnd.text = _dataList[v].Data[3].ToString();
    }

    private void OnDrawGizmos()
    {
        if (_allGizmo.isOn)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < _dataList.Count - 1; i++)
                Gizmos.DrawLine(_pointObjectList[i].transform.position, _pointObjectList[i + 1].transform.position);
        }
        else
        {
            if (_drawGizmo && _pointObjectList.Count > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_pointObjectList[_dropdownBegin.value].transform.position, _pointObjectList[_dropdownBegin.value + 1].transform.position);
            }
        }
    }

    private void VisualizeSelectedPoints()
    {
        for (int i=0; i<_totalPoint; i++)
        {   
            if (i < _dataList.Count)
            {
                _pointObjectList[i].SetActive(true);
                _pointObjectList[i].GetComponent<Renderer>().material.color = new Color(1, 0.25f, 0.25f);
            }
            else
                _pointObjectList[i].SetActive(false);
        }

        _pointObjectList[_dropdownBegin.value].GetComponent<Renderer>().material.color = Color.green;
        _pointObjectList[_dropdownBegin.value + 1].GetComponent<Renderer>().material.color = Color.cyan;
    }

    private Vector3 CursorToWorld()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out RaycastHit hit))
            return hit.point;

        return Vector3.zero;
    }

    // [ IENUMERATOR HANDLERS ] //
    IEnumerator EndTransition()
    {
        _drawGizmo = false;

        yield return new WaitForSeconds(0.5f);
        foreach (GameObject point in _pointObjectList)
            point.SetActive(false);
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
    public class PathDataList
    {
        public List<PathData> list = new List<PathData>();
    }

    public class PathData //For Json
    {
        [JsonProperty("Name")]
        public string name { get; set; }
        [JsonProperty("Data")]
        public float[] data { get; set; }

        public PathData(string _name = "Point ?", Vector3 _position = new Vector3(), float _size = 1f)
        {
            name = _name;
            data = new float[] { _position.x, _position.y, _position.z, _size };
        }
    }

    public class dataStruct //For Script
    {
        public string Name { get; set; }
        public float[] Data { get; set; }

        public dataStruct(string _name, float _x, float _y, float _z, float _size)
        {
            Name = _name;
            Data = new float[] { _x, _y, _z, _size };
        }
    }
}
