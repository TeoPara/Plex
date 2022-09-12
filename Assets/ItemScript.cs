using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    [HideInInspector] public bool IsBeingHeld = false;
    public bool Pickupable = true;
    public string ItemName;
    public float InteractCooldownTime = 0f;
    public bool InInteractCooldown = false;
    public IEnumerator InteractCooldown()
    {
        if (InInteractCooldown) yield break;
        InInteractCooldown = true;
        yield return new WaitForSeconds(InteractCooldownTime);
        InInteractCooldown = false;
    }

    public float LastDropped = 0f;

    public int AmmoLeft;

    public void Interact()
    {
        if (ItemName == "landmine")
        {
            GameObject created = GameObject.Instantiate(Resources.Load<GameObject>("landmine_placed"), transform.position, Quaternion.identity);
            created.GetComponent<ItemScript>().StartLandmine();
        }
    }

    void Update()
    {
        if (GetComponent<Rigidbody2D>().simulated && Physics2D.OverlapPoint(GetComponent<Rigidbody2D>().ClosestPoint(new Vector2(transform.position.x, transform.position.y - 10f)) + new Vector2(0,-0.1f)) != null)
            GetComponent<Rigidbody2D>().velocity /= 1.05f;
    }

    void StartLandmine()
    {
        StartCoroutine(loop());
        IEnumerator loop()
        {
            yield return new WaitForSeconds(2f);

            while (true)
            {
                foreach (GameObject c in GameObject.FindGameObjectsWithTag("player"))
                {
                    if (Vector3.Distance(transform.position, c.transform.position) < 1.25f)
                    {
                        c.GetComponent<Health>().HealthAmount -= 50f;
                        GameObject created = Instantiate(Resources.Load<GameObject>("Explosion"), transform.position, Quaternion.identity);
                        created.GetComponent<ParticleSystem>().Play();
                        Destroy(created, 5);
                        Destroy(gameObject);
                        yield break;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }


    private void OnDestroy()
    {
        if (Gamemode.SpawnedItems.Contains(gameObject))
            Gamemode.SpawnedItems.Remove(gameObject);
    }
}
