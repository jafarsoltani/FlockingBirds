using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public GameObject birdPrefab;
    public int flockSize = 20;
    public Vector3 spawnArea = new Vector3(10, 10, 10);
    public List<Bird> birds;

    public float globalSpeed = 5f;
    public float globalAlignmentWeight = 1.5f;
    public float globalCohesionWeight = 1.0f;
    public float globalSeparationWeight = 2.0f;
    public float globalMaxSteerForce = 2f;
    public float globalSmoothingFactor = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        birds = new List<Bird>();
        SpawnFlock();
    }

    private void SpawnFlock()
    {
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(-spawnArea.y / 2, spawnArea.y / 2),
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            GameObject birdObj = Instantiate(birdPrefab, randomPos, Quaternion.identity);
            Bird bird = birdObj.GetComponent<Bird>();
            birds.Add(bird);
            AssignGlobalSettings(bird);
        }
    }

    private void AssignGlobalSettings(Bird bird)
    {
        bird.speed = globalSpeed;
        bird.alignmentWeight = globalAlignmentWeight;
        bird.cohesionWeight = globalCohesionWeight;
        bird.separationWeight = globalSeparationWeight;
        bird.maxSteerForce = globalMaxSteerForce;
        bird.smoothingFactor = globalSmoothingFactor;
    }
}
