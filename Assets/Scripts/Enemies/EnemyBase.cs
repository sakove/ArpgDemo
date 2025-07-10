using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float aggroRange = 5f;
    
    [Header("References")]
    [SerializeField] protected Transform target;
    [SerializeField] protected LayerMask playerLayer;
    
    // Components
    protected Rigidbody2D rb;
    protected Animator animator;
    
    // State
    protected int currentHealth;
    protected bool isAttacking;
    protected bool isFacingRight = true;
    protected float lastAttackTime;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }
    
    protected virtual void Start()
    {
        // Find player if target not set
        if (target == null)
        {
            PlayerManager player = FindObjectOfType<PlayerManager>();
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    protected virtual void Update()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        // Check if player is in aggro range
        if (distanceToTarget <= aggroRange)
        {
            // Check if player is in attack range
            if (distanceToTarget <= attackRange)
            {
                // Attack if cooldown is over
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // Move towards player
                MoveTowardsTarget();
            }
        }
        else
        {
            // Idle behavior
            Idle();
        }
        
        // Update animations
        UpdateAnimations();
    }
    
    protected virtual void MoveTowardsTarget()
    {
        if (target == null) return;
        
        // Get direction to target
        Vector2 direction = (target.position - transform.position).normalized;
        
        // Move in that direction
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        
        // Flip sprite based on movement direction
        if (direction.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && isFacingRight)
        {
            Flip();
        }
    }
    
    protected virtual void Attack()
    {
        // Set attacking state
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Play attack animation
        animator?.SetTrigger("Attack");
        
        // Detect player in range
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        
        // Apply damage to player
        if (hitPlayer != null)
        {
            // 尝试获取PlayerHealth组件
            PlayerHealth playerHealth = hitPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                // 如果没有PlayerHealth组件，尝试获取PlayerManager组件
                PlayerManager playerManager = hitPlayer.GetComponent<PlayerManager>();
                if (playerManager != null)
                {
                    playerManager.TakeDamage(damage);
                }
            }
        }
        
        // Reset attacking state after animation
        Invoke(nameof(ResetAttackState), 0.5f);
    }
    
    protected virtual void Idle()
    {
        // Default idle behavior - stop moving
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    protected virtual void ResetAttackState()
    {
        isAttacking = false;
    }
    
    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
    protected virtual void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsAttacking", isAttacking);
        }
    }
    
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Play hit animation
        animator?.SetTrigger("Hit");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        // Play death animation
        animator?.SetTrigger("Death");
        
        // Disable components
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        this.enabled = false;
        
        // Destroy after animation
        Destroy(gameObject, 2f);
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw aggro range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
} 