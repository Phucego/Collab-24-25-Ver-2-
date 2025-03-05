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

public class PathEditor_Handler : MonoBehaviour
{
    //---------------------------------------------------------------- < VARIABLES > ----------------------------------------------------------------//
    // [ Json ] //
    [Header("Toggle for Testing Mode (Editor Only)")]
    [SerializeField] bool _jsonReadInEditor = true;
    private string _jsonDirectory;
    private string _savePath;

    // [ Data Process ] //
    private List<string> _jsonFiles = new List<string>();
    private PathDataList _jsonData = new PathDataList();
    private List<dataStruct> _dataList = new List<dataStruct>();

    // [ References ] //
    [Header("References")]
    [SerializeField] GameObject _selectionPanel;
    [Header("")]
    [SerializeField] Button _return;
    [Header("")]
    [SerializeField] TMP_Dropdown _path;
    [SerializeField] TMP_InputField _saveAs;
    [SerializeField] Button _saveButton;

    // [ Accessibility ] //
    List<TMP_InputField> _inputFieldList = new List<TMP_InputField>();
    int _iField = 0;
    bool _isEditing = false;

    // [ Gizmo ] //
    [Header("Visualizer")]
    [SerializeField] GameObject _pointObject;
    [SerializeField] GameObject _unselectPointObject;
    List<GameObject> _pointObjectList = new List<GameObject>();
    [SerializeField] Toggle _allGizmo;

    // [ Transition ] //
    [Header("Transition")]
    [SerializeField] GameObject _transition;

    //-------------------------------------------------------------- < MAIN FUNCTIONS > --------------------------------------------------------------//
    void OnEnable()
    {
        #if UNITY_EDITOR
        if (_jsonReadInEditor)
            _jsonDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Paths"); // Editors
        else
            _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Final Build
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
                _jsonFiles.Add(fileName);
            }
        }

        //Add all found path.json files to dropdown
        _path.AddOptions(new List<string>(_jsonFiles));
    }

    void Update()
    {
        if (!_isEditing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)))
        {
            _transition.SetActive(true);
            StartCoroutine(EndTransition());
        }
    }

    //-------------------------------------------------------------- < MINI FUNCTIONS > --------------------------------------------------------------//
    // [ BUTTON HANDLERS ] //
    public void ReturnToSelection()
    {
        _transition.SetActive(true);
        StartCoroutine(EndTransition());
    }

    public void SavePath(string value)
    {
        if (_jsonFiles.Contains(value))
        {

        }
        else
        {
            if (!string.IsNullOrEmpty(value))
            {
                //Debug.Log($"Created {value}.json");
                List<PathData> newData = new List<PathData>
            {
                new PathData("Point 0"),
                new PathData("Point 1")
            };

                File.WriteAllText(value + ".json", JsonConvert.SerializeObject(newData, Formatting.Indented));
                _jsonData.list = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText($"{value}.json"));


            }
        }
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

        }
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

    //-------------------------------------------------------------- < Class & Struct > --------------------------------------------------------------//
    public class PathDataList
    {
        public List<PathData> list = new List<PathData>();
    }

    public class PathData
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

    public class dataStruct
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
