using Interfaces;
using UnityEngine;

namespace Unit_AI
{
    public class FarmerAI : MonoBehaviour 
    {
        private IUnit _unit;
        [SerializeField] private Transform goldNodePos;
        [SerializeField] private Transform storagePos;

        void Awake()
        {
            
            _unit = gameObject.GetComponent<IUnit>();
            // Start moving to the gold resource position
            _unit?.MoveToResource(goldNodePos.position, 10f, () =>   //Check if the unit is null or not ?
            {
                // Start playing anim when the unit arrived at position
                _unit.PlayAnim(goldNodePos.transform.position, () =>
                {
                    // Move back to storage when finished
                    _unit.MoveToResource(storagePos.position, 5f, () => { });  
                });
            });
            
            
        }
    
    
    }
}
