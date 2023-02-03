using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Movement : MonoBehaviour
{
    private bool isTouchingGround;

    public Color cubeColor;
    public Color cubeColorRed;
    public Color cubeColorBlue;

    public float maxSpeed = 100;
    public float jumpForce;
    public float rayDistance = 1;
    public bool canJump = true;
    public bool canDoubleJump = true;
    public int charges = 3;

    public float dashForce;

    public Transform groundTransform;

    private RaycastHit2D hitData;

    private Vector2 movement;
    public float speed = 5;

    private Light2D l2d;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    
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
        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        movement = new Vector2(moveHorizontal, 0);

        movement = movement * speed * Time.deltaTime;

        if (Input.GetButtonDown("Fire1"))
        {
            //Debug.Log("Button Hit");
            Jump();
        }

        if(Input.GetButtonDown("Fire2"))
        {
            Dash();
        }

        if (Input.GetButtonDown("Fire3"))
        {
            //sr.color = Color.blue;
        }

        if (Input.GetButtonDown("Jump"))
        {
           //sr.color = Color.green;
        }

        rb.position = rb.position + movement;
    }

    private void LateUpdate()
    {
        hitData = Physics2D.Linecast(gameObject.transform.position, groundTransform.position, 1 << LayerMask.NameToLayer("Ground"));

        if (hitData.collider)
        {
            //Debug.Log(hitData.collider.gameObject.name);
            canDoubleJump = true;
            canJump = true;
            isTouchingGround = true;
            charges = 3;
        }
        else
        {
            isTouchingGround = false;
        }

        if(isTouchingGround)
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

    public void Dash()
    {
        if(charges > 0)
        {
            Debug.Log("Dashin");
            rb.AddForce(new Vector2(dashForce * Input.GetAxisRaw("Horizontal"), 0), ForceMode2D.Impulse);
            this.Wait(0.1f, () =>
            {
                rb.velocity.Set(0, rb.velocity.y);
            });
            charges--;
        }
    }
 }

