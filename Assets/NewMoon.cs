using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMoon : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    void Start()
    {
        StartCoroutine(loop());
        IEnumerator loop()
        {
            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().sprite = sprites[1];

            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().sprite = sprites[2];

            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().sprite = sprites[3];

            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().sprite = sprites[4];

            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().sprite = sprites[5];

            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().sprite = sprites[6];

            yield return new WaitForSeconds(0.05f);
            Destroy(gameObject);
        }
    }
}
