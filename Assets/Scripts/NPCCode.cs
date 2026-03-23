using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownNPC : MonoBehaviour
{
    public enum State { Walking, Talking, Waiting }
    public State currentState = State.Walking;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 3f;
    private Vector2 currentDirection;
    private Rigidbody2D rb;

    [Header("Click Movement")]
    public Vector2 clickMoveTarget;
    private bool movingToTarget = false;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public bool isFacingRight = true;

    [Header("Camera Bounds")]
    public Camera mainCamera;
    public float boundsPadding = 0.5f;

    [Header("Talking Mechanic")]
    [Range(0f, 1f)] public float talkProbability = 0.5f;
    public float talkDuration = 3f;

    [Header("UI")]
    public GameObject yourInterface;

    // ── Juice settings ──────────────────────────────────────────────────
    [Header("Juice – Cartoon Tilt")]
    public float tiltAngle = 15f;   // degrees to snap left/right
    public float stepInterval = 0.18f; // seconds per "step" (how fast it alternates)

    [Header("Juice – Flip Turn")]
    public float flipSquishTime = 0.10f; // seconds to squish before flipping
    public float flipBounceTime = 0.12f; // seconds to spring back after flipping
    // ────────────────────────────────────────────────────────────────────

    private bool onCooldown = false;
    private float cooldownDuration = 2f;
    private bool isClickMoving = false;

    // Juice state
    private float stepTimer = 0f;
    private int stepFrame = 0;      // alternates 0 / 1
    private bool isFlipping = false;
    private Vector3 baseScale;
    // ═══════════════════════════════════════════════════════════════
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (mainCamera == null) mainCamera = Camera.main;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        baseScale = transform.localScale;

        PickRandomDirection();
        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        if (currentState == State.Walking && !movingToTarget)
        {
            UpdateFacingDirection();
            CheckBounds();
        }

        ApplyJuice();
    }

    // ── Juice update ────────────────────────────────────────────────
    void ApplyJuice()
    {
        bool isMoving = (currentState == State.Walking || movingToTarget) && !isFlipping;

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer -= stepInterval;
                stepFrame = 1 - stepFrame; // flip between 0 and 1
            }

            // Hard snap: no lerp, pure cartoon alternation
            float tilt = (stepFrame == 0) ? tiltAngle : -tiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);
        }
        else if (!isFlipping)
        {
            // Snap back to upright instantly when stopping
            stepTimer = 0f;
            stepFrame = 0;
            transform.rotation = Quaternion.identity;
        }
    }

    // ── Organic flip coroutine ──────────────────────────────────────
    IEnumerator FlipTurn(bool newFacingRight)
    {
        isFlipping = true;

        // Phase 1 – squish narrow (turning-into-camera feel)
        float t = 0f;
        while (t < flipSquishTime)
        {
            t += Time.deltaTime;
            float p = t / flipSquishTime;
            float squishX = Mathf.Lerp(1f, 0.15f, p);   // squeeze to sliver
            float squishY = Mathf.Lerp(1f, 1.2f, p);   // compensating stretch
            transform.localScale = new Vector3(
                baseScale.x * squishX * (isFacingRight ? 1f : -1f),
                baseScale.y * squishY,
                baseScale.z
            );
            yield return null;
        }

        // Commit the flip at the narrowest moment
        isFacingRight = newFacingRight;

        // Phase 2 – spring back with overshoot
        t = 0f;
        while (t < flipBounceTime)
        {
            t += Time.deltaTime;
            float p = t / flipBounceTime;
            // Elastic ease-out: overshoot then settle
            float elastic = 1f + Mathf.Sin(p * Mathf.PI) * 0.25f;
            transform.localScale = new Vector3(
                baseScale.x * Mathf.Lerp(0.15f, 1f, p) * elastic * (isFacingRight ? 1f : -1f),
                baseScale.y * Mathf.Lerp(1.2f, 1f, p),
                baseScale.z
            );
            yield return null;
        }

        // Settle to exact base scale
        transform.localScale = new Vector3(
            baseScale.x * (isFacingRight ? 1f : -1f),
            baseScale.y,
            baseScale.z
        );

        isFlipping = false;
    }

    // ── Direction helpers ───────────────────────────────────────────
    void SetFacing(bool faceRight)
    {
        if (faceRight == isFacingRight || isFlipping) return;
        StartCoroutine(FlipTurn(faceRight));
    }

    void UpdateFacingDirection()
    {
        if (currentDirection.x > 0.05f) SetFacing(true);
        else if (currentDirection.x < -0.05f) SetFacing(false);
    }

    // ═══════════════════════════════════════════════════════════════
    void FixedUpdate()
    {
        if (movingToTarget)
        {
            Vector2 direction = (clickMoveTarget - rb.position).normalized;

            if (direction.x > 0.05f) SetFacing(true);
            else if (direction.x < -0.05f) SetFacing(false);

            Vector2 newPos = Vector2.MoveTowards(rb.position, clickMoveTarget, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, clickMoveTarget) < 0.05f)
            {
                movingToTarget = false;
                isClickMoving = false;
            }
        }
        else if (currentState == State.Walking)
        {
            rb.linearVelocity = currentDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeDirectionInterval);
            if (currentState == State.Walking && !movingToTarget)
                PickRandomDirection();
        }
    }

    void PickRandomDirection()
    {
        currentDirection = Random.insideUnitCircle.normalized;
    }

    public void ForcePickNewDirection()
    {
        PickRandomDirection();
    }

    void CheckBounds()
    {
        if (WalkAreaBounds.Instance == null) return;
        if (currentState != State.Walking || movingToTarget) return;

        Vector2 pos = transform.position;

        if (!WalkAreaBounds.Instance.IsInsideBounds(pos))
        {
            currentDirection = ((Vector2)WalkAreaBounds.Instance.GetCenter() - pos).normalized;
            UpdateFacingDirection();
        }
    }
    void OnMouseDown()
    {
        if (currentState == State.Waiting) return;
        if (PatientUIManager.Instance != null && PatientUIManager.Instance.yourInterface.activeSelf) return;
        movingToTarget = true;
        isClickMoving = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            BounceOffObject(collision.gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Counter") && !isClickMoving)
            BounceOffObject(collision.gameObject);
    }

    void BounceOffObject(GameObject obstacle)
    {
        Vector2 awayDirection = ((Vector2)transform.position - (Vector2)obstacle.transform.position).normalized;
        awayDirection += Random.insideUnitCircle * 0.4f;
        currentDirection = awayDirection.normalized;
        movingToTarget = false;
        isClickMoving = false;
        UpdateFacingDirection();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ---- COUNTER ----
        if (collision.gameObject.CompareTag("Counter"))
        {
            if (isClickMoving)
            {
                movingToTarget = false;
                isClickMoving = false;
                rb.linearVelocity = Vector2.zero;
                currentState = State.Waiting;

                if (yourInterface != null)
                    yourInterface.SetActive(true);

                GetComponent<PatientController>()?.OnReachedCounter();
            }
            return;
        }

        // ---- OBSTACLE ----
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (!isClickMoving)
                BounceOffObject(collision.gameObject);
            return;
        }

        // ---- NPC TALK ----
        if (currentState != State.Walking || onCooldown) return;

        TopDownNPC otherNPC = collision.GetComponent<TopDownNPC>();
        if (otherNPC != null && otherNPC.currentState == State.Walking && !otherNPC.onCooldown)
        {
            if (AreFacingEachOther(otherNPC))
            {
                if (Random.value <= talkProbability)
                    StartCoroutine(TalkRoutine(otherNPC));
                else
                {
                    StartCoroutine(InteractionCooldown());
                    otherNPC.StartCoroutine(otherNPC.InteractionCooldown());
                }
            }
        }
    }

    bool AreFacingEachOther(TopDownNPC otherNPC)
    {
        float myX = transform.position.x;
        float theirX = otherNPC.transform.position.x;
        bool amILookingAtThem = isFacingRight ? (theirX > myX) : (theirX < myX);
        bool areTheyLookingAtMe = otherNPC.isFacingRight ? (myX > theirX) : (myX < theirX);
        return amILookingAtThem && areTheyLookingAtMe;
    }

    public IEnumerator TalkRoutine(TopDownNPC partner)
    {
        currentState = State.Talking;
        partner.currentState = State.Talking;
        rb.linearVelocity = Vector2.zero;
        partner.rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(talkDuration);

        currentState = State.Walking;
        partner.currentState = State.Walking;
        PickRandomDirection();
        partner.PickRandomDirection();
        StartCoroutine(InteractionCooldown());
        partner.StartCoroutine(partner.InteractionCooldown());
    }

    public IEnumerator InteractionCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        onCooldown = false;
    }
}