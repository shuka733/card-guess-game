using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase }

    public float PatrolSpeed = 1.2f;
    public float ChaseSpeed = 2.8f;
    public float DetectionRadius = 4f;
    public float CatchRadius = 0.55f;
    public float AlertDuration = 4f;

    public List<Transform> PatrolPoints = new List<Transform>();

    private State currentState = State.Patrol;
    private Rigidbody2D rb;
    private Transform player;
    private int patrolIndex;
    private float alertTimer;
    private Vector2 lastKnownPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying()) { rb.linearVelocity = Vector2.zero; return; }
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= CatchRadius)
        {
            GameManager.Instance.GameOver();
            return;
        }

        switch (currentState)
        {
            case State.Patrol:
                if (dist <= DetectionRadius)
                {
                    currentState = State.Chase;
                    UIManager.Instance.ShowWarning(true);
                }
                break;

            case State.Alert:
                alertTimer -= Time.deltaTime;
                if (dist <= DetectionRadius)
                {
                    currentState = State.Chase;
                    UIManager.Instance.ShowWarning(true);
                }
                else if (alertTimer <= 0f)
                {
                    currentState = State.Patrol;
                    UIManager.Instance.ShowWarning(false);
                }
                break;

            case State.Chase:
                if (dist > DetectionRadius * 1.4f)
                {
                    lastKnownPos = player.position;
                    currentState = State.Alert;
                    alertTimer = AlertDuration;
                    UIManager.Instance.ShowWarning(false);
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying()) return;
        if (player == null) return;

        switch (currentState)
        {
            case State.Patrol:  DoPatrol(); break;
            case State.Alert:   MoveTo(lastKnownPos, PatrolSpeed); break;
            case State.Chase:   MoveTo(player.position, ChaseSpeed); break;
        }
    }

    void DoPatrol()
    {
        if (PatrolPoints.Count == 0) { rb.linearVelocity = Vector2.zero; return; }

        Vector2 target = PatrolPoints[patrolIndex].position;
        if (Vector2.Distance(transform.position, target) < 0.25f)
            patrolIndex = (patrolIndex + 1) % PatrolPoints.Count;
        else
            MoveTo(target, PatrolSpeed);
    }

    void MoveTo(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    public State CurrentState => currentState;
}
