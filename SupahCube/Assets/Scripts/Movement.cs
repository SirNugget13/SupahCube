using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Movement : MonoBehaviour
{
    #region Public Variables

    public float animationScaler = 1;

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
    private bool doIgnoreChargeLightChange;

    private bool canMove = true;

    private bool isTouchingGrass;

    private bool isBlue = true;

    private RaycastHit2D hitData;
    private RaycastHit2D hitData2;
    private RaycastHit2D hitData3;

    private float curRadius;

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
            BigJumpCharge();
        }

        if(Input.GetButtonUp("Fire3"))
        {
            BigJumpApply();
        }

        if (Input.GetButtonDown("Jump"))
        {
            ColorChange();
        }

        #endregion

        if (!canDash)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashRechargeTime)
            {
                canDash = true;
            }
        }     
    }

    private void FixedUpdate()
    {
        PhysicsMovement();
    }

    private void LateUpdate()
    {
        #region Jump Checker

        LinecastGroundDetection();

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

    #region Custom Functions

    #region Button Functions

    public void Jump()
    {
        if (canMove)
        {
            if (canJump)
            {
                rb.AddForce(new Vector2(0, jumpForce - rb.velocity.y), ForceMode2D.Impulse);
                canJump = false;
                ChargeManager();
            }
            else if (canDoubleJump)
            {
                rb.AddForce(new Vector2(0, jumpForce - rb.velocity.y), ForceMode2D.Impulse);
                canDoubleJump = false;
                ChargeManager();
            }
        }
    }

    public void Dash(bool ableDash)
    {
        if (canMove)
        {
            if (charges > 0 && ableDash)
            {
                rb.AddForce(new Vector2(dashForce * Input.GetAxisRaw("Horizontal"), 0), ForceMode2D.Impulse);

                this.Wait(0.1f, () =>
                {
                    rb.velocity.Set(0, rb.velocity.y);
                });

                dashTimer = 0;
                canDash = false;
                charges--;
                ChargeManager();
            }
        }
    }

    void BigJumpCharge()
    {
        if (isTouchingGrass)
        {
            canMove = false;

            if (!willBigJump)
            {
                //Condense the l2d light and turn to purple as charging the jump
                l2d.color = Color.Lerp(l2d.color, cubeColorBigJumpCharged, bigJumpChargeTime * Time.deltaTime);
                l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 0.9f, bigJumpChargeTime * Time.deltaTime);
                l2d.intensity = Mathf.Lerp(l2d.intensity, 25, bigJumpChargeTime * Time.deltaTime);

                bigJumpTimer += Time.deltaTime;

                //Checks if the jump is charged
                if (bigJumpTimer >= bigJumpChargeTime && isTouchingGrass)
                {
                    willBigJump = true;
                    bigJumpTimer = 0;
                }
            }

            //if the jump is charged, expand the l2d light to showed charged
            if (willBigJump)
            {
                sr.color = Color.Lerp(sr.color, cubeColorBigJumpCharged, 1f * Time.deltaTime);
                l2d.pointLightOuterRadius = LeanTween.linear(l2d.pointLightOuterRadius, 2.5f, 5 * Time.deltaTime);
                l2d.intensity = Mathf.Lerp(l2d.intensity, 10, 1f * Time.deltaTime);
            }
        }
    }

    void BigJumpApply()
    {
        canMove = true;

        //reset back to normal light
        if (isBlue)
        {
            l2d.pointLightOuterRadius = 1.7f;
            l2d.intensity = 10;

            sr.color = cubeColorBlue;
            cubeColor = cubeColorBlue;

            l2d.color = cubeColor;
        }
        else
        {
            l2d.pointLightOuterRadius = 1.7f;
            l2d.intensity = 10;

            sr.color = cubeColorRed;
            cubeColor = cubeColorRed;

            l2d.color = cubeColor;
        }

        //Apply the jump force
        if (willBigJump)
        {
            rb.AddForce(Vector2.up * bigJumpForce, ForceMode2D.Impulse);
            willBigJump = false;
        }
    }

    public void ColorChange()
    {
        if (isBlue)
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

    public void ChargeManager()
    {
        /*
        curRadius = l2d.pointLightOuterRadius;
        
        if(canJump && canDoubleJump)
        {
            curRadius = 1.7f;
            curRadius =- curRadius - (0.2f * (2 - charges));
        }
        else
        {
            if (canDoubleJump && !canJump)
            {
                curRadius = 1.5f;
                curRadius = curRadius - (0.2f * (2 - charges));
            }
            else
            {
                if (!canDoubleJump && !canJump)
                {
                    curRadius = 1.2f;
                    curRadius = curRadius - (0.2f * (2 - charges));
                }
            }
        }

        l2d.pointLightOuterRadius = curRadius;
        */
    }
    
    void PhysicsMovement()
    {
        float targetSpeed = moveHorizontal * speed;

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Mathf.Sign(speedDif);

        if (canMove)
        {
            rb.AddForce(movement * Vector2.right);
        }
    }

    void LinecastGroundDetection()
    {
        hitData = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("BlueGround"));
        hitData2 = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("RedGround"));
        hitData3 = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("Ground"));

        if (hitData.collider)
        {
            if (!isBlue)
            {
                //Debug.Log(hitData.collider.gameObject.name);
                canDoubleJump = true;
                canJump = true;
                isTouchingGrass = true;
                charges = 2;
                ChargeManager();
            }
        }

        if (hitData2.collider)
        {
            if (isBlue)
            {
                //Debug.Log(hitData.collider.gameObject.name);
                canDoubleJump = true;
                canJump = true;
                isTouchingGrass = true;
                charges = 2;
                ChargeManager();
            }
        }

        if (hitData3.collider)
        {
            canDoubleJump = true;
            canJump = true;
            isTouchingGrass = true;
            charges = 2;
            ChargeManager();
        }

        if (!hitData.collider && !hitData2.collider && !hitData3.collider)
        {
            isTouchingGrass = false;
        }
    }

    /*
    float VelocityToScaleXCalc(float xScale)
    {
        if(rb.velocity.x != 0 && xScale > 0.4)
        {
            return xScale / (rb.velocity.x / animationScaler);
        }
        else
        {
            return xScale;
        }
    }

    float VelocityToScaleYCalc(float yScale)
    {
        if (rb.velocity.y != 0 && yScale > 0.4)
        {
            return yScale / (rb.velocity.y / animationScaler);
        }
        else
        {
            return yScale;
        }
    }
    */
    #endregion
}

