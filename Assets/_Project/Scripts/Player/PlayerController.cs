using System.Collections;
using UnityEngine;

namespace PisoEsLava
{
    // Movimiento del chico (TDD 5.2.6 b): coyote time, jump buffer, salto variable y caida mas rapida.
    // Maquina de estados sencilla: en el suelo / en el aire.
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        public float moveSpeed = 7f;
        public float groundAcceleration = 80f;
        public float airAcceleration = 50f;

        [Header("Salto")]
        [Tooltip("Altura del salto con el boton mantenido (unidades)")]
        public float jumpHeight = 3.5f;
        [Tooltip("Altura del salto si se suelta el boton enseguida (unidades)")]
        public float minJumpHeight = 1.2f;
        [Tooltip("Gravedad del personaje (u/s2)")]
        public float gravity = 30f;
        public float fallGravityMultiplier = 1.6f;
        public float maxFallSpeed = 20f;
        public float coyoteTime = 0.10f;
        public float jumpBuffer = 0.12f;

        [Header("Suelo")]
        public float groundCheckDistance = 0.06f;
        [Tooltip("Tiempo que se ignoran las plataformas de un sentido al bajar con S + Espacio")]
        public float dropThroughTime = 0.3f;

        public bool IsGrounded { get; private set; }
        public bool ControlEnabled { get; private set; } = true;
        public float JumpVelocity => CalculateJumpVelocity(gravity, jumpHeight);
        public float MinJumpVelocity => CalculateJumpVelocity(gravity, minJumpHeight);

        static readonly int SpeedParam = Animator.StringToHash("Velocidad");
        static readonly int GroundedParam = Animator.StringToHash("EnSuelo");
        static readonly int VerticalSpeedParam = Animator.StringToHash("VelocidadY");

        Rigidbody2D rb;
        CapsuleCollider2D body;
        SpriteRenderer sprite;
        Animator anim;
        float coyoteTimer;
        float bufferTimer;
        float moveInput;
        bool jumpHeld;
        bool rising;
        bool droppingThrough;
        int groundMask;
        int oneWayMask;
        int oneWayLayer;

        // v = raiz(2 * g * h): el disenador solo indica la altura del salto
        public static float CalculateJumpVelocity(float g, float h)
        {
            return Mathf.Sqrt(2f * g * h);
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<CapsuleCollider2D>();
            sprite = GetComponentInChildren<SpriteRenderer>();
            anim = GetComponentInChildren<Animator>();
            groundMask = LayerMask.GetMask("Ground");
            oneWayMask = LayerMask.GetMask("OneWay");
            oneWayLayer = LayerMask.NameToLayer("OneWay");
        }

        // La meta es el objeto con la etiqueta Finish
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Finish") && GameManager.Instance != null)
            {
                GameManager.Instance.PlayerReachedGoal();
            }
        }

        void Update()
        {
            if (!ControlEnabled)
            {
                UpdateAnimator();
                return;
            }
            // En pausa no se lee la entrada (si no, el Espacio de los botones haria saltar al volver)
            if (Time.timeScale == 0f)
            {
                return;
            }
            // Hasta que termina la cuenta atras no se mueve ni salta (la gravedad si actua)
            if (GameManager.Instance != null && GameManager.Instance.LocksInput)
            {
                moveInput = 0f;
                bufferTimer = 0f;
                IsGrounded = CheckGround(out _);
                ReadInput(out _, out _, out _);
                UpdateAnimator();
                return;
            }

            ReadInput(out Vector2 move, out bool pressed, out bool released);
            moveInput = Mathf.Abs(move.x) > 0.2f ? Mathf.Clamp(move.x, -1f, 1f) : 0f;
            float dt = Time.deltaTime;

            IsGrounded = CheckGround(out Collider2D groundCollider);
            if (IsGrounded)
            {
                rising = false;
            }
            coyoteTimer = IsGrounded ? coyoteTime : coyoteTimer - dt;
            bufferTimer = pressed ? jumpBuffer : bufferTimer - dt;

            bool standingOnOneWay = IsGrounded && groundCollider != null && groundCollider.gameObject.layer == oneWayLayer;
            if (pressed && move.y < -0.5f && standingOnOneWay)
            {
                // S + Espacio: bajar de la plataforma de un sentido
                bufferTimer = 0f;
                StartCoroutine(DropThrough(groundCollider));
            }
            else if (bufferTimer > 0f && coyoteTimer > 0f)
            {
                Jump();
            }

            // Salto variable: al soltar el boton se corta la subida (salto minimo de 1,2 u)
            if ((released || !jumpHeld) && rising && rb.linearVelocity.y > MinJumpVelocity)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, MinJumpVelocity);
                rising = false;
            }

            if (moveInput != 0f && sprite != null)
            {
                sprite.flipX = moveInput < 0f;
            }

            UpdateAnimator();
        }

        void FixedUpdate()
        {
            if (!ControlEnabled)
            {
                return;
            }

            Vector2 v = rb.linearVelocity;
            // Caida mas rapida: al bajar la gravedad se multiplica por 1,6
            float baseScale = gravity / Mathf.Abs(Physics2D.gravity.y);
            rb.gravityScale = v.y < 0f ? baseScale * fallGravityMultiplier : baseScale;

            float accel = IsGrounded ? groundAcceleration : airAcceleration;
            v.x = Mathf.MoveTowards(v.x, moveInput * moveSpeed, accel * Time.fixedDeltaTime);
            v.y = Mathf.Max(v.y, -maxFallSpeed);
            rb.linearVelocity = v;
        }

        void ReadInput(out Vector2 move, out bool pressed, out bool released)
        {
            move = GameInput.Move;
            pressed = GameInput.JumpPressed;
            released = GameInput.JumpReleased;
            jumpHeld = GameInput.JumpHeld;
        }

        void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpVelocity);
            bufferTimer = 0f;
            coyoteTimer = 0f;
            IsGrounded = false;
            rising = true;
        }

        // ---------- Suelo y plataformas de un sentido ----------

        // BoxCast corto bajo los pies (TDD 5.2.7)
        bool CheckGround(out Collider2D hitCollider)
        {
            hitCollider = null;
            Bounds b = body.bounds;
            int mask = groundMask | (droppingThrough ? 0 : oneWayMask);
            Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
            Vector2 size = new Vector2(b.size.x * 0.9f, 0.05f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance + 0.05f, mask);
            if (hit.collider == null || hit.normal.y < 0.5f)
            {
                return false;
            }
            // Si sube (saltando, o atravesando una plataforma) no esta apoyado
            if (rb.linearVelocity.y > 0.05f)
            {
                return false;
            }

            hitCollider = hit.collider;
            return true;
        }

        IEnumerator DropThrough(Collider2D platform)
        {
            droppingThrough = true;
            Physics2D.IgnoreCollision(body, platform, true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
            yield return new WaitForSeconds(dropThroughTime);
            Physics2D.IgnoreCollision(body, platform, false);
            droppingThrough = false;
        }

        void UpdateAnimator()
        {
            if (anim == null || anim.runtimeAnimatorController == null)
            {
                return;
            }

            anim.SetFloat(SpeedParam, Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool(GroundedParam, IsGrounded);
            anim.SetFloat(VerticalSpeedParam, rb.linearVelocity.y);
        }

        // ---------- Perder y ganar ----------

        // Color del jugador al perder (el unico efecto)
        public static readonly Color HurtColor = new Color(1f, 0.45f, 0.35f);

        // Lo llama GameManager al perder: se queda quieto donde esta y se pone rojizo
        public void Die()
        {
            StopAllCoroutines();
            droppingThrough = false;
            ControlEnabled = false;
            moveInput = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            body.enabled = false;
            if (sprite != null)
            {
                sprite.color = HurtColor;
            }
        }

        // Lo llama GameManager al llegar a la meta
        public void Freeze()
        {
            ControlEnabled = false;
            moveInput = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
