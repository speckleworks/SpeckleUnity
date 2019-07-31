using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeckleSendTransform : SpeckleSend
{  
    //set to true when moving, if transform has not moved, and hasMoved is true, send update
    bool hasMoved = false;
    
    private void Start()
    {
       obj = new SpeckleUnityTransform(this.gameObject);
       Sender?.RegisterObject(obj);
    }

    void Update()
    {
        //don't update every frame - check once it has stopped moving          
        if (transform.hasChanged)
        {
            hasMoved = true;
            transform.hasChanged = false;
        } else
        {
            if (hasMoved)
            {
                hasMoved = false;
                if (obj != null)
                {
                    obj.OnValueChanged();
                }
            }
        }

        
    }
}
