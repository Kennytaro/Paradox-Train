using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class Enemy : Entity {
  // -1: Player is to the enemy's left
  // 0: Player is out of tracking range
  // 1: Player is to the enemy's right
  public float playerDirection = -1;

  public float damage = 5f;
  
  public GameObject target;
  public float nextWaypointDistance = 3f;
  
  public Path path;
  public int currentWaypointIndex = 0;
  public bool reachedEndOfPath = false;
  public float distanceFromPlayerToTrack = 30;
  public float distanceFromReachedGoal = 1;
  
  // public bool isAttacking = false;

  public Seeker seeker;

  public override void Start() {
    base.Start();

    target = FindFirstObjectByType<PlayerController>().gameObject;
    seeker = GetComponent<Seeker>();
    speed = 10f;
  }

  public void OnPathComplete(Path p) {
    if (!p.error) {
      path = p;
      currentWaypointIndex = 0;
    }
  }
  
  void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
      // Attack player
      StartCoroutine(target.GetComponent<PlayerController>().Hurt(damage, transform.position));
    }
  }

  public float DistanceFromPlayer() {
    Vector2 floorBelowPlayer = target.GetComponent<PlayerController>().groundPos;
    float distanceFromPlayer = Vector2.Distance(floorBelowPlayer, transform.position);

    return distanceFromPlayer;
  }
}