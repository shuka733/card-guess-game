using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float InteractRadius = 1.2f;

    private Rigidbody2D rb;
    private VirtualJoystick joystick;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        joystick = FindObjectOfType<VirtualJoystick>();
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying()) return;

        // Touch: tap right half of screen to interact
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width * 0.5f)
                    TryInteract();
            }
        }

        // Editor keyboard fallback
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            TryInteract();
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying()) { rb.velocity = Vector2.zero; return; }

        Vector2 input = Vector2.zero;
        if (joystick != null)
            input = joystick.Direction;

        // Keyboard fallback
        if (input == Vector2.zero)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }

        rb.velocity = input.normalized * MoveSpeed;
    }

    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, InteractRadius);
        foreach (var hit in hits)
        {
            var keyCard = hit.GetComponent<KeyCardItem>();
            if (keyCard != null && !keyCard.IsCollected)
            {
                keyCard.Collect();
                return;
            }

            var exit = hit.GetComponent<ExitDoor>();
            if (exit != null)
            {
                exit.TryOpen();
                return;
            }
        }
    }
}
