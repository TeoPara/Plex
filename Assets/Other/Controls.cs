using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;

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

    [SyncVar] float RColor;
    [SyncVar] float GColor;
    [SyncVar] float BColor;

    void Start()
    {
        if (isLocalPlayer)
            CmdGetRandomColor();
        sr.color = new Color(RColor, GColor, BColor);
    }
    [Command] void CmdGetRandomColor()
    {
        RpcSetColor(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
    }
    [ClientRpc] void RpcSetColor(float r, float g, float b)
    {
        RColor = r;
        GColor = g;
        BColor = b;
        sr.color = new Color(r, g, b);
    }


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


    bool InDashCooldown = false;
    IEnumerator DashCooldown()
    {
        if (InDashCooldown) yield break;

        TMP_Text t = GameObject.Find("Canvas").transform.Find("Dash Cooldown").GetComponent<TMPro.TMP_Text>();

        InDashCooldown = true;

        t.text = "Dash Cooldown: 3";
        for (int i = 0; i <= 2f; i++)
        {
            yield return new WaitForSeconds(1f);
            t.text = "Dash Cooldown: " + ((2-i));
        }

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

        bool Floored =
            // bottom
            Physics2D.OverlapPoint(new Vector2(transform.position.x - 0.5f, transform.position.y - 1.05f)) != null ||
            Physics2D.OverlapPoint(new Vector2(transform.position.x + 0.5f, transform.position.y - 1.05f)) != null ||
            // middle
            Physics2D.OverlapPoint(new Vector2(transform.position.x - 0.55f, transform.position.y)) != null ||
            Physics2D.OverlapPoint(new Vector2(transform.position.x + 0.55f, transform.position.y)) != null ||
            // top
            Physics2D.OverlapPoint(new Vector2(transform.position.x - 0.55f, transform.position.y + 0.3f)) != null ||
            Physics2D.OverlapPoint(new Vector2(transform.position.x + 0.55f, transform.position.y + 0.3f)) != null;

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


        // attempt to pickupe an item
        foreach(GameObject c in GameObject.FindGameObjectsWithTag("item"))
        {
            if (Input.GetKeyDown(KeyCode.E) && Vector3.Distance(transform.position, c.transform.position) < 1.25f)
            {
                if (HeldItem == null && Time.time - c.GetComponent<ItemScript>().LastDropped > 1f && c.GetComponent<ItemScript>().Pickupable && c.GetComponent<ItemScript>().IsBeingHeld == false)
                {
                    CmdPickupItem(c);
                    GameObject.Find("Canvas").transform.Find("Ammo Counter").gameObject.SetActive(true);
                    GameObject.Find("Canvas").transform.Find("Ammo Counter").GetComponent<TMP_Text>().text = c.GetComponent<ItemScript>().AmmoLeft.ToString();
                    break;
                }
            }
        }

        // drop an item
        if (Input.GetKeyDown(KeyCode.Q) && HeldItem != null)
        {
            CmdDropItem();
            GameObject.Find("Canvas").transform.Find("Ammo Counter").gameObject.SetActive(false);
        }

        // we got an item
        if (HeldItem != null)
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
                    HeldItem.transform.position = transform.position + (cursorpos - transform.position).normalized;
                    
                    if (HeldItem.transform.localScale.x == -1)
                        CmdFlipGunXScale(false);
                }
                if (cursorpos.x < transform.position.x)
                {
                    HeldItem.transform.position = transform.position + (cursorpos - transform.position).normalized;

                    if (HeldItem.transform.localScale.x == 1)
                        CmdFlipGunXScale(true);
                }

                Vector3 two = cursorpos - HeldItem.transform.position;
                Vector3 three = new Vector3(two.x / two.x, two.y / two.x);

                HeldItem.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan(three.y) * Mathf.Rad2Deg);
            }

            // shooting le gun
            if (Input.GetMouseButton(0) && HeldItem.GetComponent<ItemScript>().InInteractCooldown == false && HeldItem.GetComponent<ItemScript>().AmmoLeft > 0)
            {
                StartCoroutine(HeldItem.GetComponent<ItemScript>().InteractCooldown());
                if (HeldItem.GetComponent<ItemScript>().ItemName == "sniper")
                    CmdShootGun(HeldItem.transform.position, cursorpos, "sniper", HeldItem);
                if (HeldItem.GetComponent<ItemScript>().ItemName == "auto")
                    CmdShootGun(HeldItem.transform.position, cursorpos, "auto", HeldItem);
                if (HeldItem.GetComponent<ItemScript>().ItemName == "shotgun")
                    CmdShootGun(HeldItem.transform.position, cursorpos, "shotgun", HeldItem);
                if (HeldItem.GetComponent<ItemScript>().ItemName == "landmine")
                {
                    CmdInteractLandmine();
                }

                StartCoroutine(waitupdatecounter(HeldItem));
                IEnumerator waitupdatecounter(GameObject gun)
                {
                    yield return new WaitForSeconds(0.1f);
                    if (gun != null)
                        GameObject.Find("Canvas").transform.Find("Ammo Counter").GetComponent<TMP_Text>().text = gun.GetComponent<ItemScript>().AmmoLeft.ToString();
                    else
                        GameObject.Find("Canvas").transform.Find("Ammo Counter").gameObject.SetActive(false);
                }
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

    [Command] void CmdInteractLandmine() => RpcInteractLandmine();
    [ClientRpc] void RpcInteractLandmine()
    {
        HeldItem.GetComponent<ItemScript>().Interact();
        GameObject todestroy = HeldItem;

        // drop
        GetComponent<NetworkTransformChild>().enabled = false;
        GetComponent<NetworkTransformChild>().target = transform;
        HeldItem.transform.SetParent(null);
        HeldItem = null;

        Destroy(todestroy);
    }



    GameObject HeldItem = null;

    [Command] void CmdPickupItem(GameObject targetItem)
    {
        if (targetItem.GetComponent<ItemScript>().IsBeingHeld == true)
            return;

        targetItem.GetComponent<ItemScript>().IsBeingHeld = true;
        RpcPickupItem(targetItem);
    }
    [ClientRpc] void RpcPickupItem(GameObject targetItem)
    {
        targetItem.GetComponent<ItemScript>().IsBeingHeld = true;
        targetItem.transform.SetParent(transform);
        targetItem.GetComponent<Rigidbody2D>().simulated = false;
        targetItem.GetComponent<BoxCollider2D>().enabled = false;
        HeldItem = targetItem;
        GetComponent<NetworkTransformChild>().target = targetItem.transform;
        GetComponent<NetworkTransformChild>().enabled = true;

        targetItem.GetComponent<NetworkTransform>().enabled = false;
    }



    [Command] void CmdFlipGunXScale(bool a) => RpcFlipGunXScale(a);
    [ClientRpc] void RpcFlipGunXScale(bool a)
    {
        if (a)
            HeldItem.transform.localScale = new Vector3(-1f, 1f, 1f);
        else
            HeldItem.transform.localScale = new Vector3(1f, 1f, 1f);
    }


    [Command] void CmdDropItem() => RpcDropItem();
    [ClientRpc]
    void RpcDropItem()
    {
        GetComponent<NetworkTransformChild>().enabled = false;
        GetComponent<NetworkTransformChild>().target = transform;
        HeldItem.transform.SetParent(null);
        HeldItem.GetComponent<ItemScript>().IsBeingHeld = false;
        HeldItem.GetComponent<Rigidbody2D>().simulated = true;
        HeldItem.GetComponent<Rigidbody2D>().velocity = rb.velocity;
        HeldItem.GetComponent<BoxCollider2D>().enabled = true;
        HeldItem.GetComponent<ItemScript>().LastDropped = Time.time;

        HeldItem.GetComponent<NetworkTransform>().enabled = true;

        HeldItem = null;
    }


    [Command] void CmdFlipSprite(bool b) => RpcFlipSprite(b);
    [ClientRpc] void RpcFlipSprite(bool b) => sr.flipX = b;

    [Command] void CmdFlipGunSprite(bool b) => RpcFlipGunSprite(b);
    [ClientRpc]
    void RpcFlipGunSprite(bool b)
    {
        if (HeldItem != null)
            HeldItem.GetComponent<SpriteRenderer>().flipX = b;
    }

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
    [Command] void CmdShootGun(Vector3 GunPosition, Vector3 CursorPosition, string gunType, GameObject gunItem)
    {
        GameObject createdBullet = GameObject.Instantiate(Resources.Load<GameObject>("bullet"), GunPosition, Quaternion.identity);
        NetworkServer.Spawn(createdBullet);

        float speed = 50;
        float damage = 25;
        switch (gunType)
        {
            case "auto":
                speed = 25f;
                damage = 10f;
                break;
            case "sniper":
                speed = 50f;
                damage = 25f;
                break;
            case "shotgun":
                speed = 30f;
                damage = 15f;
                break;
        }

        Vector3 direction = (CursorPosition - GunPosition).normalized;

        StartCoroutine(bulletmovemnet(createdBullet, direction));

        if (gunType == "shotgun")
        {
            GameObject createdBullet2 = GameObject.Instantiate(Resources.Load<GameObject>("bullet"), GunPosition, Quaternion.identity);
            NetworkServer.Spawn(createdBullet2);
            GameObject createdBullet3 = GameObject.Instantiate(Resources.Load<GameObject>("bullet"), GunPosition, Quaternion.identity);
            NetworkServer.Spawn(createdBullet3);
            float a = Vector3.Distance(transform.position, CursorPosition) / 5f;
            Vector3 direction2 = (CursorPosition + (new Vector3(UnityEngine.Random.Range(-a, a), UnityEngine.Random.Range(-a, a))) - GunPosition).normalized;
            Vector3 direction3 = (CursorPosition + (new Vector3(UnityEngine.Random.Range(-a, a), UnityEngine.Random.Range(-a, a))) - GunPosition).normalized;
            StartCoroutine(bulletmovemnet(createdBullet2, direction2));
            StartCoroutine(bulletmovemnet(createdBullet3, direction3));
        }

        IEnumerator bulletmovemnet(GameObject bullet, Vector3 direction)
        {
            float TimeCreated = Time.time;
            for (;;)
            {
                foreach(GameObject c in GameObject.FindGameObjectsWithTag("player"))
                {
                    if (c == gameObject)
                        continue;
                    if (c.GetComponent<BoxCollider2D>().bounds.Contains(bullet.transform.position))
                    {
                        RpcSetHealth(c, c.GetComponent<Health>().HealthAmount - damage, "none");

                        Destroy(bullet);
                        yield break;
                    }
                }
                foreach(GameObject c in GameObject.FindObjectsOfType<GameObject>().Where(c => c.GetComponent<Break>() != null))
                {
                    if (c.GetComponent<BoxCollider2D>().bounds.Contains(bullet.transform.position))
                    {
                        RpcSetHealthPlatform(c.transform.position, c.GetComponent<Break>().Damage - damage);

                        Destroy(bullet);
                        yield break;
                    }
                }

                bullet.transform.position += direction * Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
                if (Time.time - TimeCreated > 5f)
                {
                    Destroy(bullet);
                    yield break;
                }
            }
        }

        if (gunType == "sniper")
            RpcShootGun("shot");
        else if (gunType == "auto")
            RpcShootGun("smallshot");
        else if (gunType == "shotgun")
            RpcShootGun("shot");

        RpcDecreaseAmmo(gunItem);
    }
    [ClientRpc] void RpcShootGun(string name)
    {
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(name), transform.position, 1f);
    }
    [ClientRpc]
    void RpcDecreaseAmmo(GameObject gun)
    {
        gun.GetComponent<ItemScript>().AmmoLeft -= 1;
        if (gun.GetComponent<ItemScript>().AmmoLeft == 0)
        {
            if (HeldItem == gun)
            {
                GetComponent<NetworkTransformChild>().enabled = false;
                GetComponent<NetworkTransformChild>().target = transform;
                HeldItem.transform.SetParent(null);
                HeldItem.GetComponent<ItemScript>().IsBeingHeld = false;
                HeldItem.GetComponent<Rigidbody2D>().simulated = true;
                HeldItem.GetComponent<Rigidbody2D>().velocity = rb.velocity;
                HeldItem.GetComponent<BoxCollider2D>().enabled = true;
                HeldItem.GetComponent<ItemScript>().LastDropped = Time.time;

                HeldItem = null;
            }

            Destroy(gun);
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
                    RpcSetHealth(c, c.GetComponent<Health>().HealthAmount - 10, "none");
                    RpcPlayPunchSound();
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
                    RpcSetHealth(c, c.GetComponent<Health>().HealthAmount - 10, "none");
                    RpcPlayPunchSound();
                }
            }
        }
    }

    [ClientRpc] void RpcPlayPunchSound()
    {
        List<AudioClip> all = new List<AudioClip>();
        all.Add(Resources.Load<AudioClip>("punch1"));
        all.Add(Resources.Load<AudioClip>("punch2"));
        all.Add(Resources.Load<AudioClip>("punch3"));
        all.Add(Resources.Load<AudioClip>("punch4"));
        AudioSource.PlayClipAtPoint(all[UnityEngine.Random.Range(0, all.Count)], transform.position, 1f);
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
    [ClientRpc] void RpcSetHealthPlatform(Vector3 target, float amount)
    {
        foreach(GameObject c in GameObject.FindObjectsOfType<GameObject>().Where(c => c.GetComponent<Break>() != null))
        {
            if (c.transform.position == target)
                c.GetComponent<Break>().Damage = amount;
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
                {
                    i = 0;
                    
                    if (CurrentAnimation == "walk")
                    {
                        List<AudioClip> all = new List<AudioClip>();
                        all.Add(Resources.Load<AudioClip>("asteroid1"));
                        all.Add(Resources.Load<AudioClip>("asteroid2"));
                        all.Add(Resources.Load<AudioClip>("asteroid3"));
                        all.Add(Resources.Load<AudioClip>("asteroid4"));
                        all.Add(Resources.Load<AudioClip>("asteroid5"));
                        AudioSource.PlayClipAtPoint(all[UnityEngine.Random.Range(0, all.Count)], transform.position, 1f);
                    }
                }
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
