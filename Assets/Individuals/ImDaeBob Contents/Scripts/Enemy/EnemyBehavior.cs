using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using System.Linq;

public class EnemyBehavior : MonoBehaviour, I_GetType, I_Damagable
{
    public BaseEnemySO data;

    // [ ENEMY'S UI ] //
    private Enemy_HPBar _bar;

    // [ ENEMY'S DATA ] //
    private float _health, _speed, _acceleration, _gravity, _heightAboveGround;

    // [ Rigidbody & CharacterController ] //
    private Rigidbody _rb; //For external forces like knockbacks and explosion forces
    private CharacterController _ctrl; //For moving basically

    private int _dataIndex = 1;
    private List<dataStruct> _dataList = new List<dataStruct>();

    private bool _isMovable = false;
    private Vector3 _startPosition, _targetPosition, _velocity;
    private float _moveTime = 0f, _curSpeed = 0f;

    void Start()
    {
        _bar = GetComponentInChildren<Enemy_HPBar>();

        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    public void SetStats()
    {
        _health = data.maxHealth;
        _speed = data.maxSpeed;
        _acceleration = data.acceleration;
        _gravity = data.gravity * -1;
        _heightAboveGround = data.levitation;

        GetComponent<Renderer>().material.color = data.color;
    }

    public void SetPath(string pathway)
    {
        // Path.json Processor
        if (!string.IsNullOrEmpty(pathway))
        {
            string _jsonPath = $"{Application.dataPath}/Data/Enemies/Paths/{pathway}";
            string _getFile = $"{_jsonPath}.json";

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
            Destroy(gameObject);

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
            Death();

        // [Movement]
        if (_isMovable)
        {
            MoveToward();

            if (!data.type.Contains(eType.Airborne))
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
                    Destroy(gameObject);
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

    public void Death()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float dmg)
    {

    }

    public void ApplyDebuff(float smth)
    {

    }

    public TargetTypeEnum[] GetTargetType()
    {
        return data.targets;
    }

    IEnumerator StartEnemyMovement()
    {
        yield return new WaitForSeconds(0.1f);
        _isMovable = true;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Classes & Structs
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

        public PathData(string _name = "Point ?", Vector3 _position = new Vector3(), float _scaleMultiplier = 1f)
        {
            name = _name;
            data = new float[] { _position.x, _position.y, _position.z, _scaleMultiplier };
        }
    }

    public class dataStruct
    {
        public string Name { get; set; }
        public float[] Data { get; set; }

        public dataStruct(string _name, float _x, float _y, float _z, float _scaleMultiplier)
        {
            Name = _name;
            Data = new float[] { _x, _y, _z, _scaleMultiplier };
        }
    }
}
