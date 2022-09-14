using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break : MonoBehaviour
{
    [SerializeField]
    Sprite BrokeTexture;
    
    public Sprite SpriteRepaired;

    [SerializeField]
    GameObject Pieces;
    public float Damage
    {
        get { return _damageAmount; }
        set
        {
            _damageAmount = value;
            if (_damageAmount <= 25f/2f && this.GetComponent<SpriteRenderer>().sprite != BrokeTexture)
            {
                this.GetComponent<SpriteRenderer>().sprite = BrokeTexture;
            }
            if (_damageAmount <= 0)
            {
                GameObject pieces = Instantiate(Pieces, this.transform.position, Quaternion.Euler(0, 0, 0));
                Destroy(pieces, 3);

                GetComponent<BoxCollider2D>().enabled = false;
                GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
    float _damageAmount = 25;
}
