using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCursorParticles : MonoBehaviour
{
    private Camera cam;

    [SerializeField]
    Transform cursorPos;

    [SerializeField]
    float distanceToCam = 0;

    Vector3 newPos;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        newPos = new Vector3();
        FindObjectOfType<AudioManager>().Play("Cats");
    }

    // Update is called once per frame
    void Update()
    {
        newPos = cam.ScreenToWorldPoint(new Vector3(cursorPos.position.x, cursorPos.position.y, distanceToCam));
        float xNewPosNormalized = ((newPos.x /Screen.width)*2f)-1f;
        float yNewPosNormalized = ((newPos.y /Screen.height)*2f)-1f;
        newPos.x = xNewPosNormalized;
        newPos.y = yNewPosNormalized;
        Debug.Log(newPos);
        Debug.Log(cam.ScreenToWorldPoint(new Vector3(cursorPos.position.x, cursorPos.position.y, distanceToCam)));
        transform.position = newPos;
    }
}
