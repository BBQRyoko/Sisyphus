using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SalmonManager : MonoBehaviour
{
    [Header("Status")]
    public bool haveEgg;
    public bool onGround;
    bool inWater;
    public bool onWall;
    public bool onLeftWall;
    public bool onRightWall;
    float velocityX;
    bool isJumping;
    float throwingTimer;

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

    [Header("ǽ�ж�")]
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

    Rigidbody2D rig;
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

        if (throwingTimer <= 0) 
        {
            if (!inWater)
                moveSp = 5f;
            else
                moveSp = 6.5f;
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
            rig.drag = 1f;
        }
    }
    void EggManager() 
    {
        if (haveEgg)
        {
            eggSprite.SetActive(true);
            if (!onWall) 
            {
                if (Input.GetMouseButton(0))
                {
                    float throwingForce = 20f;
                    if (!sprite.flipX)
                    {
                        GameObject egg = Instantiate(shootingEgg, shootingPos.transform);
                        egg.SetActive(true);
                        egg.transform.parent = null;
                        egg.transform.position = shootingPos.transform.position;
                        egg.GetComponent<Rigidbody2D>().velocity = new Vector2(curVelocity.x, 0);
                        egg.GetComponent<Rigidbody2D>().AddForce((shootingDir.position - shootingPos.position) * throwingForce, ForceMode2D.Impulse);
                    }
                    else
                    {
                        GameObject egg = Instantiate(shootingEgg, negShootingPos.transform);
                        egg.SetActive(true);
                        egg.transform.parent = null;
                        egg.transform.position = negShootingDir.transform.position;
                        egg.GetComponent<Rigidbody2D>().velocity = new Vector2(curVelocity.x, 0);
                        egg.GetComponent<Rigidbody2D>().AddForce((negShootingDir.position - negShootingPos.position) * throwingForce, ForceMode2D.Impulse);
                    }
                    haveEgg = false;
                    throwingTimer = 0.5f;
                }
            }
        }
        else 
        {
            eggSprite.SetActive(false);
        }
    }
    void CharacterLocomtion() 
    {
        #region �����ƶ�
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

        #region ��Ծ
        //��Ծ
        if (Input.GetAxis("Jump") > 0 && !isJumping && canJump && onGround && jumpButtonRelease && !onWall)
        {
            rig.velocity = new Vector2(rig.velocity.x, jumpSp);
            isJumping = true;
            jumpButtonRelease = false;

            //JumpParticle.Play();
            //anim.SetTrigger("Jump");
        }

        if (onGround) //�ڵ���ʱ
        {
            isJumping = false;
            doubleJumped = false;
            canWallJump = true;

            //anim.SetBool("onGround", true);
            //PlayPartical(WalkPartical);
        }
        else //�ڿ���ʱ
        {
            if (Input.GetAxis("Jump") > 0 && !doubleJumped && !haveEgg && jumpButtonRelease && !onWall && rig.velocity.y >= 0 && !haveEgg)
            {
                rig.velocity = new Vector2(rig.velocity.x, jumpSp * 0.85f);
                doubleJumped = true;
                jumpButtonRelease = false;
            }
        }
        #endregion

        #region ��Ծ����
        if (!jumpButtonRelease && Input.GetAxis("Jump") == 0)
        {
            jumpButtonRelease = true;
        }

        #endregion

        #region ����
        if (gravityModifier)
        {
            if (rig.velocity.y < 0)
            {
                rig.velocity += Vector2.up * Physics2D.gravity.y * (fallMulti - 1) * Time.fixedDeltaTime; //����
            }
            else if (rig.velocity.y > 0 && Input.GetAxis("Jump") != 1) //�����������û�а�����Ծʱ
            {
                rig.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMulti - 1) * Time.fixedDeltaTime;
            }
        }
        #endregion

        #region ��ǽ��
        if (canWallJump)
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

        #region ��ǽ
        if (onWall && !onGround && rig.velocity.y <= 0 && dir.y != -1)
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
    bool GroundCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + pointOffset, size, 0, groundLayer);
        if (coll != null)
            return true;
        else
            return false;
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
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + pointOffset, size);
        Gizmos.DrawWireCube((Vector2)transform.position + leftPointOffset, onWallSize);
        Gizmos.DrawWireCube((Vector2)transform.position + rightPointOffset, onWallSize);
    }
}