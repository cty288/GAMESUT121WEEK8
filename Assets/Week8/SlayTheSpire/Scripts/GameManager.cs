using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class GameManager : MonoBehaviour {
    public static GameManager Singleton;

    [Header("Random Related Stuff")]
   

    public Random random;

    private TMP_InputField inputField;
    private Button restartRandomButton;
    private Button restartWithSeedButton;


    [HideInInspector]
    public int Seed;

    private void Awake() {
        if (Singleton != null) {
            Destroy(this.gameObject);
        }
        else {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }

        SetRandomSeed();
    }

    private void Update() {
        if (!restartRandomButton) {
            restartRandomButton = GameObject.Find("RestartRandomButton").GetComponent<Button>();
            restartWithSeedButton = GameObject.Find("RestartWithSeed").GetComponent<Button>();
            inputField = GameObject.Find("SeedInput").GetComponent<TMP_InputField>();

            restartRandomButton.onClick.AddListener(RestartGameRandom);
            restartWithSeedButton.onClick.AddListener(RestartGameWithSeed);
        }
    }

    private void SetRandomSeed() {
        Seed = UnityEngine.Random.Range(-1000000000, 1000000000);
        random = new Random(Seed);
    }

    public void RestartGameRandom() {
        SetRandomSeed();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGameWithSeed() {
        int.TryParse(inputField.text,out Seed);
        random = new Random(Seed);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
