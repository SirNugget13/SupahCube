using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public float jumpForce;
    public float rayDistance = 1;
    public bool canJump = true;
    public bool canDoubleJump = true;

    public Transform groundTransform;

    private RaycastHit2D hitData;

    private Vector3 movement;
    public float speed = 5;

    private Rigidbody2D rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        movement = new Vector3(moveHorizontal, 0, 0f);

        movement = movement * speed * Time.deltaTime;

        
        if(Input.GetButtonDown("Fire1"))
        {
            //Debug.Log("Button Hit");
            Jump();
        }

        if(Input.GetButtonDown("Fire2"))
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }

        if (Input.GetButtonDown("Fire3"))
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        if (Input.GetButtonDown("Jump"))
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }

        Debug.DrawRay(gameObject.transform.position, Vector2.down, Color.red);

        hitData = Physics2D.Linecast(gameObject.transform.position, );

        if(hitData.collider != null)
        {
            Debug.Log(hitData.collider.gameObject.name);
        }

        if (hitData.collider.CompareTag("Ground"))
        {
            canDoubleJump = true;
            canJump = true;
        }

        transform.position += movement;
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
 }

