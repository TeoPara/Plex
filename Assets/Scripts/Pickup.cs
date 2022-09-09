using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameObject Weapon = Instantiate(this.transform.GetChild(1).gameObject, new Vector3(this.transform.position.x - (2 * -this.transform.localScale.x), this.transform.position.y), Quaternion.Euler(new Vector3(0, 0, 0)));
            Weapon.transform.localScale = new Vector3(-this.transform.localScale.x, 1, 1);
            Weapon.AddComponent<Rigidbody2D>();
            Weapon.AddComponent<PolygonCollider2D>();
            Destroy(this.transform.GetChild(1).gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Weapon")
        {
            GameObject Weapon = Instantiate(collision.gameObject, new Vector3(this.transform.position.x - (2 * -this.transform.localScale.x), this.transform.position.y), Quaternion.Euler(new Vector3(0,0,0)), this.transform);
            Weapon.transform.localScale = new Vector3(-1, 1, 1);
            Destroy(Weapon.GetComponent<PolygonCollider2D>());
            Destroy(Weapon.GetComponent<Rigidbody2D>());
            Destroy(collision.gameObject);
        }
    }
}
