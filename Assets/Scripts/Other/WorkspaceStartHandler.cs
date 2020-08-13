using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkspaceStartHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] s = GameObject.FindGameObjectsWithTag("RemoveFromWorkspaceOnLoad");
        foreach (var obj in s)
        {
            Destroy(obj);
        }
    }
}
