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

    [SerializeField]
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float normalizedCursorPosX = Mathf.InverseLerp((-Screen.width)/2f, (Screen.width)/2f, cursorPos.localPosition.x );
        float normalizedCursorPosY = Mathf.InverseLerp((-Screen.height)/2f, (Screen.height)/2f, cursorPos.localPosition.y );

        transform.position = cam.ViewportToWorldPoint(new Vector3(normalizedCursorPosX, normalizedCursorPosY, distanceToCam));
        //transform.position = new Vector3(normalizedCursorPosX, normalizedCursorPosY,0);
        Debug.Log($"x lerped val : {normalizedCursorPosX} | func val : {cam.ScreenToWorldPoint(new Vector3(normalizedCursorPosX, normalizedCursorPosY, distanceToCam))}");

        //If selected, animation
        //anim.SetTrigger("Select");

    }
}
