using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("MoveAttributes")]
    public float moveSp;
    public float accTime;
    public float decTime;
    public Vector2 inputOffset;
    bool canMove = true;
    bool flip;
    [Header("JumpAttributes")]
    public float jumpSp;
    public float fallMulti;
    public float lowJumpMulti;
    bool canJump = true;
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
    public bool canOnWall;
    //public float wallHoldSec;
    bool onWall;
    bool OnLeftWall;
    bool onRightWall;
    [Header("墙跳")]
    public float wallJumpSpX;
    public float wallJumpSpY;
    public bool canWallJump;
    bool wallJumped;
    public bool jumpButtonRelease = true;
    [Header("Dash")]
    public float dashForce;
    public float dragMax;
    public float dragDuration;
    public float dashWait;
    Vector2 dir;
    bool dashedOnce;


    public Vector2 velocityTEST;

    [Header("Particle")]
    public ParticleSystem JumpParticle;
    public ParticleSystem WallJumpParticle;
    public ParticleSystem DashPartical;
    public ParticleSystem SlideParticle;
    public ParticleSystem WalkPartical;


    Rigidbody2D rig;
    Animator anim;
    [HideInInspector]
    public SpriteRenderer sR;

    float velocityX;

    bool onGround;
    bool isJumping;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sR = GetComponent<SpriteRenderer>();

        SlideParticle.Stop();
        DashPartical.Stop();
        WalkPartical.Stop();

    }

    void FixedUpdate()
    {
        onGround = GroundCheck();
        OnLeftWall = leftWallCheck();
        onRightWall = rightWallCheck();
        onWall = OnLeftWall ^ onRightWall; //亦或符

        velocityTEST = rig.velocity;

        #region 左右移动
        if (canMove)
        {
            if (Input.GetAxisRaw("Horizontal") > inputOffset.x)
            {
                if(rig.velocity.x < moveSp * Time.fixedDeltaTime*60)
                    rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, moveSp * Time.fixedDeltaTime * 60, ref velocityX, accTime), rig.velocity.y);
                sR.flipX = false;
                anim.SetFloat("Walk", 1f);
            }
            else if (Input.GetAxisRaw("Horizontal") < inputOffset.x * -1)
            {
                if (rig.velocity.x > moveSp * Time.fixedDeltaTime * 60*-1)
                    rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, moveSp * Time.fixedDeltaTime * 60 * -1, ref velocityX, accTime), rig.velocity.y);
                sR.flipX = true;
                anim.SetFloat("Walk", 1f);
            }
            else
            {
                rig.velocity = new Vector2(Mathf.SmoothDamp(rig.velocity.x, 0, ref velocityX, decTime), rig.velocity.y);
                anim.SetFloat("Walk", 0f);
            }
        }

        #endregion

        #region 跳跃
        //跳跃
            if (Input.GetAxis("Jump") > 0 && !isJumping && canJump && onGround)
            {
                rig.velocity = new Vector2(rig.velocity.x, jumpSp);
                isJumping = true;
                jumpButtonRelease = false;
                JumpParticle.Play();

                anim.SetTrigger("Jump");
            }

            if (onGround)
            {
                isJumping = false;
                wallJumped = false;
                anim.SetBool("onGround", true);
                PlayPartical(WalkPartical);
            }

            if (!onGround)
            {
                anim.SetBool("onGround", false);
                WalkPartical.Stop();
        }
        
        #endregion

        if (rig.velocity.y <= 0 && !onGround)
        {
            anim.SetBool("Fall", true);
        }
        else
        {
            anim.SetBool("Fall", false);
        }

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

        #region 冲刺
        if (Input.GetAxis("Dash") == 1 && !dashedOnce)
        {
            dashedOnce = true;
            dir = GetDir();

            //将玩家当前所有的动量清零
            rig.velocity = Vector2.zero;
            //施加一个力,让玩家飞出去
            rig.velocity += dir * dashForce;

            StartCoroutine(Dash());
        }

        if (onGround && Input.GetAxisRaw("Dash") == 0)
        {
            dashedOnce = false;
        }
        #endregion

        #region 踩墙跳
        if (canWallJump)
        {
            if (Input.GetAxis("Jump") == 1 && onWall && !onGround && !wallJumped && jumpButtonRelease)
            {
                if (OnLeftWall)
                {
                    rig.velocity = new Vector2(wallJumpSpX, wallJumpSpY);
                }
                else
                {
                    rig.velocity = new Vector2(wallJumpSpX*-1, wallJumpSpY);
                }
                //wallJumped = true;
                jumpButtonRelease = false;
                SlideParticle.Play();

            }

            //if (onGround && Input.GetAxis("Jump") == 0)
                wallJumped = false;
        }

        #endregion

        #region 爬墙
        if (onWall && !onGround && rig.velocity.y <= 0 && dir.y != -1 && canOnWall)
        {
            Debug.Log("Wall");
            //StartCoroutine(wallHold());
            if (OnLeftWall)
            {
                rig.velocity = new Vector2(rig.velocity.x, -wallFallSp * Time.fixedDeltaTime * 50);
                //flip = false;
                sR.flipX = false;
            }
            else
            {
                rig.velocity = new Vector2(rig.velocity.x, -wallFallSp * Time.fixedDeltaTime * 50);
                 //flip= true;
                sR.flipX = true;
            }
            //SlideParticle.transform.parent.localPosition = new Vector3(ParticleSide(), -0.45f, 0);
            anim.SetBool("onWall", true);
            //FallTime = 0;
            
            if (!SlideParticle.isPlaying)
            {
                SlideParticle.Play();
            }
            
        }
        else if (onWall && !onGround && rig.velocity.y <= 0 && dir.y == -1)
        {
            
            if (!SlideParticle.isPlaying)
            {
                SlideParticle.Play();
            }
            
        }
        else
        {
            //anim.SetBool("onWall", false);
            SlideParticle.Stop();
        }
        #endregion

    }


    IEnumerator Dash()
    {
        //关闭玩家的移动和跳跃功能
        canJump = false;
        canMove = false;
        //关闭重力调整
        gravityModifier = false;
        //关闭重力影响
        rig.gravityScale = 0;
        //施加一个空气阻力(Rigidbody.Drag)
        DOVirtual.Float(dragMax, 0, dragDuration, (x)=> rig.drag = x);
        PlayPartical(DashPartical);
        //等待一段时间
        yield return new WaitForSeconds(dashWait);
        //开启所有关闭的内容
        DashPartical.Stop();
        canJump = true;
        canMove = true;
        gravityModifier = true;
        rig.gravityScale = 1;
    }

    /*
    IEnumerator wallHold()
    {
        yield return new WaitForSeconds(wallHoldSec);
    }
    */

       
    public Vector2 GetDir()
    {
        Vector2 tempDir = new Vector2(Input.GetAxisRaw("Horizontal"), 0); //冲刺方向, 只朝水平方向冲刺
        if (tempDir == Vector2.zero)
        {
            tempDir.x = !sR.flipX ? tempDir.x = 1: tempDir.x = -1; //三元法
        }
        return tempDir;
    }

    bool GroundCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + pointOffset, size, 0, groundLayer);
        if (coll != null)
            return true;
        else
            return false;
    }

    bool leftWallCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + leftPointOffset, onWallSize, 0, groundLayer);
        if (coll != null)
            return true;
        else
            return false;
    }

    bool rightWallCheck()
    {
        Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + rightPointOffset, onWallSize, 0, groundLayer);
        if (coll != null)
            return true;
        else
            return false;
    }

    void PlayPartical(ParticleSystem partical)
    {
        if (!partical.isPlaying)
        {
            partical.Play();
        }
        else
        {
            partical.Stop();
            partical.Play();
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
