using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Movement : MonoBehaviour
{
    #region Public Variables

    public float animationScaler = 1;//attempted physics based animation scaler

    //Colors used for the light and sprite on the player
    public Color cubeColor;
    public Color cubeColorRed;
    public Color cubeColorBlue;
    public Color cubeColorBigJumpCharged;

    //Movement speed
    public float speed = 5;
    public float maxSpeed = 100;

    //Movement feels
    public float acceleration;
    public float deceleration;
   
    //Jump heights and dash distance
    public float jumpForce;
    public float dashForce;
    public float bigJumpForce;
    public float jumpCutMultiplier = 2.5f;
    public float lowJumpCutMultiplier = 2;
    //public float jumpTime;

    //Recharge time for abilities
    public float dashRechargeTime;
    public float bigJumpChargeTime;

    //How many dashes can be performed in the air
    public int charges = 2;

    //Position of the linecast that checks if the player is on the ground
    public Transform groundTransform;

    #endregion

    #region Private Variables
    
    //ignore the light setter if the player is charging the super jump
    private bool IgnoreLightSetter;

    private float DirectionFacing;

    //disallow movement if the player is chargign the super jump
    private bool canMove = true;

    //true if the player is on the ground
    private bool isTouchingGrass;

    //Reference to what color the player is
    private bool isBlue = true;

    //Linecast data
    private RaycastHit2D hitData;//Detects BLUE platforms
    private RaycastHit2D hitData2;//Detects RED platforms
    private RaycastHit2D hitData3;//Detects normal platforms

    //Checks how many jumps the player can perform
    private bool canJump = true;
    private bool canDoubleJump = true;
    //private bool isJumping;

    //Checks for how long the player has been charging the super jump and if they have charged long enough to jump
    private float bigJumpTimer = 0;
    private bool willBigJump;

    //Checks if the player can dash based on the dash cooldown
    private bool canDash;
    private float dashTimer = 0;

    //Placeholder to get the horizontal movement axis for the player
    private float moveHorizontal;

    //Components on the player
    private Light2D l2d;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private BoxCollider2D bc;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Getting References to components on the player
        rb = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        l2d = gameObject.GetComponent<Light2D>();
        bc = gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Get the controller joystick axis
        moveHorizontal = Input.GetAxisRaw("Horizontal");

        #region Button Inputs

        //Performs a jump with the white button
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (jumpCutMultiplier - 1) * Time.deltaTime;
        }
        else
        {
            if(rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpCutMultiplier - 1) * Time.deltaTime;
            }
        }
        
        //Performs a dash with the green button
        if(Input.GetButtonDown("Dash"))
        {
            Dash(canDash);
        }

        //Begins charging the big jump
        if(Input.GetButtonDown("SuperJump"))
        {
            bigJumpTimer = 0;
            IgnoreLightSetter = true;
        }

        //Continues charging the big jump
        if (Input.GetButton("SuperJump"))
        {
            BigJumpCharge();
        }

        //Activates the big jump if the player held the button for long enough
        if(Input.GetButtonUp("SuperJump"))
        {
            IgnoreLightSetter = false;
            BigJumpApply();
        }

        //Changes the color of the player
        if (Input.GetButtonDown("ColorChange"))
        {
            ColorChange();
        }

        #endregion

        //Checks if the player can dash based on the dash timer
        if (!canDash)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashRechargeTime)
            {
                canDash = true;
            }
        }

        //Sets the radius of the light based on how many jumps and dashes have been performed
        LightRadiusSetter();
    }

    private void FixedUpdate()
    {
        //Applies the physics based movement
        PhysicsMovement();
    }

    private void LateUpdate()
    {
        #region Jump Checker

        //Detects blue, red, and normal ground types
        LinecastGroundDetection();

        //Checks if the player can jump based on if they're on the ground or not
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
        //Checks if the player is holding the big jump charge
        if (canMove)
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

    public void JumpCut()
    {
      
    }

    public void Dash(bool ableDash)
    {
        //Checks if the player is holding the big jump charge
        if (canMove)
        {
            //Checks if the player still has charges to use and the dash cooldown has passed
            if (charges > 0 && ableDash)
            {
                rb.AddForce(new Vector2(dashForce * DirectionFacing, 0), ForceMode2D.Impulse);
                rb.constraints = RigidbodyConstraints2D.FreezePositionY;
                
                //After a delay, stop the player's momentum
                this.Wait(0.2f, () =>
                {
                    rb.velocity.Set(0, rb.velocity.y);
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    transform.rotation = Quaternion.identity;
                });

                //Reset dash cooldown and use a charge
                dashTimer = 0;
                canDash = false;
                charges--;
            }
        }
    }

    void BigJumpCharge()
    {
        //Check if the player is on the ground
        if (isTouchingGrass)
        {
            //Stop the player from moving if they're holding the charge button
            canMove = false;

            if (!willBigJump)
            {
                //Condense the l2d light and turn to purple as charging the jump
                l2d.color = Color.Lerp(l2d.color, cubeColorBigJumpCharged, bigJumpChargeTime * Time.deltaTime);//Change the color of the light
                l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 0.9f, bigJumpChargeTime * Time.deltaTime);//Change the radius of the light
                l2d.intensity = Mathf.Lerp(l2d.intensity, 25, bigJumpChargeTime * Time.deltaTime);//Change the intensity of the light
                
                //Add to the charge timer
                bigJumpTimer += Time.deltaTime;

                //Checks if the jump is charged
                if (bigJumpTimer >= bigJumpChargeTime && isTouchingGrass)
                {
                    willBigJump = true;
                    bigJumpTimer = 0;
                }
            }

            //if the jump is charged, expand the l2d light to show the player the jump is charged
            if (willBigJump)
            {
                //Change the attributes to show that the jump is charged
                sr.color = Color.Lerp(sr.color, cubeColorBigJumpCharged, 1f * Time.deltaTime);//Change the sprite of the player to purple
                l2d.pointLightOuterRadius = LeanTween.linear(l2d.pointLightOuterRadius, 2.5f, 5 * Time.deltaTime);//Change the Radius to be bigger
                l2d.intensity = Mathf.Lerp(l2d.intensity, 10, 1f * Time.deltaTime);//Change the intensity of the light
            }
        }
    }

    void BigJumpApply()
    {
        //Return movement
        canMove = true;

        //reset back to normal light and sprite colors
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
        //If blue, change to red
        if (isBlue)
        {
            gameObject.layer = 10;

            sr.color = cubeColorRed;
            cubeColor = cubeColorRed;

            l2d.color = cubeColor;

            isBlue = false;
        }
        else
        //if red, change to blue
        {
            gameObject.layer = 9;

            sr.color = cubeColorBlue;
            cubeColor = cubeColorBlue;

            l2d.color = cubeColor;

            isBlue = true;
        }
    }

    #endregion

    public void LightRadiusSetter()
    {
        if(!IgnoreLightSetter)
        {
            //Inefficient and dumb brute force check for all possibilities 
            if (canDoubleJump && charges == 2)
            {
                l2d.pointLightOuterRadius = 1.7f;
                //l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 1.7f, 0.2f * Time.deltaTime);
            }

            if (!canDoubleJump && charges == 2)
            {
                l2d.pointLightOuterRadius = 1.5f;
                //l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 1.5f, 0.2f * Time.deltaTime);
            }

            if (canDoubleJump && charges == 1)
            {
                l2d.pointLightOuterRadius = 1.5f;
                //l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 1.5f, 0.2f * Time.deltaTime);
            }

            if (!canDoubleJump && charges == 1)
            {
                l2d.pointLightOuterRadius = 1.1f;
            }

            if (canDoubleJump && charges == 0)
            {
                l2d.pointLightOuterRadius = 1.1f;
                //l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 1.1f, 0.2f * Time.deltaTime);
            }

            if (!canDoubleJump && charges == 0)
            {
                l2d.pointLightOuterRadius = 0.9f;
                //l2d.pointLightOuterRadius = Mathf.Lerp(l2d.pointLightOuterRadius, 1f, 0.2f * Time.deltaTime);
            }
        } 
    }

    void PhysicsMovement()//I stole this so I don't really understand it
    {
        float targetSpeed = moveHorizontal * speed;

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Mathf.Sign(speedDif);

        if (canMove)
        {
            rb.AddForce(movement * Vector2.right);

            if (moveHorizontal > 0)
            {
                DirectionFacing = 1;
            }

            if (moveHorizontal < 0)
            {
                DirectionFacing = -1;
            }    
        }
    }

    void LinecastGroundDetection()
    {
        //Seperate checks for different types of ground to make Layers work
        hitData = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("BlueGround"));
        hitData2 = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("RedGround"));
        hitData3 = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("Ground"));

        //Checks if the 1st linecast hit something blue
        if (hitData.collider)
        {
            //Do collision with the object if the player is red
            if (!isBlue)
            {
                //Debug.Log(hitData.collider.gameObject.name);
                canDoubleJump = true;
                canJump = true;
                isTouchingGrass = true;
                charges = 2;
            }
        }

        //Checks if the 2nd linecast hit something red
        if (hitData2.collider)
        {
            //Do collision with the object if the player is blue
            if (isBlue)
            {
                //Debug.Log(hitData.collider.gameObject.name);
                canDoubleJump = true;
                canJump = true;
                isTouchingGrass = true;
                charges = 2;
            }
        }

        //Checks if the normal ground
        if (hitData3.collider)
        {
            //Do collision
            canDoubleJump = true;
            canJump = true;
            isTouchingGrass = true;
            charges = 2;
        }

        //If none of the linecasts are hitting something on their respective layers, the player is in the air.
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

