using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    public float speed;
    public float jumpForce;

    [Header("Grounding")]
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;

    [Header("Hold To Jump Settings")]
    public float maxJumpTime = 0.3f;
    public float holdForce = 3;
    private bool isJumping;
    private float jumpTimeCounter;
    private int facingDirrection  = 1;
    private float horizontal;


    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        anim.SetFloat("horizontal", Mathf.Abs(horizontal));

        if(horizontal > .1f && facingDirrection < 0 || horizontal < -.1f && facingDirrection > 0)
        {
            Flip();
        }
        
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            anim.SetTrigger("jump");
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if(Input.GetButton("Jump") && isJumping == true)
        {
            if(jumpTimeCounter > 0)
            {
              rb.linearVelocity = new Vector2(rb.linearVelocity.x, holdForce);
              jumpTimeCounter -= Time.deltaTime;   
            }
            else
            {
                isJumping = false;
            }
        }

        if(Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }
    }

    void FixedUpdate()
    {
      rb.linearVelocity =  new Vector2(horizontal * speed, rb.linearVelocity.y);  
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Flip()
    {
        facingDirrection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
