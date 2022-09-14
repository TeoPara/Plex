using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;
using System;

public class Gamemode : NetworkBehaviour
{
    public static List<GameObject> SpawnedItems = new List<GameObject>();
    public static List<GameObject> PlayerList;
    void Start()
    {
        StartCoroutine(wait());
        IEnumerator wait()
        {   
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                if (NetworkServer.active && NetworkServer.connections.All(c => c.Value.isReady))
                {
                    StartGame();
                    yield break;
                }
            }
        }
    }

    [SyncVar(hook = nameof(lehook))]
    public string CurrentlyActiveMap = " ";
    void lehook(string oldValue, string newValue)
    {
        GameObject.Find("Maps").transform.Find(oldValue)?.gameObject.SetActive(false);
        GameObject.Find("Maps").transform.Find(newValue)?.gameObject.SetActive(true);
    }

    void StartGame()
    {
        // Server only
        if (!isServer) return;

        // adding maps to list
        List<Transform> AllMaps = new List<Transform>();
        foreach (Transform t in GameObject.Find("Maps").transform)
            AllMaps.Add(t);

        // shuffle the list of maps
        for (int i = 0; i < AllMaps.Count; i++)
        {
            Transform current = AllMaps[i];

            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                AllMaps.RemoveAt(AllMaps.IndexOf(current));
                AllMaps.Insert(0, current);
            }
            else
            {
                AllMaps.RemoveAt(AllMaps.IndexOf(current));
                AllMaps.Add(current);
            }
        }

        StartNextRound(0);
        void StartNextRound(int SelectedMapIndex)
        {
            // Load the map
            Transform CurrentMap = AllMaps[SelectedMapIndex];

            GameObject.Find("Maps").transform.Find(CurrentMap.gameObject.name).gameObject.SetActive(true);
            CurrentlyActiveMap = CurrentMap.gameObject.name;

            RepairPlatforms();

            // get players

            PlayerList = GameObject.FindGameObjectsWithTag("player").ToList();

            // reset player health
            foreach (GameObject player in PlayerList)
                player.GetComponent<Health>().RpcSetHealth(100f);

            // teleport players to player spawn points

            List<GameObject> TempPlayerList = PlayerList.ToList();
            List<Transform> PlayerSpawnPoints = new List<Transform>();
            foreach (Transform t in CurrentMap.Find("PlayerSpawnpoints"))
                PlayerSpawnPoints.Add(t);
            for (int i = 0; i < PlayerSpawnPoints.Count; i++)
            {
                Transform current = PlayerSpawnPoints[i];

                if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                {
                    PlayerSpawnPoints.RemoveAt(PlayerSpawnPoints.IndexOf(current));
                    PlayerSpawnPoints.Insert(0, current);
                }
                else
                {
                    PlayerSpawnPoints.RemoveAt(PlayerSpawnPoints.IndexOf(current));
                    PlayerSpawnPoints.Add(current);
                }
            }
            REPEAT: foreach (Transform t in PlayerSpawnPoints)
            {
                if (TempPlayerList.Count <= 0) break;

                GameObject player = TempPlayerList[UnityEngine.Random.Range(0, TempPlayerList.Count())];
                player.transform.position = t.position;
                TeleportPlayer(player, t.position);
                TempPlayerList.Remove(player);
            }
            if (TempPlayerList.Count > 0) goto REPEAT;

            // start item spawning

            List<Transform> itemSpawns = new List<Transform>();
            foreach (Transform t in CurrentMap.Find("ItemSpawnpoints"))
                itemSpawns.Add(t);
            Coroutine startedItemSpawnLoop = StartCoroutine(itemSpawnLoop());
            IEnumerator itemSpawnLoop()
            {
                while (true)
                {
                    GameObject spawned = Instantiate(Resources.Load<GameObject>(new List<string>() { "gun", "gun2", "gun3", "blade", "landmine", "potion", "nade"  }[UnityEngine.Random.Range(0, 7)]), itemSpawns[UnityEngine.Random.Range(0, itemSpawns.Count)].position, Quaternion.identity);
                    NetworkServer.Spawn(spawned);
                    SpawnedItems.Add(spawned);
                    yield return new WaitForSeconds(5f);
                }
            }

            // wait for winner
            StartCoroutine(waitForWinner());
            IEnumerator waitForWinner()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1f);

                    int count = 0;
                    foreach (GameObject c in PlayerList)
                        if (c.GetComponent<Health>().HealthAmount > 0f)
                            count++;
                    if (PlayerList.Count > 1 && count < 2)
                    {
                        Debug.Log("Calling increasescoreborad text");

                        PlayerList.Find(c => c.GetComponent<Health>().HealthAmount > 0f).GetComponent<Controls>().OurScoreBoardTextWins += 1;
                        IncreaseScoreBoardText(PlayerList.Find(c => c.GetComponent<Health>().HealthAmount > 0f));

                        break;
                    }

                    else if (PlayerList.Count < 2 && count < 1)
                        break;
                    else if (PlayerList.Count < 2 && NetworkServer.connections.Count > 1)
                        break;
                }

                yield return new WaitForSeconds(1.5f);

                // stop item spawning
                StopCoroutine(startedItemSpawnLoop);

                // drop all items
                foreach (GameObject c in PlayerList)
                    c.GetComponent<Controls>().RpcDropItem();

                // destroy all spawned items
                foreach (GameObject c in SpawnedItems)
                    NetworkServer.Destroy(c);

                // load next map and round
                CurrentMap.gameObject.SetActive(false);
                if (AllMaps.IndexOf(CurrentMap) == AllMaps.Count - 1)
                    StartNextRound(0);
                else
                    StartNextRound(AllMaps.IndexOf(CurrentMap) + 1);
            }
        }
    }


    [ClientRpc]
    void IncreaseScoreBoardText(GameObject player)
    {
        Debug.Log("got increasescoreboard text. player is " + (player?.name ?? "null") + ". His scoreboard text is " + (player.GetComponent<Controls>().OurScoreBoardText.name ?? "null"));
        player.GetComponent<Controls>().OurScoreBoardText.GetComponent<TMP_Text>().text = (Convert.ToInt32(player.GetComponent<Controls>().OurScoreBoardText.GetComponent<TMP_Text>().text) + 1).ToString();
    }

    [ClientRpc]
    void RepairPlatforms()
    {
        // repair platforms
        foreach (GameObject c in GameObject.FindGameObjectsWithTag("platform").Where(c => c.GetComponent<Break>() != null))
        {
            c.GetComponent<SpriteRenderer>().sprite = c.GetComponent<Break>().SpriteRepaired;
            c.GetComponent<SpriteRenderer>().enabled = true;
            c.GetComponent<BoxCollider2D>().enabled = true;
            c.GetComponent<Break>().Damage = 25f;
        }
    }

    [ClientRpc]
    void TeleportPlayer(GameObject player, Vector3 position)
    {
        player.transform.position = position;
        Debug.Log("Client: received the teleport command");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
