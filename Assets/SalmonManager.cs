using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SalmonManager : MonoBehaviour
{
    [Header("Status")]
    public bool haveEgg;
    bool onGround;
    bool groundJumpReset;
    bool inWater;
    bool onWall;
    bool onLeftWall;
    bool onRightWall;
    float velocityX;
    bool isJumping;
    public float throwingTimer;
    public bool isDamaged;
    float tempDamageTimer;
    [SerializeField] int curHealth;
    [SerializeField] int maxHealth = 100;

    [Header("Egg")]
    [SerializeField] float throwingForce;
    [SerializeField] int maxContainerHealth = 10;
    public int containerHealth;
    public float babyEnergy = 100f;

    [Header("MoveAttributes")]
    public float moveSp;
    public float accTime;
    public float decTime;
    public Vector2 curVelocity;
    public Vector2 inputOffset;
    bool canMove = true;

    [Header("JumpAttributes")]
    public float jumpSp;
    public float fallMulti;
    public float lowJumpMulti;
    bool canJump = true;
    bool doubleJumped = false;

    [Header("GroundCheck")]
    public Vector2 pointOffset;
    public Vector2 size;
    public LayerMask groundLayer;
    bool gravityModifier = true;

    [Header("墙判定")]
    public Vector2 leftPointOffset;
    public Vector2 rightPointOffset;
    public Vector2 onWallSize;
    public float wallFallSp;
    //public float wallHoldSec;

    [Header("WallJump")]
    public float wallJumpSpX;
    public float wallJumpSpY;
    bool canWallJump = true;
    bool jumpButtonRelease = true;

    [Header("Dash")]
    public float dashForce;
    public float dragMax;
    public float dragDuration;
    public float dashWait;
    Vector2 dir;

    public Rigidbody2D rig;
    Animator anim;
    public SpriteRenderer sprite;
    public GameObject eggSprite;
    public GameObject shootingEgg;
    public Transform shootingPos;
    public Transform negShootingPos;
    public Transform shootingDir;
    public Transform negShootingDir;

    private void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        curHealth = maxHealth;
        containerHealth = maxContainerHealth;
        haveEgg = true;
    }
    private void Update()
    {
        EggManager();
        TempTimeManager();
    }
    private void FixedUpdate()
    {
        CharacterManager();
        CharacterLocomtion();
    }
    void CharacterManager() 
    {
        onGround = GroundCheck();
        onLeftWall = LeftWallCheck();
        onRightWall = RightWallCheck();
        onWall = onLeftWall ^ onRightWall;
        curVelocity = rig.velocity;

        if (isDamaged)
        {
            canMove = false;
            canJump = false;
        }
        else 
        {
            canMove = true;
        }


        if (throwingTimer <= 0) 
        {
            if (!haveEgg)
                moveSp = 8.5f;
            else
                moveSp = 5.5f;
        }
    }
    void TempTimeManager() 
    {
        if (throwingTimer > 0)
        {
            moveSp = 0.5f;
            canMove = false;
            sprite.flipY = true;
            if (onGround) throwingTimer -= Time.deltaTime;
            rig.drag = 3f;
        }
        else 
        {
            sprite.flipY = false;
            throwingTimer = 0;
            canMove = true;
            rig.drag = 0.5f;
        }
        if (isDamaged)
        {
            if (!onGround) 
            {
                rig.drag = 2f;
            }
            tempDamageTimer += Time.deltaTime;
            if (tempDamageTimer >= 0.5f)
            {
                tempDamageTimer = 0;
                isDamaged = false;
            }
        }
        else 
        {
            isDamaged = false;
            tempDamageTimer = 0;
        }
    }
    void EggManager() 
    {
        if (haveEgg)
        {
            eggSprite.SetActive(true);
            if (babyEnergy <= 100) 
            {
                babyEnergy += Time.deltaTime * 5f;
            }
            if (!onWall) 
            {
                if (Input.GetMouseButton(0))
                {
                    if (!sprite.flipX)
                    {
                        GameObject egg = Instantiate(shootingEgg, shootingPos.transform);
                        egg.SetActive(true);
                        egg.transform.parent = null;
                        egg.transform.position = shootingPos.transform.position;
                        egg.GetComponent<Rigidbody2D>().velocity = curVelocity;
                        if (onGround)
                        {
                            egg.GetComponent<Rigidbody2D>().AddForce(new Vector2(1,0.15f) * throwingForce, ForceMode2D.Impulse);
                        }
                        else 
                        {
                            egg.GetComponent<Rigidbody2D>().AddForce(new Vector2(1f, 0.55f) * throwingForce * 0.8f, ForceMode2D.Impulse);
                        }
                    }
                    else
                    {
                        GameObject egg = Instantiate(shootingEgg, shootingPos.transform);
                        egg.SetActive(true);
                        egg.transform.parent = null;
                        egg.transform.position = shootingPos.transform.position;
                        egg.GetComponent<Rigidbody2D>().velocity = curVelocity;
                        if (onGround)
                        {
                            egg.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1, 0.15f) * throwingForce, ForceMode2D.Impulse);
                        }
                        else
                        {
                            egg.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1f, 0.55f) * throwingForce * 0.8f, ForceMode2D.Impulse);
                        }
                    }
                    haveEgg = false;
                    throwingTimer = 0.5f;
                }
            }
        }
        else 
        {
            babyEnergy -= Time.deltaTime * 10f;
            if (babyEnergy <= 0) 
            {
                babyEnergy = 0;
                Debug.Log("Baby is alone and die");
            }
            eggSprite.SetActive(false);
        }
        if (containerHealth <= 0) 
        {
            containerHealth = 0;
            Debug.Log("Container destroyed, Game Over");
        }
    }
    void CharacterLocomtion() 
    {
        #region 左右移动
        if (canMove)
        {
            if (Input.GetAxisRaw("Horizontal") > inputOffset.x)
            {
                if (rig.velocity.x < moveSp * Time.fixedDeltaTime * 60 )
                    rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, moveSp * Time.fixedDeltaTime * 60, ref velocityX, accTime), rig.velocity.y);
                sprite.flipX = false;
                //anim.SetFloat("Walk", 1f);
            }
            else if (Input.GetAxisRaw("Horizontal") < inputOffset.x * -1)
            {
                if (rig.velocity.x > moveSp * Time.fixedDeltaTime * 60 * -1 )
                    rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, moveSp * Time.fixedDeltaTime * 60 * -1, ref velocityX, accTime), rig.velocity.y);
                sprite.flipX = true;
                //anim.SetFloat("Walk", 1f);
            }
            else
            {
                rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, 0, ref velocityX, decTime), rig.velocity.y);
                //anim.SetFloat("Walk", 0f);
            }
        }
        #endregion

        #region 跳跃
        //跳跃
        if (Input.GetAxis("Jump") == 1 && !isJumping && canJump && onGround && jumpButtonRelease && !onWall && throwingTimer<=0 && groundJumpReset)
        {
            rig.velocity = new Vector2(rig.velocity.x, jumpSp);
            isJumping = true;
            jumpButtonRelease = false;
            groundJumpReset = false;
            //JumpParticle.Play();
            //anim.SetTrigger("Jump");
        }

        if (onGround) //在地上时
        {
            isJumping = false;
            doubleJumped = false;
            canJump = true;
            canWallJump = true;
            //if (isDamaged) 
            //{
            //    isDamaged = false;
            //    rig.velocity = Vector2.zero;
            //}
            if (Input.GetAxis("Jump") == 0 && !groundJumpReset) 
            {
                groundJumpReset = true;
            }
            //anim.SetBool("onGround", true);
            //PlayPartical(WalkPartical);
        }
        else //在空中时
        {
            if (Input.GetAxis("Jump") == 1 && !doubleJumped && !haveEgg && !onWall && jumpButtonRelease)
            {
                Debug.Log("123");
                rig.velocity = new Vector2(rig.velocity.x * 0.9f, jumpSp*0.75f);
                doubleJumped = true;
                jumpButtonRelease = false;
            }
        }
        #endregion

        #region 跳跃重置
        if (!jumpButtonRelease && Input.GetAxis("Jump") == 0)
        {
            jumpButtonRelease = true;
        }

        #endregion

        #region 重力
        if (gravityModifier)
        {
            if (rig.velocity.y < 0)
            {
                rig.velocity += Vector2.up * Physics2D.gravity.y * (fallMulti - 1) * Time.fixedDeltaTime; //下落
            }
            else if (rig.velocity.y > 0 && Input.GetAxis("Jump") != 1) //当玩家上升且没有按下跳跃时
            {
                rig.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMulti - 1) * Time.fixedDeltaTime;
            }
        }
        #endregion

        #region 踩墙跳
        if (canWallJump && !haveEgg)
        {
            if (Input.GetAxis("Jump") == 1 && onWall && !onGround && canWallJump && jumpButtonRelease)
            {
                if (onLeftWall)
                {
                    rig.velocity = new Vector2(wallJumpSpX, wallJumpSpY);
                }
                else
                {
                    rig.velocity = new Vector2(wallJumpSpX * -1, wallJumpSpY);
                }
                jumpButtonRelease = false;
                //SlideParticle.Play();
            }
        }

        #endregion

        #region 爬墙
        if (onWall && !onGround && rig.velocity.y <= 0 && dir.y != -1 && !haveEgg)
        {
            if (onLeftWall && Input.GetAxisRaw("Horizontal")<0)
            {
                rig.velocity = new Vector2(rig.velocity.x, -wallFallSp * Time.fixedDeltaTime * 50);
                //flip = false;
                sprite.flipX = false;
            }
            else if (onRightWall && Input.GetAxisRaw("Horizontal") > 0)
            {
                rig.velocity = new Vector2(rig.velocity.x, -wallFallSp * Time.fixedDeltaTime * 50);
                //flip= true;
                sprite.flipX = true;
            }
            //SlideParticle.transform.parent.localPosition = new Vector3(ParticleSide(), -0.45f, 0);
            //anim.SetBool("onWall", true);
        }
        #endregion
    }
    public void PlayerTakeDamage(int damage, bool damageOnRight) 
    {
        isDamaged = true;
        rig.velocity = Vector2.zero;
        curHealth -= damage;
        if (damageOnRight)
        {
            rig.AddForce(new Vector2(-1f, 0.9f).normalized * 15f, ForceMode2D.Impulse);
        }
        else 
        {
            rig.AddForce(new Vector2(1f, 0.9f).normalized * 15f, ForceMode2D.Impulse);
        }
    }
    bool GroundCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + pointOffset, size, 0, groundLayer);
        if (coll != null)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
    bool LeftWallCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + leftPointOffset, onWallSize, 0, groundLayer);
        if (coll != null)
        {
            if (onGround)
            {
                return false;
            }
            else 
            {
                return true;
            }
        }
        else 
        {
            return false;
        }
    }
    bool RightWallCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + rightPointOffset, onWallSize, 0, groundLayer);
        if (coll != null)
        {
            if (onGround)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4) 
        {
            inWater = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4)
        {
            inWater = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Egg") 
        {
            if (collision.collider.GetComponent<EggManager>().isPickable) 
            {
                Destroy(collision.gameObject);
                haveEgg = true;
            }
        }
        
        if (collision.collider.tag == "Enemy" && !collision.collider.GetComponent<EnemyManager>().isStunned && !isDamaged) 
        {
            if (transform.position.x < collision.transform.position.x) //敌人在右
            {
                PlayerTakeDamage(10, true);
            }
            else 
            {
                PlayerTakeDamage(10, false);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + pointOffset, size);
        Gizmos.DrawWireCube((Vector2)transform.position + leftPointOffset, onWallSize);
        Gizmos.DrawWireCube((Vector2)transform.position + rightPointOffset, onWallSize);
    }
}