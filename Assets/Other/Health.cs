using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Health : NetworkBehaviour
{
    public float HealthAmount
    {
        get { return _healthAmount; }
        set
        {
            if (value < _healthAmount)
                StartCoroutine(shake());
            
            if (_healthAmount > 0 && value <= 0f)
            {
                GameObject created = Instantiate(transform.Find("Particle System").gameObject, transform.position, Quaternion.identity);
                created.SetActive(true);
                created.GetComponent<ParticleSystem>().Play();
                Destroy(created, 5f);

                GetComponent<Rigidbody2D>().velocity = Vector3.zero;

                GetComponent<NetworkTransformChild>().enabled = false;
                GetComponent<NetworkTransformChild>().target = transform;
                
                transform.position = new Vector3(0, 50, 0);
            }

            transform.Find("Canvas").Find("RawImage").GetComponent<RectTransform>().sizeDelta = new Vector2(value / 100f, 0.0736f);
            
            _healthAmount = value;
        }
    } float _healthAmount = 100f;



    bool InFallDeathDelay = false;
    IEnumerator FallDeathDelay()
    {
        InFallDeathDelay = true;
        yield return new WaitForSeconds(1f);
        InFallDeathDelay = false;
    }

    void Update()
    {
        if (isServer && transform.position.y < -6f && InFallDeathDelay == false)
        {
            RpcDie();
            StartCoroutine(FallDeathDelay());
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        HealthAmount = 0f;
    }

    [ClientRpc]
    public void RpcSetHealth(float value)
    {
        HealthAmount = value;
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
