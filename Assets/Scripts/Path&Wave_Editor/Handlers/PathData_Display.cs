using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Search;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class PathData_Display : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _Option;

    [Header("Display Coordinate:")]
    [SerializeField] TMP_InputField _X;
    [SerializeField] TMP_InputField _Y;
    [SerializeField] TMP_InputField _Z;

    [Header("Display Size:")]
    [SerializeField] TMP_InputField _Scale;

    private List<dataStruct> _dataList = new List<dataStruct>();

    void Start()
    {
        _Option.onValueChanged.AddListener(
            delegate { 
                ItemSelected(_Option);
            });
    }

    public void ResetList()
    {
        _dataList.Clear();
        _Option.ClearOptions();
    }

    public void AddData(string name, float x, float y, float z, float scale)
    {
        _Option.AddOptions(new List<string>() { name });
        _dataList.Add(new dataStruct(x, y, z, scale));

        if (_dataList.Count == 1)
        {
            _X.text = x.ToString();
            _Y.text = y.ToString();
            _Z.text = z.ToString();
            _Scale.text = scale.ToString();
        }
    }

    void ItemSelected(TMP_Dropdown dropdown)
    {
        int i = dropdown.value;
    
        _X.text = _dataList[i].Position[0].ToString(); _X.ForceLabelUpdate();
        _Y.text = _dataList[i].Position[1].ToString(); _Y.ForceLabelUpdate();
        _Z.text = _dataList[i].Position[2].ToString(); _Z.ForceLabelUpdate();
        _Scale.text = _dataList[i].ScaleMultiplier.ToString(); _Scale.ForceLabelUpdate();
    }

    public struct dataStruct
    {
        public float[] Position { get; set; }
        public float ScaleMultiplier { get; set; }

        public dataStruct(float x, float y, float z, float scaleMultiplier)
        {
            Position = new float[] { x, y, z };
            ScaleMultiplier = scaleMultiplier;
        }
    }
}
