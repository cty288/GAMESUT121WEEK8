using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSystem : MonoBehaviour {
    //from down to up
    public int playerLevel = 0;
    private int prevPlayerLevel = 0;

    private PathSystem pathSystem;

    public bool Ready = false;

    public static SelectionSystem Singleton;

    private PathNode lastNode;
    
    private void Awake() {
        Singleton = this;
        
    }

    private void Start() {
        pathSystem = PathSystem.Singleton;
        StartCoroutine(Floor0Stage());
    }

    

    private IEnumerator Floor0Stage() {
        yield return new WaitForSeconds(1f);
        Ready = true;
        
        List<PathNode> level0Nodes = pathSystem.GetPathNodeAtDepth(pathSystem.PathDepth);
        foreach (PathNode level0Node in level0Nodes) {
           
            level0Node.LevelObject.GetComponent<LevelObject>().OnPlayerMeet();
            
        }
    }




    private void Update() {
        if (Ready) {
            if (prevPlayerLevel != playerLevel)
            {
                prevPlayerLevel = playerLevel;
                OnLevelChange();
            }
        }

        CheckClick();

    }

    private void CheckClick() {
        Camera cam = Camera.main;
        
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(cam.ScreenPointToRay(Input.mousePosition));

            Collider2D collider = ray.collider;

            if (collider != null && collider.gameObject.CompareTag("MapItem")) {
                LevelObject level = collider.GetComponent<LevelObject>();
                if (level.Interactable) {
                    level.OnPlayerSelect();
                    lastNode = level.Node;
                   

                        pathSystem.GetPathNodeAtDepth(pathSystem.PathDepth - playerLevel).ForEach(
                            node => {
                                if (node != lastNode) {
                                    node.LevelObject.GetComponent<LevelObject>().OnPlayerLeave();
                                }
                            });
                    
                    playerLevel++;
                }
                
            }

        }
    }

    private void OnLevelChange() {

        
        foreach (PathNode levelNode in lastNode.ConnectedByNodes) {
            
            levelNode.LevelObject.GetComponent<LevelObject>().OnPlayerMeet();
        }
    }
}
