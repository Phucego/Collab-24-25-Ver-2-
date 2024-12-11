using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leper : MonoBehaviour
{
    //public static Leper Instance;

    //[HideInInspector] List<Graphic> _objects = new List<Graphic>();
    //[HideInInspector] List<float> _starts = new List<float>();
    //[HideInInspector] List<float> _ends = new List<float>();
    //[HideInInspector] List<float> _durations = new List<float>();

    //private bool isFading = false;
    //private float elapsedTime = 0f;

    //void Awake()
    //{
    //    if (Instance == null)
    //        Instance = this;
    //    else
    //        Destroy(gameObject);
    //}

    //void Update()
    //{
    //    if (_objects.Count > 0)
    //    {
    //        elapsedTime += Time.deltaTime;
            
    //        float t = Mathf.Clamp01(elapsedTime / _durations);

    //        for (int i = 0; i < targetUIElements.Count; i++)
    //        {
    //            Graphic uiElement = targetUIElements[i];
    //            if (uiElement != null)
    //            {
    //                Color currentColor = uiElement.color;
    //                currentColor.a = Mathf.Lerp(startAlphas[i], endAlphas[i], t);
    //                uiElement.color = currentColor;
    //            }
    //        }
    //    }
    //}

    //public void doTweenAlpha(Graphic uiObject, float start, float end, float duration)
    //{
    //    if (_objects.Contains(uiObject))
    //    {
    //        _objects.Add(uiObject);
    //        _starts.Add(start);
    //        _ends.Add(end);  
    //        _durations.Add(duration);
    //    }
    //    else
    //    {
    //        int i = _objects.IndexOf(uiObject);
    //        _starts[i] = start;
    //        _ends[i] = end;
    //        _durations[i] = end;
    //    }
    //}
}