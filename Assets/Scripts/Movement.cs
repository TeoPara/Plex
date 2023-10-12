using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public Transform groundCheck;
    bool grounded = false;
    private Rigidbody2D player;
    Transform transform;
    void Awake()
    {
        player = GetComponent<Rigidbody2D>();
        transform = base.transform;
    }
    void Update()
    {
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));

        Vector3 v = player.velocity;
        
        bool a = Input.GetKey(KeyCode.A);
        bool d = Input.GetKey(KeyCode.D);
        
        // move left or right
        v.x = d ? 6f : a ? -6f : 0;
        // do neither if both r pressed
        if (a && d)
            v.x = 0;

        // down
        if (grounded == false && Input.GetKey(KeyCode.S))
            v.y -= 0.03f;
        // jump
        if (Input.GetKeyDown(KeyCode.W) && grounded)
        {
            v = new Vector2(v.x, 12f);
            grounded = false;
        }

        if (v.x < -0.1)
            transform.localScale = new Vector3(-1f, transform.localScale.y, 1f);
        if (v.x > 0.1)
            transform.localScale = new Vector3(1f, transform.localScale.y, 1f);
        
        player.velocity = v;
    }
}