using UnityEngine;

namespace Interfaces
{
    public interface IUnit
    {
        bool IsIdle();
        void MoveToResource(Vector3 pos, float stopDist, System.Action onArrived);
        void PlayAnim(Vector3 lookAtPos, System.Action onCompleted);

    

    }
}
