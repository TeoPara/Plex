using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostJoinPanel : MonoBehaviour
{
    NetworkManager NM;
    private void Start()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }
    public void JoinButtonClicked()
    {
        NM.networkAddress = transform.Find("IP").GetComponent<TMPro.TMP_InputField>().text;
        NM.StartClient();
    }
}
