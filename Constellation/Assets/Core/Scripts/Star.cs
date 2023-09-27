using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField]List<GameObject>starGFX;


        private void Start() {
            Instantiate(starGFX[Random.Range(0, starGFX.Count)], transform);
        }
}
