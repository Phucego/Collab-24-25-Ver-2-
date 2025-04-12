using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using static PathEditor_Handler;

public class EnemyBehavior : MonoBehaviour, I_GetType, I_Damagable
{
    private BaseEnemySO data;

    // [ DIRECTORY ] //
    private string _pathDirectory;

    // [ ENEMY'S UI ] //
    private Enemy_HPBar _bar;

    // [ ENEMY'S DATA ] //
    private int _reward;
    private float _health, _speed, _acceleration, _gravity, _heightAboveGround;

    // [ Rigidbody & CharacterController ] //
    private Rigidbody _rb; //For external forces like knockbacks and explosion forces
    private CharacterController _ctrl; //For moving basically

    private int _dataIndex = 1;
    private List<dataStruct> _dataList = new List<dataStruct>();

    private bool _isMovable = false;
    private Vector3 _startPosition, _targetPosition, _velocity;
    private float _moveTime = 0f, _curSpeed = 0f;

    //----------------------------------------------------------------- < SET UP > -----------------------------------------------------------------//
    void Awake()
    {
        #if UNITY_EDITOR
        _pathDirectory = Path.Combine(Application.dataPath, "Data/Enemies/Paths"); // Editors
        #else
            _jsonDirectory = Path.Combine(Application.streamingAssetsPath, "JsonData"); // Works in Final Build
        #endif
    }

    void OnDisable()
    {
        _isMovable = false;
        StopAllCoroutines();
        _dataIndex = 1;
        _dataList.Clear();
    }

    void Start()
    {
        _bar = GetComponentInChildren<Enemy_HPBar>();

        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }
    
    //-------------------------------------------------------------- < ACCESSORS > ---------------------------------------------------------------//
    public void Death(bool isValid = true)
    {
        if (isValid)
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.currentCurrency += _reward;
            if (LevelEditor_Handler.Instance != null)
                LevelEditor_Handler.Instance._coinTest += _reward;
        }

        WaveManager.Instance.ReturnToPool(gameObject, data.name);
    }

    public void TakeDamage(float dmg)
    {
        _health -= dmg;
    }

    public void ApplyDebuff(int type, float duration, float value)
    {
        throw new System.NotImplementedException();
    }

    public void SetBaseStat(int type)
    {
        throw new System.NotImplementedException();
    }

    public float GetSpeed()
    {
        return _speed;
    }

    public void ApplyDebuff(int type, float duration, float value)
    { }

    public void SetBaseStat(int type)
    { }

    public List<eType> GetTargetType()
    {
        return data.typing ?? new List<eType> { eType.Normal };
    }

    //-------------------------------------------------------------- < PROCEESSORS > --------------------------------------------------------------//
    public void Spawn(BaseEnemySO type, string path)
    {
        data = type;

        _health = type.maxHealth;
        _reward = type.reward;
        _speed = type.maxSpeed;
        _acceleration = type.acceleration;
        _gravity = type.gravity * -1;
        _heightAboveGround = type.levitation;

        GetComponent<Renderer>().material.color = type.color;

        SetPath(path);
    }

    private void SetPath(string pathway)
    {
        // Path.json Processor
        if (!string.IsNullOrEmpty(pathway))
        {
            string _getFile = $"{_pathDirectory}/{pathway}.JSON";
            if (File.Exists(_getFile))
            {
                PathDataList _pathDataJSON = new PathDataList();
                _pathDataJSON.list = JsonConvert.DeserializeObject<List<PathData>>(File.ReadAllText(_getFile));

                foreach (PathData path in _pathDataJSON.list)
                    _dataList.Add(new dataStruct(path.name, path.data[0], path.data[1], path.data[2], path.data[3]));

                _heightAboveGround = data.levitation;
                gameObject.transform.position = new Vector3(
                    _dataList[0].Data[0] + Random.Range(-_dataList[0].Data[3], _dataList[0].Data[3]),
                    _dataList[0].Data[1] + _heightAboveGround,
                    _dataList[0].Data[2] + Random.Range(-_dataList[0].Data[3], _dataList[0].Data[3])
                );
                SetDestination(new Vector3(
                    _dataList[1].Data[0] + Random.Range(-_dataList[1].Data[3], _dataList[1].Data[3]),
                    _dataList[1].Data[1] + _heightAboveGround,
                    _dataList[1].Data[2] + Random.Range(-_dataList[1].Data[3], _dataList[1].Data[3])
                ));

                _ctrl = GetComponent<CharacterController>();
            }
            else
                Destroy(gameObject);
        }
        else
        {
            Debug.LogError($"No path found for enemy {this.gameObject}");
            Destroy(gameObject);
        }

        StartCoroutine("StartEnemyMovement");
    }

    private void SetDestination(Vector3 position)
    {
        _startPosition = transform.position;
        _targetPosition = position;
        _moveTime = 0f;
        _curSpeed = 0f;
    }

    void Update()
    {
        // [Health] //
        if (_health > 0 && data != null)
            _bar.setHealth(_health, data.maxHealth);
        else
            Death(true);
    }

    void FixedUpdate()
    {
        // [Movement]
        if (_isMovable)
        {
            MoveToward();

            if (!data.typing.Contains(eType.Airborne))
            {
                if (!_ctrl.isGrounded) // Gravity
                    _velocity.y += _gravity * Time.deltaTime;
                else
                    _velocity.y = -2f;
            }

            // Apply velocity
            _ctrl.Move(_velocity * Time.deltaTime);
        }
    }

    void MoveToward()
    {
        // Calculate and set X-axis and Z-axis movement
        Vector3 direction = (_targetPosition - transform.position).normalized;

        // Accelerate towards the target speed
        if (_moveTime < _acceleration)
        {
            _moveTime += Time.deltaTime;
            _curSpeed = Mathf.Lerp(0f, _speed, _moveTime * _acceleration);

            if (_curSpeed > _speed)
                _curSpeed = _speed;
        }

        Vector3 movement = direction * _curSpeed;
        _ctrl.Move(movement * Time.deltaTime);

        // Rotate towards the movement direction
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), _acceleration * 2.5f * Time.deltaTime);

        // Update height for enemies dynamically
        UpdateHeightAboveGround();

        // Check for destination reached
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(_targetPosition.x, 0, _targetPosition.z)) < 1f)
        {
            if (_dataIndex <= _dataList.Count - 1)
            {
                _dataIndex++;
                if (_dataIndex >= _dataList.Count)
                {
                    Death();
                }
                else
                {
                    float _randomRange = Random.Range(-_dataList[_dataIndex].Data[3], _dataList[_dataIndex].Data[3]);
                    SetDestination(new Vector3(
                        _dataList[_dataIndex].Data[0] + _randomRange,
                        _dataList[_dataIndex].Data[1] + _heightAboveGround,
                        _dataList[_dataIndex].Data[2] + _randomRange
                    ));
                }
            }
        }
    }

    private void UpdateHeightAboveGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, Mathf.Infinity))
        {
            float groundHeight = hit.point.y;
            transform.position = new Vector3(transform.position.x, groundHeight + _heightAboveGround, transform.position.z);
        }
    }

    IEnumerator StartEnemyMovement()
    {
        yield return new WaitForSeconds(0.1f);
        _isMovable = true;
    }
}
