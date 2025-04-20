using UnityEngine;

[System.Serializable]
public class SlideshowLevelData
{
    public Sprite levelPreview;
    public string displayName;
    public SceneField scene;
    public bool isTutorial = true;
    public bool isLockedInitially = false; // New boolean to indicate if level is locked at start
}