using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A quick and dirty algorithm allowing to spawn the stars in the sky
/// </summary>
public class StarGenerator : MonoBehaviour
{
    [SerializeField] Star starPrefab;
    [MinMaxSlider(-100f, 100f), SerializeField] Vector2 minMaxXStarCoord;
    [MinMaxSlider(-100f, 100f), SerializeField] Vector2 minMaxYStarCoord;
    [MinMaxSlider(0f, 2f), SerializeField] Vector2 minMaxScale;
    [SerializeField] float starsAmount = 100;
    [SerializeField] float minStarDistance = 1f;
    [SerializeField] int maxTries = 1000;
    [SerializeField] bool spawnStarsOnStart = true;

    [SerializeField] Transform starsParent;

    List<Star> stars = new List<Star>();

    private void Start()
    {
        if (spawnStarsOnStart)
            SpawnStars();
    }

    [Button]
    void SpawnStars()
    {
        Clear();

        bool isPosFree;
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < starsAmount; i++)
        {
            for (int tryAmount = 0; tryAmount < maxTries; tryAmount++)
            {
                Vector3 pos = new Vector3(Random.Range(minMaxXStarCoord.x, minMaxXStarCoord.y), Random.Range(minMaxYStarCoord.x, minMaxYStarCoord.y),0);
                isPosFree = true;

                foreach (var item in stars)
                {
                    if (Vector3.Distance(pos, item.transform.position) < minStarDistance)
                    {
                        isPosFree = false;
                        break;
                    }
                }

                if (isPosFree)
                {
                    Star starInstance = Instantiate(starPrefab, pos, Quaternion.identity,starsParent);
                    float randomScale = Random.Range(minMaxScale.x, minMaxScale.y);
                    starInstance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                    stars.Add(starInstance);
                    break;
                }
            }
        }

        Debug.Log($"Sky generated in <color=red>{Time.realtimeSinceStartup - startTime}</color> seconds");
    }

    [Button]
    void Clear()
    {
#if UNITY_EDITOR
        if (stars.Count <= 0)
        {
            int children = starsParent.childCount;
            for (int i = 0; i < children; ++i)
                stars.Add(starsParent.GetChild(i).GetComponent<Star>());
        }
#endif
        for (int i = 0; i < stars.Count; i++)
        {
#if UNITY_EDITOR
            DestroyImmediate(stars[i].gameObject); // Destroy each element in the list
#else
            Destroy(stars[i].gameObject); // Destroy each element in the list
#endif
        }

        stars.Clear(); // Clear the list after destroying all elements
    }
}
