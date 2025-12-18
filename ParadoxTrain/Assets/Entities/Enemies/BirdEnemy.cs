using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class BirdEnemy : Enemy {  
  [Header("Flight Settings")]
  public float normalFlightSpeed = 12f;
  public float approachSpeed = 15f;
  public float maxFlightSpeed = 20f;
  public float flightSmoothTime = 0.3f;
  public float hoverHeight = 5f;
  public float hoverDistance = 7f;
  public float bobbingAmplitude = 0.3f;
  public float bobbingFrequency = 2f;
  public float waypointReachedDistance = 0.5f;
  
  [Header("Deceleration Settings")]
  public float slowdownStartDistance = 5f;
  public float slowdownMinDistance = 1f;
  public float slowdownMinimumSpeed = 2f;
  public float decelerationCurve = 2f;
  
  [Header("Hover Settings")]
  public float hoverForce = 10f; // Added: Force to counteract gravity when hovering
  public float hoverStabilizationDistance = 2f; // Added: Distance to start stabilizing height
  
  [Header("Pathfinding Settings")]
  public float pathUpdateInterval = 0.3f;
  public float maxPathDistance = 30f;
  public float pathHeightBuffer = 2f;
  public float pathEndReachedDistance = 2f;
  
  private float hoverTimer = 0f;
  private Vector2 flightSmoothVelocity;
  private Vector2 targetHoverPosition;
  private float lastPathUpdateTime;
  private Vector2 lastPlayerPosition;
  private bool hasValidPath = false;
  private int currentPathIndex = 0;
  private bool hasReachedPathEnd = false;
  private bool isInHoverMode = false; // Added: Track if we're in hover mode
  
  public override void Start() {
    base.Start();
    speed = normalFlightSpeed;
    seeker = GetComponent<Seeker>();
    
    // Initialize pathfinding
    UpdateHoverTarget();
    lastPathUpdateTime = Time.time;
    
    // Start periodic path updates
    InvokeRepeating("UpdateFlightPath", 0f, pathUpdateInterval);
  }
  
  void Update() {
    if (target == null) return;
    
    // Update facing direction
    playerDirection = target.transform.position.x >= transform.position.x ? 1 : -1;
    facingDirection = playerDirection;
    
    // Update hover target periodically
    hoverTimer += Time.deltaTime;
    if (hoverTimer >= 0.1f) {
      UpdateHoverTarget();
      hoverTimer = 0f;
    }
  }
  
  void UpdateHoverTarget() {
    if (target == null) return;
    
    Vector2 playerPos = target.transform.position;
    Vector2 groundPos = target.GetComponent<PlayerController>().groundPos;
    
    // Calculate ideal hover position
    float randomOffset = Random.Range(-0.5f, 0.5f);
    targetHoverPosition = new Vector2(
      playerPos.x + (hoverDistance * -playerDirection) + randomOffset,
      groundPos.y + hoverHeight
    );

    Vector3 finalWaypoint = path.vectorPath[path.vectorPath.Count - 1];
    float distance = Vector2.Distance(transform.position, finalWaypoint);
    Debug.Log(distance);
    
    // Check if we need a new path
    float playerMoved = Vector2.Distance(playerPos, lastPlayerPosition);
    if (playerMoved > 1.5f || !hasValidPath) {
      UpdateFlightPath();
      lastPlayerPosition = playerPos;
    }
  }
  
  void UpdateFlightPath() {
    if (target == null || seeker == null) return;
    
    // Only request new path if not already at the end of current path
    if (hasReachedPathEnd && Vector2.Distance(transform.position, targetHoverPosition) < 3f) {
      return; // We're already where we want to be
    }
    
    // Calculate the actual target position for pathfinding
    Vector2 pathfindingTarget = targetHoverPosition;
    
    // Add some height buffer to fly over obstacles
    pathfindingTarget.y += pathHeightBuffer;
    
    // Limit pathfinding distance
    float distanceToTarget = Vector2.Distance(transform.position, pathfindingTarget);
    if (distanceToTarget > maxPathDistance) {
      // Too far, move towards player in smaller steps
      Vector2 direction = (pathfindingTarget - (Vector2)transform.position).normalized;
      pathfindingTarget = (Vector2)transform.position + direction * maxPathDistance;
    }
    
    // Request a path
    if (seeker.IsDone()) {
      seeker.StartPath(transform.position, pathfindingTarget, new OnPathDelegate(OnPathComplete));
    }
  }
  
  private new void OnPathComplete(Path p) {
    if (p.error) {
      Debug.LogWarning("Pathfinding error: " + p.errorLog);
      hasValidPath = false;
      hasReachedPathEnd = false;
      return;
    }
    
    // Store the path
    path = p;
    currentPathIndex = 0;
    hasValidPath = true;
    hasReachedPathEnd = false;
  }
  
  public override void FixedUpdate() {
    if (target == null) {
      base.FixedUpdate();
      return;
    }
    
    Fly();
    
    // Apply reduced gravity for flight
    if (!isInHoverMode) {
      // Only apply gravity when not in hover mode
      velocity.y += gravity * 0.3f * Time.fixedDeltaTime;
    } else {
      // In hover mode, counteract gravity with hover force
      velocity.y += (hoverForce + gravity * 0.3f) * Time.fixedDeltaTime;
      
      // Additional stabilization to maintain exact hover height
      Vector2 groundPos = target.GetComponent<PlayerController>().groundPos;
      float desiredHeight = groundPos.y + hoverHeight;
      float currentHeight = transform.position.y;
      float heightError = desiredHeight - currentHeight;
      
      if (Mathf.Abs(heightError) > 0.1f) {
        velocity.y += heightError * 2f * Time.fixedDeltaTime;
      }
    }
    
    // Apply movement
    // controller.Move(velocity * Time.fixedDeltaTime);
  }
  
  void Fly() {
    if (target == null) {
      // Hover in place if no target
      velocity = Vector2.Lerp(velocity, Vector2.zero, 5f * Time.fixedDeltaTime);
      isInHoverMode = true;
      return;
    }
    
    // Add bobbing motion
    float bobbingOffset = Mathf.Sin(Time.time * bobbingFrequency) * bobbingAmplitude;
    
    // Follow A* path if available and haven't reached the end
    if (hasValidPath && !hasReachedPathEnd && path != null && path.vectorPath != null && path.vectorPath.Count > 0) {
      FollowPath(bobbingOffset);
    } else {
      // Fallback: Direct flight towards target
      DirectFlight(bobbingOffset);
    }
  }
  
  void FollowPath(float bobbingOffset) {
    float distance = Vector2.Distance(transform.position, path.vectorPath[path.vectorPath.Count - 1]);

    // Check if we've reached the end of the path
    if (distance <= pathEndReachedDistance) {
      hasReachedPathEnd = true;
      hasValidPath = false;
      isInHoverMode = true;
      return;
    }
    
    // Get current waypoint
    Vector3 currentWaypoint = path.vectorPath[currentPathIndex];
    
    // Adjust waypoint height for bobbing
    currentWaypoint.y += bobbingOffset;
    
    // Calculate direction to waypoint
    Vector2 toWaypoint = currentWaypoint - transform.position;
    float distanceToWaypoint = toWaypoint.magnitude;
    
    // Get the final waypoint in the path
    Vector3 finalWaypoint = path.vectorPath[path.vectorPath.Count - 1];
    float distanceToFinalWaypoint = Vector2.Distance(transform.position, finalWaypoint);
    
    // Check if we should enter hover mode
    isInHoverMode = distanceToFinalWaypoint <= hoverStabilizationDistance;
    
    // Check if we've reached the end of the path
    if (distanceToFinalWaypoint <= pathEndReachedDistance) {
      hasReachedPathEnd = true;
      hasValidPath = false;
      isInHoverMode = true;
      // Slow down to a hover
      velocity = Vector2.Lerp(velocity, Vector2.zero, 8f * Time.fixedDeltaTime);
      return;
    }
    
    // Calculate slowdown factor based on distance to final waypoint
    float slowdownFactor = CalculateSlowdownFactor(distanceToFinalWaypoint);
    
    // Calculate base speed
    float currentMaxSpeed = normalFlightSpeed * slowdownFactor;
    
    // Move to next waypoint if close enough
    if (distanceToWaypoint < waypointReachedDistance) {
      currentPathIndex++;
      
      // Check if we've reached the end of the path
      if (currentPathIndex >= path.vectorPath.Count) {
        hasReachedPathEnd = true;
        hasValidPath = false;
        isInHoverMode = true;
        // Hover in place at the target
        velocity = Vector2.Lerp(velocity, Vector2.zero, 8f * Time.fixedDeltaTime);
        return;
      }
      
      // Get new waypoint
      currentWaypoint = path.vectorPath[currentPathIndex];
      currentWaypoint.y += bobbingOffset;
      toWaypoint = currentWaypoint - transform.position;
      distanceToWaypoint = toWaypoint.magnitude;
    }
    
    // Calculate desired velocity
    Vector2 desiredVelocity;
    if (distanceToWaypoint > 0.1f) {
      // Normalize direction
      desiredVelocity = toWaypoint.normalized;
      
      // Adjust speed based on distance to waypoint
      float waypointSpeedFactor = Mathf.Clamp01(distanceToWaypoint / 2f);
      desiredVelocity *= currentMaxSpeed * waypointSpeedFactor;
      
      // Even slower if very close to final waypoint
      if (distanceToFinalWaypoint < slowdownMinDistance) {
        desiredVelocity *= Mathf.Clamp01(distanceToFinalWaypoint / slowdownMinDistance);
      }
      
      // Add slight lift when flying to counteract gravity
      if (!isInHoverMode && desiredVelocity.y < 0) {
        desiredVelocity.y += Mathf.Abs(gravity) * 0.2f * Time.fixedDeltaTime;
      }
    } else {
      desiredVelocity = Vector2.zero;
    }
    
    // Add slight perpendicular motion for bird-like flight
    if (slowdownFactor > 0.3f && !isInHoverMode) {
      Vector2 perpendicular = Vector2.Perpendicular(desiredVelocity.normalized) * 0.2f;
      float wiggle = Mathf.Sin(Time.time * 4f) * 0.1f;
      desiredVelocity += perpendicular * wiggle * slowdownFactor;
    }
    
    // Add gentle wing flaps
    float wingFlap = Mathf.Sin(Time.time * 3f) * 0.15f;
    desiredVelocity.y += wingFlap * Mathf.Clamp01(slowdownFactor * 2f) * (isInHoverMode ? 0.5f : 1f);
    
    // Apply air resistance
    float airResistance = Mathf.Lerp(0.85f, 0.98f, slowdownFactor);
    
    // Smoothly interpolate to desired velocity
    float currentSmoothTime = Mathf.Lerp(flightSmoothTime * 0.5f, flightSmoothTime * 2f, 1f - slowdownFactor);
    velocity = Vector2.SmoothDamp(
      velocity,
      desiredVelocity,
      ref flightSmoothVelocity,
      currentSmoothTime,
      currentMaxSpeed,
      Time.fixedDeltaTime
    );
    
    // Apply air resistance
    velocity *= airResistance;
  }
  
  void DirectFlight(float bobbingOffset) {
    // Calculate direction to hover target (with bobbing)
    Vector2 bobbingTarget = targetHoverPosition;
    bobbingTarget.y += bobbingOffset;
    
    Vector2 toTarget = bobbingTarget - (Vector2)transform.position;
    float distanceToTarget = toTarget.magnitude;
    
    // Check if we should enter hover mode
    isInHoverMode = distanceToTarget <= hoverStabilizationDistance;
    
    // If we're very close to our target, just hover
    if (distanceToTarget <= pathEndReachedDistance) {
      hasReachedPathEnd = true;
      isInHoverMode = true;
      velocity = Vector2.Lerp(velocity, Vector2.zero, 5f * Time.fixedDeltaTime);
      
      // Request a new path only if we've drifted too far
      if (distanceToTarget > 3f && Time.time - lastPathUpdateTime > pathUpdateInterval) {
        UpdateFlightPath();
        lastPathUpdateTime = Time.time;
      }
      return;
    }
    
    // Calculate slowdown factor
    float slowdownFactor = CalculateSlowdownFactor(distanceToTarget);
    
    // Calculate base speed
    float currentMaxSpeed = normalFlightSpeed * slowdownFactor;
    
    // Calculate desired velocity
    Vector2 desiredVelocity;
    if (distanceToTarget > 0.5f) {
      desiredVelocity = toTarget.normalized * currentMaxSpeed;
      
      // Even slower if very close
      if (distanceToTarget < slowdownMinDistance) {
        desiredVelocity *= Mathf.Clamp01(distanceToTarget / slowdownMinDistance);
      }
      
      // Add slight lift when flying to counteract gravity
      if (!isInHoverMode && desiredVelocity.y < 0) {
        desiredVelocity.y += Mathf.Abs(gravity) * 0.2f * Time.fixedDeltaTime;
      }
    } else {
      // Hover in place when close
      desiredVelocity = Vector2.zero;
    }
    
    // Add bird-like motion
    if (slowdownFactor > 0.3f && !isInHoverMode) {
      float wiggle = Mathf.Sin(Time.time * 5f) * 0.15f;
      desiredVelocity.x += wiggle * slowdownFactor;
    }
    
    // Add wing flaps
    float wingFlap = Mathf.Sin(Time.time * 3.5f) * 0.2f;
    desiredVelocity.y += wingFlap * Mathf.Clamp01(slowdownFactor * 2f) * (isInHoverMode ? 0.5f : 1f);
    
    // Apply air resistance
    float airResistance = Mathf.Lerp(0.8f, 0.97f, slowdownFactor);
    
    // Smooth movement
    float currentSmoothTime = Mathf.Lerp(flightSmoothTime * 0.5f, flightSmoothTime * 2f, 1f - slowdownFactor);
    velocity = Vector2.SmoothDamp(
      velocity,
      desiredVelocity,
      ref flightSmoothVelocity,
      currentSmoothTime,
      currentMaxSpeed,
      Time.fixedDeltaTime
    );
    
    // Apply air resistance
    velocity *= airResistance;
    
    // Request a new path if we're far from target
    if (distanceToTarget > 5f && Time.time - lastPathUpdateTime > pathUpdateInterval) {
      UpdateFlightPath();
      lastPathUpdateTime = Time.time;
    }
  }
  
  float CalculateSlowdownFactor(float distanceToTarget) {
    if (distanceToTarget <= slowdownMinDistance) {
      // Very close - minimum speed
      return slowdownMinimumSpeed / normalFlightSpeed;
    }
    else if (distanceToTarget <= slowdownStartDistance) {
      // Within slowdown range - calculate smooth slowdown
      float normalizedDistance = (distanceToTarget - slowdownMinDistance) / 
                                 (slowdownStartDistance - slowdownMinDistance);
      
      // Use exponential easing for smoother slowdown
      float easedFactor = Mathf.Pow(normalizedDistance, decelerationCurve);
      
      // Map from minimum speed to normal speed
      return Mathf.Lerp(slowdownMinimumSpeed, normalFlightSpeed, easedFactor) / normalFlightSpeed;
    }
    else {
      // Far away - full speed
      return 1f;
    }
  }
}