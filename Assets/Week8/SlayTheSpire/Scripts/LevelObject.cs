using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour {
    [SerializeField]
    private LevelType levelType;
    public LevelType LevelType {
        get => levelType;
        set => levelType = value;
    }

    public PathNode Node;

    private Animator animator;

    public bool Interactable = false;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void OnPlayerMeet() {
        animator.SetBool("PlayerReach", true);
        Interactable = true;
    }

    public void OnPlayerSelect() {
        animator.SetTrigger("PlayerComplete");
        Interactable = false;
        if (levelType == LevelType.Boss) {
            GameManager.Singleton.BossLevelPass();
        }
    }

    public void OnPlayerLeave() {
        animator.SetBool("PlayerReach", false);
        Interactable = false;
    }
}
