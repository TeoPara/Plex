using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Rigidbody2D rb 
    {
        get
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();
            return _rb;
        }
    } Rigidbody2D _rb;
    SpriteRenderer sr 
    {
        get
        {
            if (_sr == null)
                _sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
            return _sr;
        }
    } SpriteRenderer _sr;
    
    public float W_time = 0;
    public float A_time = 0;
    public float S_time = 0;
    public float D_time = 0;
    
    void Update()
    {
        // Inputs

        if (Input.GetKeyDown(KeyCode.W)) W_time = Time.time;
        if (Input.GetKeyDown(KeyCode.A)) A_time = Time.time;
        if (Input.GetKeyDown(KeyCode.S)) S_time = Time.time;
        if (Input.GetKeyDown(KeyCode.D)) D_time = Time.time;

        bool W = false;
        bool A = false;
        bool S = false;
        bool D = false;

        if (Input.GetKey(KeyCode.W)) W = true;
        if (Input.GetKey(KeyCode.A)) A = true;
        if (Input.GetKey(KeyCode.S)) S = true;
        if (Input.GetKey(KeyCode.D)) D = true;

        if (A && D && A_time > D_time) D = false;
        if (A && D && A_time < D_time) A = false;
        if (W && S && W_time > S_time) S = false;
        if (W && S && W_time < S_time) W = false;

        // Controls

        if (A)
            rb.MovePosition(rb.position + new Vector2(-8f * Time.deltaTime, 0));
        if (D)
            rb.MovePosition(rb.position + new Vector2(8f * Time.deltaTime, 0));

        // Align to pixels

        float x = Convert.ToSingle(Mathf.Round(rb.position.x / 0.05263157894f) * 0.05263157894f);
        float y = Convert.ToSingle(Mathf.Round(rb.position.y / 0.05263157894f) * 0.05263157894f);
        transform.GetChild(0).localPosition = new Vector3(x - rb.position.x, y - rb.position.y);

        // Flip

        if (D)
            sr.flipX = true;
        if (A)
            sr.flipX = false;
    }
}
