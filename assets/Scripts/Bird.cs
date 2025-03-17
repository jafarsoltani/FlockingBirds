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
    public float smoothingFactor = 5f;
    
    private Vector3 velocity;
    private List<Bird> neighbours;

    void Start()
    {
        velocity = transform.forward * speed;
    }

    void Update()
    {
        neighbours = GetNearbyBirds();
        var flockingForce = ComputeCollidingForce();
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
}
