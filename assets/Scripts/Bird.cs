using System;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed = 5f;
    public float neighborRadius = 3f;
    public float avoidanceRadius = 1.5f;
    public float maxSteerForce = 2f;
    public float alignmentWeight = 1.5f;
    public float cohesionWeight = 1.0f;
    public float separationWeight = 2.0f;
    public float obstacleAvoidanceWeight = 3.0f;
    public float smoothingFactor = 5f;
    public float obstacleDetectionRange = 5f;
    public LayerMask obstacleLayer;

    
    private Vector3 velocity;
    private List<Bird> neighbours;
    private Vector3 lastAvoidanceForce = Vector3.zero;
    private int birdID;

    void Start()
    {
        velocity = transform.forward * speed;
        birdID = GetInstanceID();
        Debug.Log($"Velocity: {velocity}");
    }

    void Update()
    {
        neighbours = GetNearbyBirds();
        var flockingForce = ComputeCollidingForce();
        var avoidanceForce = ComputeObstacleAvoidanceForce();
        //var finalForce = flockingForce + (avoidanceForce * obstacleAvoidanceWeight);
        Vector3 finalForce;
        if(avoidanceForce != Vector3.zero)
        {
            finalForce = avoidanceForce * obstacleAvoidanceWeight;
        }
        else
        {
            finalForce = flockingForce;
        }

        // Ensure final force is not zero
        if (finalForce == Vector3.zero)
        {
            finalForce = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized * 0.1f;
        }

        Debug.Log($"{birdID} - flocking:{flockingForce} avoiding:{avoidanceForce} Final force:{finalForce}");
        Move(finalForce);

        lastAvoidanceForce = avoidanceForce;

    }

    private void Move(Vector3 flockingForce)
    {
        // Use lerp to avoid sudden jumps
        velocity = Vector3.Lerp(velocity, velocity + flockingForce, Time.deltaTime * smoothingFactor);
        velocity = Vector3.ClampMagnitude(velocity, maxSteerForce);

        // Ensure the velocity is zero, so the birds don't move along the y axis
        velocity.y = 0;

        transform.position += velocity * Time.deltaTime;
        // Ensure the position's y remains at 1
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        
        transform.forward = velocity.normalized;
    }

    private List<Bird> GetNearbyBirds()
    {
        List<Bird> nearbyBirds = new List<Bird>();
        Collider[] neighboursColliders = Physics.OverlapSphere(transform.position, neighborRadius);
        foreach (Collider col in neighboursColliders)
        {
            Bird bird = col.GetComponent<Bird>();
            if (bird != null && bird != this)
            {
                nearbyBirds.Add(bird);
            }
        }

        return nearbyBirds;
    }

    private Vector3 ComputeCollidingForce()
    {
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;  
        int count = neighbours.Count;

        if (count == 0)
        {
            Debug.Log($"{birdID} - No neighbours");
            return Vector3.zero;
        }

        foreach (Bird neighbour in neighbours)
        {
            alignment += neighbour.velocity;
            cohesion += neighbour.transform.position;
            var diff =  transform.position - neighbour.transform.position;
            // if the birds are too close, move away by normalising and dividing by distance
            if (diff.magnitude < avoidanceRadius)
            {
                separation += diff.normalized / diff.magnitude;
            }
        }
        
        alignment = (alignment / count).normalized * alignmentWeight;
        cohesion = ((cohesion / count) - transform.position).normalized * cohesionWeight;
        separation = (separation / count).normalized * separationWeight;

        Vector3 flockingForce = alignment + cohesion + separation;
        
        return Vector3.ClampMagnitude(flockingForce, maxSteerForce);
        
    }

    private Vector3 ComputeObstacleAvoidanceForce()
    {
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 initialAvoidanceForce = Vector3.zero;
        
        RaycastHit hit;
        Debug.Log($"{birdID} - transform position {transform.position} forward {transform.forward} Obstacle detection range: {obstacleDetectionRange} Obstacle Layer: {obstacleLayer.value}");
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleDetectionRange, obstacleLayer))
        {
            initialAvoidanceForce = Vector3.Reflect(transform.forward, hit.normal);
            // Strengthen the avoidance based on distance (stronger when closer)
            float strength = 1.0f - (hit.distance / obstacleDetectionRange);
            avoidanceForce = initialAvoidanceForce * strength * obstacleAvoidanceWeight;
            Debug.Log($"{birdID} - Obstacle detected at distance {hit.distance}, initial force: {initialAvoidanceForce}, adjusted force: {avoidanceForce}");
        }
        else
        {
            Debug.Log($"{birdID} - No obstacle detected");
        }

        return avoidanceForce.normalized;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleDetectionRange);

        if (lastAvoidanceForce != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, lastAvoidanceForce * obstacleDetectionRange);
        }

        // Draw birdID near the bird's position
        Gizmos.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"ID: {birdID}");
    }    

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected");
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0) // Check if it's an obstacle
        {
            Debug.Log("Collision with obstacle detected");
            // Push bird slightly away from obstacle
            transform.position += collision.contacts[0].normal * 0.5f;

            // Reverse direction to prevent further penetration
            velocity = -velocity;
        }
    }
    void OnGUI()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.z > 0) // Only display if the bird is in front of the camera
        {
            GUI.Label(new Rect(screenPosition.x, Screen.height - screenPosition.y, 100, 20), $"ID: {birdID}");
        }
    }
    


}
