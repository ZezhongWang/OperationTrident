﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class God : MonoBehaviour {
    public string nextScene = "SpaceBattle";
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameMgr.instance.nextScene = nextScene;
            SceneManager.LoadScene("Loading", LoadSceneMode.Single);
            //SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
	}
}
