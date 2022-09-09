using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public Transform groundCheck;
    bool grounded = false;
    private Rigidbody2D player;
    private float Speed = 6f;
    private float JumpSpeed = 12f;
    private float DownSpeed = 0.03f;
    private void Start()
    {
        player = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));

        if (Input.GetKey(KeyCode.A))
        {
            player.velocity = new Vector2(-Speed, player.velocity.y);
        }
        else if (player.velocity.x < 0)
        {
            player.velocity = new Vector2(player.velocity.x / ((10f * Time.deltaTime) + 1f), player.velocity.y);
        }
        if (Input.GetKey(KeyCode.D))
        {
            player.velocity = new Vector2(Speed, player.velocity.y);
        }
        if (Input.GetKey(KeyCode.A) && (Input.GetKey(KeyCode.D)))
        {
            player.velocity = new Vector2(0, player.velocity.y);
        }
        else if (player.velocity.x > 0)
        {
            player.velocity = new Vector2(player.velocity.x / ((10f * Time.deltaTime) + 1f), player.velocity.y);
        }
        if (grounded == false && Input.GetKey(KeyCode.S))
        {
            player.velocity -= new Vector2(0, DownSpeed);
        }
        if ((Input.GetKeyDown(KeyCode.W)) && (grounded))
        {
            player.velocity = new Vector2(player.velocity.x, JumpSpeed);
            grounded = false;
        }
        if (player.velocity.x < -0.1)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, 1f);
        }
        if (player.velocity.x > 0.1)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, 1f);
        }
    }
}