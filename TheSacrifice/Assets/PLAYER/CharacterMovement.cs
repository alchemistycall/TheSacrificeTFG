using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    private float moveSpeed;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ataque")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;
    private float lastAttackTime;
    private bool canAttack = true;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        moveSpeed = walkSpeed;
    }

    void Update()
    {
        HandleMovement();
        HandleRun();
        HandleJump();
        HandleAttack();
    }

    void FixedUpdate()
    {
        UpdateJumpState();
    }

    void HandleMovement()
    {
        if (!canAttack) return; // Bloquear movimiento durante el ataque

        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        if (moveInput > 0 && !isFacingRight || moveInput < 0 && isFacingRight)
            Flip();
    }

    void HandleRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = runSpeed;
            animator.SetBool("IsRunning", true);
        }
        else
        {
            moveSpeed = walkSpeed;
            animator.SetBool("IsRunning", false);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetBool("IsJumping", true);
        }
    }

    void HandleAttack()
    {
        if (Input.GetButtonDown("Fire1") && canAttack && Time.time > lastAttackTime + attackCooldown)
        {
            canAttack = false;
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            // Detectar enemigos
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("Golpeado: " + enemy.name);
                // enemy.GetComponent<Enemy>().TakeDamage(10); // Descomenta si tienes un sistema de daño
            }
        }
    }

    // Llamar este método desde un evento en la animación de ataque
    public void EndAttack()
    {
        canAttack = true;
    }

    void UpdateJumpState() => animator.SetBool("IsJumping", !IsGrounded());

    bool IsGrounded() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Dibujar rango de ataque en el Editor (opcional)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}