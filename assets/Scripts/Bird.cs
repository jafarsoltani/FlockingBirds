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


    void Start()
    {
        velocity = transform.forward * speed;
    }

    void Update()
    {
        neighbours = GetNearbyBirds();
        var flockingForce = ComputeCollidingForce();
        var avoidanceForce = ComputeObstacleAvoidanceForce();
        var finalForce = flockingForce + (avoidanceForce * obstacleAvoidanceWeight);

        Move(finalForce);

        lastAvoidanceForce = avoidanceForce;

    }

    private void Move(Vector3 flockingForce)
    {
        // Use lerp to avoid sudden jumps
        velocity = Vector3.Lerp(velocity, velocity + flockingForce, Time.deltaTime * smoothingFactor);
        velocity = Vector3.ClampMagnitude(velocity, maxSteerForce);
        transform.position += velocity * Time.deltaTime;
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
            Console.WriteLine("No neighbours");
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleDetectionRange, obstacleLayer))
        {
            avoidanceForce = Vector3.Reflect(transform.forward, hit.normal);

            // Strengthen the avoidance based on distance (stronger when closer)
            float strength = 1.0f - (hit.distance / obstacleDetectionRange);
            avoidanceForce *= strength * obstacleAvoidanceWeight;
        }

        return avoidanceForce.normalized;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleDetectionRange);

        if (lastAvoidanceForce != Vector3.zero)
        {
            Console.WriteLine("Drawing avoidance force");
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, lastAvoidanceForce * obstacleDetectionRange);
        }
    }    

    void OnCollisionEnter(Collision collision)
    {
        Console.WriteLine("Collision detected");
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0) // Check if it's an obstacle
        {
            Console.WriteLine("Obstacle collision detected");
            // Push bird slightly away from obstacle
            transform.position += collision.contacts[0].normal * 0.5f;

            // Reverse direction to prevent further penetration
            velocity = -velocity;
        }
    }


}
