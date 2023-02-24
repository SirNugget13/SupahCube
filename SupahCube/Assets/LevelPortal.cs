using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    public SceneLoader sl;
    public int SceneID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        sl.LoadScenes(SceneID);
    }
}
