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
        transform.position = CursorManager.Instance.GetCursorWorldPosition();
        //If selected, animation
        //anim.SetTrigger("Select");

    }
}
