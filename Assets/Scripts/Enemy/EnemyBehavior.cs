using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    [HideInInspector] public BaseEnemySO _data;

    // [ ENEMY'S UI ] //
    private Enemy_HPBar bar;

    // [ ENEMY'S DATA] //
    private float health, speed, acceleration, gravity;

    // [ Rigidbody & CharacterController ] //
    private Rigidbody rb; //For external forces like knockbacks and explosion forces
    private CharacterController ctrl; //For moving basically

    private bool isMoving = false;
    private Vector3 startPosition, targetPosition;

    private Vector3 velocity;
    private float moveTime = 0f, curSpeed = 0f;
    [SerializeField] private float heightAboveGround = 1f; //Avoid clipping through the floor

    void Start()
    {
        health = _data.maxHealth;
        speed = _data.maxSpeed;
        acceleration = _data.acceleration;
        gravity = _data.gravity * -1;

        bar = GetComponentInChildren<Enemy_HPBar>();

        GetComponent<Renderer>().material.color = _data.color;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        ctrl = GetComponent<CharacterController>();
    }

    public void SetDestination(Vector3 position)
    {
        startPosition = transform.position;
        targetPosition = position;
        moveTime = 0f;
        curSpeed = 0f;

        isMoving = true;
    }

    void Update()
    {
        // [Health] //
        if (health > 0)
            bar.setHealth(health, _data.maxHealth);
        else
            Death();
        ////////////////////////////////////////////////////////////
        // [Movement] //
        if (isMoving)
            MoveToward();
        
        if (!ctrl.isGrounded) //Gravity
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y = -2f;
        
        // ==>
        ctrl.Move(velocity * Time.deltaTime);
    }

    void MoveToward()
    {
        // Calculate and setting Xasix and Zasix 
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (moveTime < acceleration)
        {
            moveTime += Time.deltaTime;
            curSpeed = Mathf.Lerp(0f, speed, moveTime * acceleration);

            if (curSpeed > speed)
                curSpeed = speed;
        }
        Vector3 movement = direction * curSpeed;
        ctrl.Move(movement * Time.deltaTime);

        // Setting only Yasix
        if (ctrl.isGrounded)
        {
            Vector3 posY = new Vector3(transform.position.x, heightAboveGround, transform.position.z);
            transform.position = posY;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 1f)
            isMoving = false;
    }

    public void Death()
    {
        Destroy(gameObject);
    }
}
