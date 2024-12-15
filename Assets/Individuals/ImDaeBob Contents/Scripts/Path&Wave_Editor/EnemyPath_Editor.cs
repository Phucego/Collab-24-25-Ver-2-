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
using System;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using UnityEditor.UI;
using UnityEditor;

public class EnemyPath_Editor : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////
    //Variables:
    private string _jsonDirectory;
    private string _savePath;

    //Data Process
    private List<dataStruct> _dataList = new List<dataStruct>();


    //Links
    [Header("Loader")]
    [SerializeField] TMP_Text _announcer;
    private Tween _announcerTween;
    [SerializeField] GameObject _createButtonIfEmpty;
    private PathDataList _pathDataJSON = new PathDataList();

    [Header("The Editable Editor")]
    [SerializeField] GameObject _editablePanel;
    [SerializeField] GameObject _savePanel;

    [Header("Waypoints")]
    [SerializeField] GameObject _beginPoint;
    [SerializeField] GameObject _endPoint;
    private string _jsonPath;
    private bool _drawGizmo = false;

    [Header("")]
    [SerializeField] TMP_Dropdown _dropdownBegin;
    [SerializeField] TMP_InputField _nameBegin;
    [SerializeField] TMP_InputField _xBegin;
    [SerializeField] TMP_InputField _yBegin;
    [SerializeField] TMP_InputField _zBegin;
    [SerializeField] TMP_InputField _sBegin;
    [SerializeField] UnityEngine.UI.Button _buttonBegin;

    [Header("")]
    [SerializeField] TMP_Dropdown _dropdownEnd;
    [SerializeField] TMP_InputField _nameEnd;
    [SerializeField] TMP_InputField _xEnd;
    [SerializeField] TMP_InputField _yEnd;
    [SerializeField] TMP_InputField _zEnd;
    [SerializeField] TMP_InputField _sEnd;
    [SerializeField] UnityEngine.UI.Button _buttonEnd;

    List<TMP_InputField> _inputFieldList = new List<TMP_InputField>();
    int _iField = 0;
    bool _isEditing = false;

    [Header("How to Set Position")]
    [SerializeField] CanvasGroup _howToUse;
    private bool _worldSelect = false;
    private int _pointType = 0;

    [Header("Visualizer")]
    [SerializeField] GameObject _pointObject;
    List<GameObject> _pointObjectList = new List<GameObject>();
    [SerializeField] UnityEngine.UI.Toggle _allGizmo;

    /////////////////////////////////////////////////////////////////////////////
    //Main Functions:
    private bool _active = false;

    void Awake()
    {
        //Only for in Editor!
        _jsonDirectory = Application.dataPath + "/Data/Enemies/Paths";
        if (!Directory.Exists(_jsonDirectory))
            Directory.CreateDirectory(_jsonDirectory);

        //Debug.Log(_jsonDirectory);
    }

    private void Start()
    {
        if (this.GetComponent<CanvasGroup>().alpha == 0)
            gameObject.SetActive(false);

        _inputFieldList.Add(_xBegin);
        _inputFieldList.Add(_yBegin);
        _inputFieldList.Add(_zBegin);
        _inputFieldList.Add(_sBegin);
        _inputFieldList.Add(_xEnd);
        _inputFieldList.Add(_yEnd);
        _inputFieldList.Add(_zEnd);
        _inputFieldList.Add(_sEnd);
    }

    private void Update()
    {
        if (_worldSelect)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                _worldSelect = false;
                this.gameObject.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
                _howToUse.DOFade(0f, 0.25f);
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
                _howToUse.DOFade(0f, 0.25f);

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

                _drawGizmo = true;
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

    public void ToggleEditor(bool mode)
    {
        _active = mode;

        if (!mode)
        {
            _drawGizmo = false;
            if (_pointObjectList.Count > 0)
            {
                foreach (GameObject _point in _pointObjectList)
                    _point.transform.localScale = Vector3.zero;
            }
        }
        else
            _drawGizmo = true;
    }

    public bool GetActive()
    {
        return _active;
    }

    /////////////////////////////////////////////////////////////////////////////
    // Buttons and On Clicked functions:
    public void ReadJSON(string json)
    {
        if (_pointObjectList.Count > 0)
        {
            foreach (GameObject _point in _pointObjectList)
                _point.transform.localScale = Vector3.zero;
        }

        if (!string.IsNullOrEmpty(json))
        {
            _jsonPath = $"{_jsonDirectory}/{json}";
            string _getFile = $"{_jsonPath}.json";
            //Debug.Log(_getFile);

            if (File.Exists(_getFile))
            {
                FileProcessPulse($"[{json}.json] FILE LOADED!");
                _createButtonIfEmpty.SetActive(false);
                _editablePanel.SetActive(true);
                _savePanel.SetActive(true);

                _pathDataJSON.list = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText(_getFile));

                ResetList();
                ConvertToEditableData();
            }
            else
            {
                FileProcessPulse("NO FILE FOUND!");

                _drawGizmo = false;
                _createButtonIfEmpty.SetActive(true);
                _editablePanel.SetActive(false);
                _savePanel.SetActive(false);
            }
        }
        else
        {
            _jsonPath = "";

            _drawGizmo = false;
            _editablePanel.SetActive(false);
            _savePanel.SetActive(false);
            _createButtonIfEmpty.SetActive(false);
        }
    }

    public void CreateJSON()
    {
        if (!string.IsNullOrEmpty(_jsonPath))
        {
            //Debug.Log($"Created {_jsonPath}.json");
            List<PathData> newData = new List<PathData>
            {
                new PathData("Point 0"),
                new PathData("Point 1")
            };

            File.WriteAllText(_jsonPath + ".json", JsonConvert.SerializeObject(newData, Formatting.Indented));
            _pathDataJSON.list = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText($"{_jsonPath}.json"));

            FileProcessPulse($"New .json file created", 2f);
            _editablePanel.SetActive(true);
            _savePanel.SetActive(true);
            _createButtonIfEmpty.SetActive(false);

            ResetList();
            ConvertToEditableData();
        }
    }

    public void AddWaypoint()
    {
        //Debug.Log("Added");
        FileProcessPulse("Added new Waypoint!");

        _dataList.Add(new dataStruct($"Point {_dataList.Count.ToString()}", 0, 0, 0, 1));
        _dropdownBegin.AddOptions(new List<string>() {"Point " + (_dropdownBegin.options.Count).ToString() });
        _dropdownEnd.AddOptions(new List<string>() { "Point " + (_dropdownEnd.options.Count + 1).ToString() });

        AddInstantiateWaypoint();

        int v = _dataList.Count-1;
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

        _drawGizmo = true;
        VisualizeSelectedPoints();
    }

    public void ReadSavePath(string path)
    {
        if (!string.IsNullOrEmpty(path))
            _savePath = $"{_jsonDirectory}/{path}";
        else
            _savePath = _jsonPath;

        //Debug.Log(_savePath);
    }

    public void SaveJson()
    {
        if (string.IsNullOrEmpty(_savePath))
            _savePath = _jsonPath;

        if (!string.IsNullOrEmpty(_savePath))
        {
            //Debug.Log($"Saved as {_savePath}.json");

            File.WriteAllText($"{_savePath}.json", JsonConvert.SerializeObject(_dataList, Formatting.Indented));

            FileProcessPulse($"Saved as {_savePath}.json");
        }
    }

    private void FileProcessPulse(string text, float fadeTime = 1.5f)
    {
        if (_announcerTween != null && _announcerTween.IsActive())
            _announcerTween.Kill();

        _announcer.text = "-> " + text;
        _announcer.GetComponent<CanvasGroup>().alpha = 1;
        _announcerTween = _announcer.GetComponent<CanvasGroup>().DOFade(0, fadeTime);
    }

    /////////////////////////////////////////////////////////////////////////////
    //Path Editable Panel Functions
    private void ResetList()
    {
        _dataList.Clear();
        _dropdownBegin.ClearOptions();
        _dropdownEnd.ClearOptions();
    }

    private void ConvertToEditableData()
    {
        List<string> _beginOptions = new List<string>();
        List<string> _endOptions = new List<string>();

        for (int i = 0; i < _pathDataJSON.list.Count; i++)
        {
            PathData path = _pathDataJSON.list[i];
            _dataList.Add(new dataStruct(path.name, path.data[0], path.data[1], path.data[2], path.data[3]));

            if (i < _pathDataJSON.list.Count - 1)
                _beginOptions.Add(path.name);
            if (i > 0)
                _endOptions.Add(path.name);
        }

        _dropdownBegin.AddOptions(_beginOptions);
        _dropdownEnd.AddOptions(_endOptions);
        IntanstiateWaypoint();
        ShowFirstOption();
        _drawGizmo = true;
    }

    private int _totalPoint = 0;
    private void IntanstiateWaypoint()
    {
        while (_totalPoint < _dataList.Count)
        {
            GameObject _point = Instantiate(_pointObject);

            dataStruct data = _dataList[_totalPoint];
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
    }
    private void AddInstantiateWaypoint()
    {
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
    }

    private void ShowFirstOption()
    {
        _xBegin.text = _dataList[0].Data[0].ToString();
        _yBegin.text = _dataList[0].Data[1].ToString();
        _zBegin.text = _dataList[0].Data[2].ToString();
        _sBegin.text = _dataList[0].Data[3].ToString();

        _xEnd.text = _dataList[1].Data[0].ToString();
        _yEnd.text = _dataList[1].Data[1].ToString();
        _zEnd.text = _dataList[1].Data[2].ToString();
        _sEnd.text = _dataList[1].Data[3].ToString();

        VisualizeSelectedPoints();
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
            _xEnd.text = _dataList[v+1].Data[0].ToString();
            _yEnd.text = _dataList[v+1].Data[1].ToString();
            _zEnd.text = _dataList[v+1].Data[2].ToString();
            _sEnd.text = _dataList[v+1].Data[3].ToString();

            _drawGizmo = true;
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

            _dropdownBegin.value = v-1;
            _dropdownBegin.RefreshShownValue();
            _xBegin.text = _dataList[v-1].Data[0].ToString();
            _yBegin.text = _dataList[v-1].Data[1].ToString();
            _zBegin.text = _dataList[v-1].Data[2].ToString();
            _sBegin.text = _dataList[v-1].Data[3].ToString();

            _xEnd.text = _dataList[v].Data[0].ToString();
            _yEnd.text = _dataList[v].Data[1].ToString();
            _zEnd.text = _dataList[v].Data[2].ToString();
            _sEnd.text = _dataList[v].Data[3].ToString();

            _drawGizmo = true;
            VisualizeSelectedPoints();

            _isFollowed = false;
        }
    }

    public void GetNewName(string name)
    {
        TMP_InputField selectedField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
        if (!string.IsNullOrEmpty(name))
        {
            if (selectedField == _nameBegin)
            {
                int i = _dropdownBegin.value;
                _dataList[i].Name = name;
                _dropdownBegin.options[i].text = name;
                _dropdownBegin.RefreshShownValue();
                if (i > 0)
                {
                    _dropdownEnd.options[i - 1].text = name;
                    _dropdownEnd.RefreshShownValue();
                }
            }
            else if (selectedField == _nameEnd)
            {
                int i = _dropdownEnd.value;
                _dataList[i + 1].Name = name;
                if (i < _dropdownBegin.options.Count - 1)
                {
                    _dropdownBegin.options[i + 1].text = name;
                    _dropdownBegin.RefreshShownValue();
                }
                _dropdownEnd.options[i].text = name;
                _dropdownEnd.RefreshShownValue();
            }

            FileProcessPulse("Name changed", 2f);
        }

        selectedField.text = null;
    }

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

    public void SelectWorldPoint()
    {
        if (!_worldSelect)
        {
            UnityEngine.UI.Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>();
            if (selectedButton == _buttonBegin)
                _pointType = 0;
            else if (selectedButton == _buttonEnd)
                _pointType = 1;

            _worldSelect = true;
            this.gameObject.GetComponent<CanvasGroup>().DOFade(0.5f, 0.25f);
            _howToUse.DOFade(1f, 0.25f);
        }
        else
        {
            _worldSelect = false;
            this.gameObject.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
            _howToUse.DOFade(0f, 0.25f);
        }
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
        foreach (GameObject _point in _pointObjectList)
            _point.GetComponent<Renderer>().material.color = new Color(1, 0.25f, 0.25f);

        _pointObjectList[_dropdownBegin.value].GetComponent<Renderer>().material.color = Color.cyan;
        _pointObjectList[_dropdownBegin.value + 1].GetComponent<Renderer>().material.color = Color.green;
    }    

    private Vector3 CursorToWorld()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out RaycastHit hit))
            return hit.point;

        return Vector3.zero;
    }
    /////////////////////////////////////////////////////////////////////////////
    //Class & Struct
    public class PathDataList
    {
        public List<PathData> list = new List<PathData>();
    }

    public class PathData
    {
        [JsonProperty("Name")]
        public string name {  get; set; }
        [JsonProperty("Data")]
        public float[] data { get; set; }

        public PathData(string _name = "Point ?", Vector3 _position = new Vector3(), float _scaleMultiplier = 1f)
        {
            name = _name;
            data = new float[] {_position.x, _position.y, _position.z, _scaleMultiplier };
        }
    }

    public class dataStruct
    {
        public string Name { get; set;}
        public float[] Data { get; set; }

        public dataStruct(string _name, float _x, float _y, float _z, float _scaleMultiplier)
        {
            Name = _name;
            Data = new float[] { _x, _y, _z, _scaleMultiplier };
        }
    }
}