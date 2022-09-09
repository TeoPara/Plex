using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float HealthAmount
    {
        get { return _healthAmount; }
        set
        {

            if (value < _healthAmount)
                StartCoroutine(shake());
            
            _healthAmount = value;
       
            if (_healthAmount <= 0f)
            {
                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                _healthAmount = 100f;
                transform.position = new Vector3(0, 5, 0);
            }

            transform.Find("Canvas").Find("RawImage").GetComponent<RectTransform>().sizeDelta = new Vector2(_healthAmount / 100f, 0.0736f);
        }
    } float _healthAmount = 100f;

    void Update()
    {
        if (transform.position.y < -15f)
            HealthAmount = 0f;
    }

    public IEnumerator shake()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.Find("Sprite").localPosition = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), 0);
            yield return new WaitForEndOfFrame();
        }
        transform.Find("Sprite").localPosition = Vector3.zero;
    }
}
