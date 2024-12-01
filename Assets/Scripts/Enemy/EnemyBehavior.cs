using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    [HideInInspector] public BaseEnemySO data;

    // [ ENEMY'S UI ] //
    private Enemy_HPBar _bar;

    // [ ENEMY'S DATA] //
    private float _health, _speed, _acceleration, _gravity;

    // [ Rigidbody & CharacterController ] //
    private Rigidbody _rb; //For external forces like knockbacks and explosion forces
    private CharacterController _ctrl; //For moving basically

    private bool _isMoving = false;
    private Vector3 _startPosition, _targetPosition, _velocity;
    private float _moveTime = 0f, _curSpeed = 0f;
    [SerializeField] float _heightAboveGround = 0.5f; //Avoid clipping through the floor

    void Start()
    {
        _health = data.maxHealth;
        _speed = data.maxSpeed;
        _acceleration = data.acceleration;
        _gravity = data.gravity * -1;

        _bar = GetComponentInChildren<Enemy_HPBar>();

        GetComponent<Renderer>().material.color = data.color;

        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _ctrl = GetComponent<CharacterController>();
    }

    public void SetDestination(Vector3 position)
    {
        _startPosition = transform.position;
        _targetPosition = position;
        _moveTime = 0f;
        _curSpeed = 0f;

        _isMoving = true;
    }

    void Update()
    {
        // [Health] //
        if (_health > 0)
            _bar.setHealth(_health, data.maxHealth);
        else
            Death();
        ////////////////////////////////////////////////////////////
        // [Movement] //
        if (_isMoving)
            MoveToward();
        
        if (!_ctrl.isGrounded) //Gravity
            _velocity.y += _gravity * Time.deltaTime;
        else
            _velocity.y = -2f;
        
        // ==>
        _ctrl.Move(_velocity * Time.deltaTime);
    }

    void MoveToward()
    {
        // Calculate and setting Xasix and Zasix 
        Vector3 direction = (_targetPosition - transform.position).normalized;
        if (_moveTime < _acceleration)
        {
            _moveTime += Time.deltaTime;
            _curSpeed = Mathf.Lerp(0f, _speed, _moveTime * _acceleration);

            if (_curSpeed > _speed)
                _curSpeed = _speed;
        }
        Vector3 movement = direction * _curSpeed;
        _ctrl.Move(movement * Time.deltaTime);

        // Rotating toward the direction it's going
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), _acceleration*2.5f * Time.deltaTime);

        // Setting only Yasix
        if (_ctrl.isGrounded)
        {
            Vector3 posY = new Vector3(transform.position.x, _heightAboveGround, transform.position.z);
            transform.position = posY;
        }

        if (Vector3.Distance(transform.position, _targetPosition) < 1f)
            _isMoving = false;
    }

    public void Death()
    {
        Destroy(gameObject);
    }
}
