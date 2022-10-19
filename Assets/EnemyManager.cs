using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    SpriteRenderer sprite;
    Rigidbody2D rig;
    Animator anim;
    [SerializeField] int health = 20;
    public bool isStunned;
    public bool isAttacking;
    [SerializeField] float stunTimer;
    [SerializeField] float moveSpeed;
    float velocityX;
    public float accTime = 0.11f;



    [Header("WayPoints")]
    [SerializeField] Transform leftPos;
    [SerializeField] Transform rightPos;

    [Header("Projectile")]
    [SerializeField] GameObject projectileObject;
    [SerializeField] Transform shootingPos;

    [Header("BearRelated")]
    [SerializeField] bool isBear;
    public bool isBearDashing;

    [Header("PlayerDetect")]
    public bool playerOnLeft;
    public bool playerOnRight; 
    public Vector2 leftPointOffset;
    public Vector2 rightPointOffset;
    public Vector2 detectRange;
    [SerializeField] LayerMask playerLayer;

    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        EnemyStatus();
        EnemyAttackingAction();
        EnemyMovement();
        BearDashMovement();
    }
    void EnemyStatus()
    {
        playerOnLeft = LeftPlayerCheck();
        playerOnRight = RightPlayerCheck();
        anim.SetBool("isAttacking", isAttacking);
        if (health <= 0)
        {
            rig.drag = 0.75f;
            EnemyDeath();
        }
        else
        {
            if (isStunned)
            {
                stunTimer += Time.deltaTime;
                rig.drag = 2.5f;
                if (stunTimer >= 10f)
                {
                    stunTimer = 0;
                    anim.SetBool("isStun", false);
                    isBearDashing = false;
                }
            }
            else if (isAttacking) 
            {
                if(!isBear) rig.drag = 2.5f;
            }
            else
            {
                rig.drag = 0.75f;
                stunTimer = 0;
            }
        }
    }
    void EnemyDeath() 
    {
        if (!this.GetComponent<Collider2D>().isTrigger) 
        {
            isStunned = false;
            isAttacking = false;
            isBearDashing = false;
            rig.drag = 0.75f;
            anim.SetBool("isDead", true);
            rig.AddForce(new Vector2(0f, 1f).normalized * (50f), ForceMode2D.Impulse);
            this.GetComponent<Collider2D>().isTrigger = true;
            Destroy(gameObject, 2f);
        }
    }
    void EnemyMovement() 
    {
        if (isStunned || isAttacking || isBearDashing || health<=0) return;

        if (!sprite.flipX) //³¯ÓÒ
        {
            rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, moveSpeed * Time.fixedDeltaTime * 60, ref velocityX, accTime), rig.velocity.y);
            if (transform.position.x >= rightPos.position.x) 
            {
                rig.velocity = Vector2.zero;
                anim.SetBool("isIdle", true);
            }
        }
        else 
        {
            rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, -moveSpeed * Time.fixedDeltaTime * 60, ref velocityX, accTime), rig.velocity.y);
            if (transform.position.x <= leftPos.position.x)
            {
                rig.velocity = Vector2.zero;
                anim.SetBool("isIdle", true);
            }
        }
    }
    void BearDashMovement() 
    {
        if (!isBearDashing) return;
        if (!sprite.flipX) //³¯ÓÒ
        {
            rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, moveSpeed * Time.fixedDeltaTime * 60 * 3.5f, ref velocityX, accTime), rig.velocity.y);
            if (transform.position.x >= rightPos.position.x)
            {
                rig.velocity = Vector2.zero;
                anim.SetBool("isStun", true);
                isBearDashing = false;
            }
        }
        else
        {
            rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, -moveSpeed * Time.fixedDeltaTime * 60 * 3.5f, ref velocityX, accTime), rig.velocity.y);
            if (transform.position.x <= leftPos.position.x)
            {
                rig.velocity = Vector2.zero;
                anim.SetBool("isStun", true);
                isBearDashing = false;
            }
        }
    }
    void EnemyAttackingAction() 
    {
        if (isBearDashing || isStunned) return;

        if (!sprite.flipX && playerOnRight)
        {
            isAttacking = true;
        }
        else if (sprite.flipX && playerOnLeft)
        {
            isAttacking = true;
        }
    }
    public void DamageOnEnemy(int damage) 
    {
        health -= damage;
    }
    public void StunOnEnemy() 
    {
        anim.SetBool("isStun", true);
        isStunned = true;
        stunTimer = 0;
    }
    bool LeftPlayerCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + leftPointOffset, detectRange, 0, playerLayer);
        if (coll != null)
            return true;
        else
            return false;
    }
    bool RightPlayerCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + rightPointOffset, detectRange, 0, playerLayer);
        if (coll != null)
            return true;
        else
            return false;
    }
    public void EndIdle_AnimationEvent() 
    {
        anim.SetBool("isIdle", false);
        if (sprite.flipX)
            sprite.flipX = false;
        else
            sprite.flipX = true;
    }
    public void StunFinish_AnimationEvent() 
    {
        isStunned = false;
    }
    public void PlayerCheck_AnimationEvent() 
    {
        if (!sprite.flipX && playerOnRight)
        {
            isAttacking = true;
        }
        else if (sprite.flipX && playerOnLeft)
        {
            isAttacking = true;
        }
        else 
        {
            isAttacking = false;
        }
    }
    public void ProjectileShooting_AnimationEvent() 
    {
        GameObject projectile = Instantiate(projectileObject, shootingPos.transform);
        projectile.SetActive(true);
        projectile.transform.parent = null;
        projectile.transform.position = shootingPos.transform.position;
        if (!sprite.flipX) 
        {
            projectile.GetComponent<Rigidbody2D>().AddForce(new Vector2(1, 0) * 7f, ForceMode2D.Impulse);
            projectile.GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            projectile.GetComponent<Rigidbody2D>().AddForce(new Vector2(1, 0) * -7f, ForceMode2D.Impulse);
            projectile.GetComponent<SpriteRenderer>().flipX = false;
        }
        Destroy(projectile, 3f);
    }
    public void BearDashStart_AnimationEvent() 
    {
        isBearDashing = true;
        isAttacking = false;
    }
    public void BearDashEnd_AnimationEvent() 
    {
        isAttacking = false;
        isBearDashing = false;
        anim.SetBool("isStun", true);
        isStunned = true;
        stunTimer = 0;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + leftPointOffset, detectRange);
        Gizmos.DrawWireCube((Vector2)transform.position + rightPointOffset, detectRange);
    }
}
