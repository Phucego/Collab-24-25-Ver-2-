using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType
{
    Enemy,
    Tower,
    None
}
public interface I_Interactable
{
    void Interact(Camera camera) { }
    void Deselect() { }
    InteractType DetectType() { return InteractType.None; }
}
