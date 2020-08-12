using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;


public class ShipController : MonoBehaviour
{
    private Rigidbody body;

    public float ForwardThrust = 30;
    public float SideThrust = 10;
    public float RotTorque = 30;

    private bool Controlling = true;

    private FixedJoystick joystick;

    private Vector2 mousepos;

    private Vector2 touchdelta;
    private Vector2 lasttouchpos;
    private int currentTouchIndex = -1;


    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody>();
        joystick = GameObject.FindObjectOfType<FixedJoystick>();
        if (SystemInfo.deviceType==DeviceType.Handheld){
            joystick.gameObject.SetActive(true);
        }else{
            joystick.gameObject.SetActive(false);
        }
            if (SystemInfo.supportsGyroscope){
                Input.gyro.enabled = true;
            }

        //GameObject.FindObjectOfType<Text>().text = $"{SystemInfo.deviceType}";

    }

    private bool skiptouchupdate = false;

    void Update()
    {   
        if (currentTouchIndex==-1){
            for (int i = 1; i <= Input.touchCount; i++)
            {
                var t = Input.GetTouch(i-1);


                if (t.phase == TouchPhase.Began){
                    if (t.position.x>Screen.width/2f){
                        currentTouchIndex=i-1;
                    }
                }
            }
            skiptouchupdate = true;
        }else{
            Touch t;
            try
            {
                t = Input.GetTouch(currentTouchIndex);
            }
            catch (System.Exception)
            {
                skiptouchupdate = true;
                t = Input.GetTouch(currentTouchIndex-1);
                float d = (t.position-lasttouchpos).magnitude;
                for (int i = currentTouchIndex - 2; i >= 0 ; i--)
                {
                    Touch n = Input.GetTouch(i);
                    if ((n.position-lasttouchpos).magnitude<d){
                        d = (n.position-lasttouchpos).magnitude;
                        t = n;
                    }
                }
            }
            
            
            if (t.phase==TouchPhase.Moved){
                if (!skiptouchupdate){
                    touchdelta += t.deltaPosition/10.0f;
                }
                skiptouchupdate = false;
                lasttouchpos = t.position;
            }else if((t.phase==TouchPhase.Stationary)){
                
            }else{
                currentTouchIndex=-1;
            }
        }
        

        if (Input.GetKeyDown(KeyCode.Escape)){
            Controlling = !Controlling;
        }
        if (Controlling){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }else{
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Update is called once per physics frame
    void FixedUpdate()
    {
        if (Controlling){
            float transZ = Input.GetAxis("Vertical") + Input.GetAxis("JoystickLeftY");
            float transX = Input.GetAxis("Horizontal") + Input.GetAxis("JoystickLeftX");
            float transY = Input.GetAxis("ShipMoveVertical");

            Debug.Log(transY);

            transZ+=joystick.Direction.y;
            transX+=joystick.Direction.x;

            //if (Input.GetKey(KeyCode.LeftShift)){transY+=1;}
            //if (Input.GetKey(KeyCode.LeftControl)){transY-=1;}

            float rotX = 0;
            float rotY = 0;
            float rotZ = 0;

            switch (SystemInfo.deviceType)
            {
                case DeviceType.Handheld:
                    rotX += -touchdelta.y;
                    rotY +=  touchdelta.x;
                    /*if (SystemInfo.supportsGyroscope){
                        rotZ +=  Input.gyro.attitude.z/3f;
                    }
                    GameObject.FindObjectOfType<Text>().text = $"{SystemInfo.supportsGyroscope} : {rotZ}";*/
                    break;
                case DeviceType.Desktop:
                    rotX += -Input.GetAxis("Mouse Y");
                    rotY +=  Input.GetAxis("Mouse X");
                    rotZ +=  Input.GetAxis("ShipRotZ");
                    /*if (SystemInfo.supportsGyroscope){
                        rotZ +=  Input.gyro.attitude.z/3f;
                    }
                    GameObject.FindObjectOfType<Text>().text = $"{SystemInfo.supportsGyroscope} : {rotZ}";*/
                    break;
                case DeviceType.Console:
                    rotX += -Input.GetAxis("JoystickRightY")*.5f;
                    rotY +=  Input.GetAxis("JoystickRightX")*.5f;
                    break;
                default:
                    break;
            }

            //rotX += -Input.GetAxis("Mouse Y") +-touchdelta.y;
            //rotY +=  Input.GetAxis("Mouse X") + touchdelta.x;
            //if (Input.GetKey(KeyCode.Q)){rotZ+=.2f;}
            //if (Input.GetKey(KeyCode.E)){rotZ-=.2f;}

            Vector3 TotalForce = new Vector3(transX*SideThrust, transY*SideThrust, transZ*ForwardThrust) * Time.deltaTime;
            Vector3 TotalTorque = new Vector3(rotX, rotY, rotZ)*RotTorque;
            
            body.AddRelativeForce(TotalForce);
            body.AddRelativeTorque(TotalTorque);
        }

        touchdelta = new Vector2(0,0);
    }
}
