using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeedUI : MonoBehaviour
{
  
    void Start() {
        GetComponent<TMP_Text>().text = $"Seed: {GameManager.Singleton.Seed}";
    }

   
}
