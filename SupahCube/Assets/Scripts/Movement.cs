using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Movement : MonoBehaviour
{
    #region Public Variables

    public Color cubeColor;
    public Color cubeColorRed;
    public Color cubeColorBlue;

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

    private bool isTouchingGrass;

    private bool isBlue = true;

    private RaycastHit2D hitData;

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

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        l2d = gameObject.GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
        moveHorizontal = Input.GetAxisRaw("Horizontal");

        /*
        \movement = new Vector2(moveHorizontal, 0);

        movement = movement * speed * Time.deltaTime;
        */

        if (Input.GetButtonDown("Fire1"))
        {
            //Debug.Log("Button Hit");
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
            canMove = false;

            if (willBigJump)
            {
                sr.color = Color.yellow;
                l2d.color = Color.magenta;
            }
            
            if(!willBigJump)
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

        if(Input.GetButtonUp("Fire3"))
        {
            canMove = true;
            
            if(willBigJump)
            {
                rb.AddForce(Vector2.up * bigJumpForce, ForceMode2D.Impulse);
                willBigJump = false;

                if(isBlue)
                {
                    sr.color = cubeColorBlue;
                    cubeColor = cubeColorBlue;

                    l2d.color = cubeColor;
                }
                else
                {
                    sr.color = cubeColorRed;
                    cubeColor = cubeColorRed;

                    l2d.color = cubeColor;
                }
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            if(isBlue)
            {
                sr.color = cubeColorRed;
                cubeColor = cubeColorRed;

                l2d.color = cubeColor;

                isBlue = false;
            }
            else
            {
                sr.color = cubeColorBlue;
                cubeColor = cubeColorBlue;

                l2d.color = cubeColor;

                isBlue = true;
            }
        }


        if(!canDash)
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
        float targetSpeed = moveHorizontal * speed;

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Mathf.Sign(speedDif);

        if(canMove)
        {
            rb.AddForce(movement * Vector2.right);
        }
    }

    private void LateUpdate()
    {
        hitData = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("Ground"));

        if (hitData.collider)
        {
            //Debug.Log(hitData.collider.gameObject.name);
            canDoubleJump = true;
            canJump = true;
            isTouchingGrass = true;
            charges = 3;
        }
        else
        {
            isTouchingGrass = false;
        }

        if(isTouchingGrass)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        /*
        if (hitData.collider.CompareTag("Ground"))
        {
            canDoubleJump = true;
            canJump = true;
        }
        */
    }

    public void Jump()
    {
        if(canMove)
        {
            if (canJump)
            {
                rb.AddForce(new Vector2(0, jumpForce - rb.velocity.y), ForceMode2D.Impulse);
                canJump = false;
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
 }

