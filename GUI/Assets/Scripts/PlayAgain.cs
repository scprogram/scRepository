using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAgain : MonoBehaviour {
    public GameObject player;
	void OnEnable()
    {
        if (player.GetComponent<PlayerControl>().getDead())
        {
            player.GetComponent<PlayerControl>().enabled = true;
        }
    }
}
