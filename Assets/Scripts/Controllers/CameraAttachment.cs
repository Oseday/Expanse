using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttachment : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Camera;
    public bool Enabled;
    public Vector3 RelativePos = new Vector3(0,0.3f,2);

    void Start()
    {
        Camera = GameObject.Find("/MainCamera");;
    }

    // Update is called once per frame
    void Update()
    {
        if (Enabled) {
            Camera.transform.position = gameObject.transform.rotation*RelativePos + gameObject.transform.position;
            Camera.transform.rotation = gameObject.transform.rotation;
        }
    }
}
