using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;

public class Controls : NetworkBehaviour
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

    public List<Sprite> Anim_Walking;
    public List<Sprite> Anim_Attack;
    public List<Sprite> Anim_Attack2;
    public Sprite Falling;

    float chanceAttack1 = 0.5f;
    float chanceAttack2 = 0.5f;

    Coroutine CurrentCoroutine = null;
    string CurrentAnimation = "none"; //none, falling, walk, attack, attack2



    [Command] void CmdDashEffect() => RpcDashEffect();
    [ClientRpc]
    void RpcDashEffect()
    {
        StartCoroutine(DashEffect());
        IEnumerator DashEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                
                GameObject created = Instantiate(Resources.Load<GameObject>("AfterImage"), transform.position, Quaternion.identity);

                created.GetComponent<SpriteRenderer>().flipX = sr.flipX;
                created.GetComponent<SpriteRenderer>().sprite = sr.sprite;

                Destroy(created, 0.1f);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    


    bool InShootCooldown = false;
    IEnumerator ShootCooldown()
    {
        if (InShootCooldown) yield break;
        InShootCooldown = true;
        yield return new WaitForSeconds(1f);
        InShootCooldown = false;
    }

    bool InDashCooldown = false;
    IEnumerator DashCooldown()
    {
        if (InDashCooldown) yield break;
        InDashCooldown = true;
        yield return new WaitForSeconds(3f);
        InDashCooldown = false;
    }

    bool InBashCooldown = false;
    IEnumerator BashCooldown()
    {
        if (InBashCooldown) yield break;
        InBashCooldown = true;
        yield return new WaitForSeconds(3f);
        InBashCooldown = false;
    }


    bool LastFloored = true;
    void Update()
    {
        if (!isLocalPlayer)
            return;


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



        // Ground

        bool Floored = Physics2D.OverlapPoint(new Vector2(transform.position.x, transform.position.y - 1.4f)) != null ||
            Physics2D.OverlapPoint(new Vector2(transform.position.x - 0.5f, transform.position.y - 1.4f)) != null || Physics2D.OverlapPoint(new Vector2(transform.position.x + 0.5f, transform.position.y - 1.4f)) != null;
        if (!LastFloored && Floored && S)
        {
            if (!InBashCooldown)
            {
                Destroy(GameObject.Instantiate(Resources.Load<GameObject>("bash"), transform.position, Quaternion.identity), 0.2f);
                StartCoroutine(BashCooldown());
            }
        }
        LastFloored = Floored;


        // Controls

        if (A)
            rb.velocity = new Vector2(-7f, rb.velocity.y);
        else if (D)
            rb.velocity = new Vector2(7f, rb.velocity.y); //rb.AddForce(new Vector2(1f, 0), ForceMode2D.Impulse);//rb.MovePosition(rb.position + new Vector2(10f * Time.deltaTime, 0));
        else
            rb.velocity = new Vector2(0, rb.velocity.y);

        // jumping and going downwards
        if (W && Input.GetKeyDown(KeyCode.W) && Floored)
            rb.AddForce(new Vector2(0f, 10f), ForceMode2D.Impulse);
        else if (S)
            rb.velocity = new Vector2(rb.velocity.x, -15f);

        // dashing
        if (Input.GetKeyDown(KeyCode.Space) && !InDashCooldown)
        {
            StartCoroutine(DashCooldown());

            CmdDashEffect();

            // directions

            Vector2 bruh = Vector2.zero;

            if (W)
                bruh += new Vector2(0, 3);
            else if (S)
                bruh += new Vector2(0, -3);
            if (A)
                bruh += new Vector2(-3, 0);
            if (D)
                bruh += new Vector2(3, 0);

            rb.MovePosition(rb.position + bruh);
        }

        // Flip

        if (D && sr.flipX == false)
            CmdFlipSprite(true);
        if (A && sr.flipX == true)
            CmdFlipSprite(false);


        // animations

        if (Floored == false)
        {
            if (CurrentAnimation != "falling")
                CmdFallAnimation();
        }
        else
        {
            if (D || A)
            {
                if (CurrentAnimation != "walk" && CurrentAnimation != "attack" && CurrentAnimation != "attack2")
                {
                    CmdWalkAnimation();
                }
            }
            else
            {
                if (CurrentAnimation == "walk")
                {
                    CmdStandStillAnimation();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
            CmdToggleGun(!transform.Find("gun").gameObject.activeInHierarchy);

        // we got gun
        if (transform.Find("gun").gameObject.activeInHierarchy)
        {
            // rotating le gun
            
            Vector3 cursorpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorpos.z = 0;

            if (cursorpos.x < transform.position.x + 0.5f && cursorpos.x > transform.position.x - 0.5f)
            {
                // dont update, going too low or too high
            }
            else
            {
                if (cursorpos.x > transform.position.x)
                {
                    transform.Find("gun").position = transform.position + (cursorpos - transform.position).normalized;
                    CmdFlipGunSprite(false);
                }
                if (cursorpos.x < transform.position.x)
                {
                    transform.Find("gun").position = transform.position + (cursorpos - transform.position).normalized;
                    CmdFlipGunSprite(true);
                }

                Vector3 two = cursorpos - transform.Find("gun").position;
                Vector3 three = new Vector3(two.x / two.x, two.y / two.x);

                transform.Find("gun").transform.eulerAngles = new Vector3(0, 0, Mathf.Atan(three.y) * Mathf.Rad2Deg);
            }

            // shooting le gun
            if (Input.GetMouseButtonDown(0) && InShootCooldown == false)
            {
                CmdShootGun(transform.Find("gun").position, cursorpos);
                StartCoroutine(ShootCooldown());
            }
        }
        // we got no gun
        else
        {
            // melee attack
            if (Input.GetKeyDown(KeyCode.F) && Floored && CurrentAnimation != "attack" && CurrentAnimation != "attack2")
            {
                CmdAttack();
            }
        }
    }


    [Command] void CmdToggleGun(bool b) => RpcToggleGun(b);
    [ClientRpc] void RpcToggleGun(bool b) => transform.Find("gun").gameObject.SetActive(b);



    [Command] void CmdFlipSprite(bool b) => RpcFlipSprite(b);
    [ClientRpc] void RpcFlipSprite(bool b) => sr.flipX = b;

    [Command] void CmdFlipGunSprite(bool b) => RpcFlipGunSprite(b);
    [ClientRpc] void RpcFlipGunSprite(bool b) => transform.Find("gun").GetComponent<SpriteRenderer>().flipX = b;


    [Command] void CmdFallAnimation() => RpcFallAnimation();
    [ClientRpc] void RpcFallAnimation()
    {
        if (CurrentCoroutine != null)
            StopCoroutine(CurrentCoroutine);
        CurrentAnimation = "falling";
        sr.sprite = Falling;
    }


    [Command] void CmdWalkAnimation() => RpcWalkAnimation();
    [ClientRpc] void RpcWalkAnimation()
    {
        if (CurrentCoroutine != null)
            StopCoroutine(CurrentCoroutine);
        CurrentCoroutine = StartCoroutine(Animation(Anim_Walking));
        CurrentAnimation = "walk";
    }


    [Command] void CmdStandStillAnimation() => RpcStandStillAnimation();
    [ClientRpc] void RpcStandStillAnimation()
    {
        if (CurrentCoroutine != null)
            StopCoroutine(CurrentCoroutine);
        CurrentAnimation = "none";
        sr.sprite = Anim_Walking[0];
    }



    // shoot gun
    [Command] void CmdShootGun(Vector3 GunPosition, Vector3 CursorPosition)
    {
        GameObject createdBullet = GameObject.Instantiate(Resources.Load<GameObject>("bullet"), GunPosition, Quaternion.identity);
        NetworkServer.Spawn(createdBullet);

        Destroy(createdBullet, 5f);

        Vector3 direction = (CursorPosition - GunPosition).normalized;

        StartCoroutine(bulletmovemnet(createdBullet, direction));

        IEnumerator bulletmovemnet(GameObject bullet, Vector3 direction)
        {
            for (int i = 0; i < 200; i++)
            {
                foreach(GameObject c in GameObject.FindGameObjectsWithTag("player"))
                {
                    if (c == gameObject)
                        continue;
                    if (Vector3.Distance(c.transform.position, bullet.transform.position) < 0.5f)
                    {
                        RpcSetHealth(c, c.GetComponent<Health>().HealthAmount - 10f, "none");

                        Destroy(bullet);
                        yield break;
                    }
                }

                bullet.transform.position += direction * Time.deltaTime * 25f;
                yield return new WaitForEndOfFrame();
            }
        }
    }


    // melee attacking
    [Command] void CmdAttack()
    {
        if (UnityEngine.Random.Range(0f, 1f) < chanceAttack1)
        {
            RpcAttackOne();
            chanceAttack1 -= 0.1f;
            chanceAttack2 += 0.1f;
        }
        else if (UnityEngine.Random.Range(0f, 1f) < chanceAttack2)
        {
            RpcAttackTwo();
            chanceAttack1 += 0.1f;
            chanceAttack2 -= 0.1f;
        }

        // left
        if (sr.flipX == false)
        {
            foreach (GameObject c in GameObject.FindGameObjectsWithTag("player"))
            {
                if (c == gameObject)
                    continue;
                if (c.transform.position.x < transform.position.x && Vector3.Distance(transform.position, c.transform.position) < 1.25f)
                {
                    RpcSetHealth(c, c.GetComponent<Health>().HealthAmount - 10, "softleft");
                }
            }
        }
        // right
        else
        {
            foreach (GameObject c in GameObject.FindGameObjectsWithTag("player"))
            {
                if (c == gameObject)
                    continue;
                if (c.transform.position.x > transform.position.x && Vector3.Distance(transform.position, c.transform.position) < 1.25f)
                {
                    RpcSetHealth(c, c.GetComponent<Health>().HealthAmount - 10, "softright");

                    c.GetComponent<Controls>().rb.AddForce(new Vector2(5, 3), ForceMode2D.Impulse);
                }
            }
        }
    }
    [ClientRpc] void RpcSetHealth(GameObject target, float amount, string knockback)
    {
        target.GetComponent<Health>().HealthAmount = amount;

        switch (knockback)
        {
            case "softright":
                target.GetComponent<Rigidbody2D>().AddForce(new Vector2(5f, 3f), ForceMode2D.Impulse);
                break;
            case "softleft":
                target.GetComponent<Rigidbody2D>().AddForce(new Vector2(-5f, 3f), ForceMode2D.Impulse);
                break;
            case "hardright":
                target.GetComponent<Rigidbody2D>().AddForce(new Vector2(15f, 10f), ForceMode2D.Impulse);
                break;
            case "hardleft":
                target.GetComponent<Rigidbody2D>().AddForce(new Vector2(-15f, 10f), ForceMode2D.Impulse);
                break;
        }
    }

    [ClientRpc] void RpcAttackOne()
    {
        if (CurrentAnimation != "attack" && CurrentAnimation != "attack2")
        {
            if (CurrentCoroutine != null)
                StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = StartCoroutine(Animation(Anim_Attack, 0.025f, true));
            CurrentAnimation = "attack";

            if (sr.flipX == false)
                rb.MovePosition(rb.position + new Vector2(-0.25f, 0));
            else
                rb.MovePosition(rb.position + new Vector2(0.25f, 0));
        }
    }
    [ClientRpc] void RpcAttackTwo()
    {
        if (CurrentAnimation != "attack" && CurrentAnimation != "attack2")
        {
            if (CurrentCoroutine != null)
                StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = StartCoroutine(Animation(Anim_Attack2, 0.025f, true));
            CurrentAnimation = "attack2";

            if (sr.flipX == false)
                rb.MovePosition(rb.position + new Vector2(-0.25f, 0));
            else
                rb.MovePosition(rb.position + new Vector2(0.25f, 0));
        }
    }





    IEnumerator Animation(List<Sprite> sprites, float frametime = 0.05f, bool once = false)
    {
        sr.sprite = sprites[0];
        while (true)
        {
            yield return new WaitForSeconds(frametime);
            int i = sprites.IndexOf(sr.sprite);
            if (i == sprites.Count - 1)
            {
                if (once == false)
                    i = 0;
                if (once)
                {
                    sr.sprite = sprites[0];
                    yield return new WaitForSeconds(frametime);
                    break;
                }
            }
            sr.sprite = sprites[i+1];
        }
        CurrentAnimation = "none";
    }
}
