using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoCallbacksHandler : MonoBehaviour
{
    [SerializeField] SerialController serialController;
    public void OnConstellationPlaced()
    {
        serialController.SendSerialMessage("A");
    }
}
