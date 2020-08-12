using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ScreenTextReporter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Text b = gameObject.GetComponent<Text>();
        //.text = $"{Screen.width} x {Screen.height}"

        Vector2 tpos = new Vector2();

        try
        {
            Touch t = Input.GetTouch(0);
            tpos = t.position;
        }
        catch (System.Exception)
        {
            
            //throw;
        }

        //b.text = $"{Screen.width} x {Screen.height} , {Screen.currentResolution.width} x {Screen.currentResolution.height} , t:{tpos.x},{tpos.y}";
    }
}
