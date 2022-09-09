using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    List<Transform> list = new List<Transform>();

    void Start()
    {
        StartCoroutine(loop());
        foreach (Transform t in transform)
            list.Add(t);
    }
    IEnumerator loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            Instantiate(Resources.Load<GameObject>("pickup_sniper"), list[Random.Range(0, list.Count)].position, Quaternion.identity);
        }
    }
}
