using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Movement : MonoBehaviour
{
    #region Public Variables

    public bool h1;
    public bool h2;
    public bool h3;

    public Color cubeColor;
    public Color cubeColorRed;
    public Color cubeColorBlue;
    public Color cubeColorBigJumpCharged;

    public float speed = 5;
    public float maxSpeed = 100;

    public float acceleration;
    public float deceleration;
   
    public float jumpForce;
    public float dashForce;
    public float bigJumpForce;

    public float dashRechargeTime;
    public float bigJumpChargeTime;

    public int charges = 3;

    public Transform groundTransform;

    #endregion

    #region Private Variables
    private bool canMove = true;

    public bool isTouchingGrass;

    private bool isBlue = true;

    private RaycastHit2D hitData;
    private RaycastHit2D hitData2;
    private RaycastHit2D hitData3;


    private bool canJump = true;
    private bool canDoubleJump = true;

    private float bigJumpTimer = 0;
    private bool willBigJump;

    private bool canDash;
    private float dashTimer = 0;

    private float moveHorizontal;

    private Light2D l2d;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private BoxCollider2D bc;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        l2d = gameObject.GetComponent<Light2D>();
        bc = gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");

        #region Button Inputs

        if (Input.GetButtonDown("Fire1"))
        {
            Jump();
        }

        if(Input.GetButtonDown("Fire2"))
        {
            Dash(canDash);
        }

        if(Input.GetButtonDown("Fire3"))
        {
            bigJumpTimer = 0;
        }

        if (Input.GetButton("Fire3"))
        {
            if(isTouchingGrass)
            {
                canMove = false;

                l2d.color = Color.Lerp(l2d.color, cubeColorBigJumpCharged, bigJumpChargeTime * Time.deltaTime);//Mathf.Lerp(l2d.color, cubeColorBigJumpCharged, bigJumpChargeTime);
                l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 0.8f, bigJumpChargeTime * Time.deltaTime);

                if (willBigJump)
                {
                    sr.color = Color.Lerp(sr.color, cubeColorBigJumpCharged, 0.2f);
                    l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 1.5f, 0.2f);
                }

                if (!willBigJump)
                {
                    bigJumpTimer += Time.deltaTime;

                    if (bigJumpTimer >= bigJumpChargeTime && isTouchingGrass)
                    {
                        willBigJump = true;
                        bigJumpTimer = 0;
                        charges--;
                    }
                }
            }
        }

        if(Input.GetButtonUp("Fire3"))
        {
            canMove = true;

            if (isBlue)
            {
                l2d.pointLightOuterRadius = 1.5f;

                sr.color = cubeColorBlue;
                cubeColor = cubeColorBlue;

                l2d.color = cubeColor;
            }
            else
            {
                l2d.pointLightOuterRadius = 1.5f;

                sr.color = cubeColorRed;
                cubeColor = cubeColorRed;

                l2d.color = cubeColor;
            }


            if (willBigJump)
            {
                rb.AddForce(Vector2.up * bigJumpForce, ForceMode2D.Impulse);
                willBigJump = false;
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            if(isBlue)
            {
                gameObject.layer = 10;
                
                sr.color = cubeColorRed;
                cubeColor = cubeColorRed;

                l2d.color = cubeColor;

                isBlue = false;
            }
            else
            {
                gameObject.layer = 9;

                sr.color = cubeColorBlue;
                cubeColor = cubeColorBlue;

                l2d.color = cubeColor;

                isBlue = true;
            }
        }

        #endregion

        if (!canDash)
        {
            dashTimer += Time.deltaTime;
            Debug.Log(dashTimer);
            if (dashTimer >= dashRechargeTime)
            {
                canDash = true;
            }
        }
        
        //rb.position = rb.position + movement;
    }

    private void FixedUpdate()
    {
        #region Movement

        float targetSpeed = moveHorizontal * speed;

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Mathf.Sign(speedDif);

        if(canMove)
        {
            rb.AddForce(movement * Vector2.right);
        }

        #endregion
    }

    private void LateUpdate()
    {
        #region Jump Manager

        hitData = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("BlueGround"));
        hitData2 = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("RedGround"));
        hitData3 = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("Ground"));

        if (hitData.collider)
        {
            if(!isBlue)
            {
                //Debug.Log(hitData.collider.gameObject.name);
                canDoubleJump = true;
                canJump = true;
                isTouchingGrass = true;
                charges = 3;
            }
        }

        if (hitData2.collider)
        {
            if(isBlue)
            {
                //Debug.Log(hitData.collider.gameObject.name);
                canDoubleJump = true;
                canJump = true;
                isTouchingGrass = true;
                charges = 3;
            }
        }

        if (hitData3.collider)
        {
            canDoubleJump = true;
            canJump = true;
            isTouchingGrass = true;
            charges = 3;
        }

        h1 = hitData.collider;
        h2 = hitData2.collider;
        h3 = hitData3.collider;

        if (!hitData.collider && !hitData2.collider && !hitData3.collider)
        {   
            Debug.Log("In Air");
            isTouchingGrass = false;
        }

        if (isTouchingGrass)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        #endregion
    }

    public void Jump()
    {
        if(canMove)
        {
            if (canJump)
            {
                rb.AddForce(new Vector2(0, jumpForce - rb.velocity.y), ForceMode2D.Impulse);
                canJump = false;
                //isTouchingGrass = false;
            }
            else if (canDoubleJump)
            {
                rb.AddForce(new Vector2(0, jumpForce - rb.velocity.y), ForceMode2D.Impulse);
                canDoubleJump = false;
            }
        }
     }

    public void Dash(bool ableDash)
    {   
        if(canMove)
        {
            if (charges > 0 && ableDash)
            {
                Debug.Log("Dashin");
                rb.AddForce(new Vector2(dashForce * Input.GetAxisRaw("Horizontal"), 0), ForceMode2D.Impulse);

                this.Wait(0.1f, () =>
                {
                    rb.velocity.Set(0, rb.velocity.y);
                });

                dashTimer = 0;
                canDash = false;
                charges--;
            }
        }
    }

    /*
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Blue") && gameObject.layer == 7)
        {
            Physics2D.IgnoreCollision(bc, collision.collider);
            
        }
        else
        {
            if (collision.gameObject.CompareTag("Red") && gameObject.layer == 8)
            {
                Physics2D.IgnoreCollision(bc, collision.collider);
            }
        }
    }
    */
}

