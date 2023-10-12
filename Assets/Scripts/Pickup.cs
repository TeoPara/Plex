using UnityEngine;

public class Pickup : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameObject weapon = Instantiate(transform.GetChild(1).gameObject, new Vector3(transform.position.x - (2 * -transform.localScale.x), transform.position.y), Quaternion.Euler(new Vector3(0, 0, 0)));
            weapon.transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            weapon.AddComponent<Rigidbody2D>();
            weapon.AddComponent<PolygonCollider2D>();
            Destroy(transform.GetChild(1).gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Weapon"))
            return;
        
        GameObject Weapon = Instantiate(collision.gameObject, new Vector3(this.transform.position.x - (2 * -this.transform.localScale.x), this.transform.position.y), Quaternion.Euler(new Vector3(0,0,0)), this.transform);
        Weapon.transform.localScale = new Vector3(-1, 1, 1);
        Destroy(Weapon.GetComponent<PolygonCollider2D>());
        Destroy(Weapon.GetComponent<Rigidbody2D>());
        Destroy(collision.gameObject);
    }
}
