using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float Speed = 5f;
    public int facingDirection = 1; // 1 for right, -1 for left
    public int verticalFacingDirection = 1; // 1 for up, -1 for down
    public Rigidbody2D rb;
    public Animator anim;


    void FixedUpdate()
    {
        // Move the player
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal > 0 && transform.localScale.x < 0 || horizontal < 0 && transform.localScale.x > 0)
        {
            FlipHorizontal();
        }

        if (vertical > 0 && transform.localScale.y < 0)
        {
            FlipVertical();
        }

        anim.SetFloat("horizontal", Mathf.Abs(horizontal));
        anim.SetFloat("vertical", Mathf.Abs(vertical));

        rb.linearVelocity = new Vector2(horizontal, vertical) * Speed;
    }

    void FlipHorizontal()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void FlipVertical()
    {
        // Chỉ flip lên trên, không flip xuống
        if (transform.localScale.y < 0)
        {
            verticalFacingDirection = 1;
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
    }

    void Flip()
    {
        FlipHorizontal();
        FlipVertical();
    }
}
