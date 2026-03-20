using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using Unity.AI.Navigation;

/// <summary>
/// This is the main agent script for the Turtle.
/// It learns how to reach the goal while avoiding walls.
/// It gives visual feedback by flashing the ground:
///     - Green if it reached the goal
///     - Red if it failed
/// </summary>
public class TurtleAgent : Agent
{

    LayerMask layerMask;
    RaycastHit hit;
    // The goal object that the turtle is trying to reach
    [SerializeField] private Transform[] _goals;


    // The ground object — we use its material to flash red or green

    // Movement and rotation speeds for the turtle
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _rotationSpeed = 300f;

    // Reference to the turtle's Renderer — we use it to change color on wall collision
    private Renderer _renderer;

    // Ground's original color (so we can fade back to it after flashing red or green)
    private Color _defaultGroundColor;

    // Tracks whether the agent reached the goal in the last episode
    private bool _reachedGoalLastEpisode = false;

    // Handle to the coroutine that flashes the ground — we store it so we can stop it early
    private Coroutine _flashGroundCoroutine;

    // Tracks how many episodes have been completed (useful for debugging GUI)
    [HideInInspector] public int _currentEposide = 0;

    // Optional: Shows current reward value (useful for GUI debug info)
    [HideInInspector] public float _cumulativeReward = 0f;
    private int choice;
    /// <summary>
    /// Called once when the agent is first initialized
    /// </summary>
    public override void Initialize()
    {

        layerMask = LayerMask.GetMask("notwalka");


        _currentEposide = 0;
        _cumulativeReward = 0f;
    }

    /// <summary>
    /// Called automatically at the start of each new episode
    /// </summary>
    public override void OnEpisodeBegin()
    {


        // Flash ground based on whether the goal was reached last episode

        // Reset this flag for the next episode
        _reachedGoalLastEpisode = false;

        // Reset counters
        _currentEposide++;
        _cumulativeReward = 0f;

        // Reset the turtle's visual color

        SpawnObjects();
        // Randomize the goal and turtle positions



    }
    /// <summary>
    /// Coroutine to flash the ground a color and fade it back to original
    /// </summary>

    /// <summary>
    /// Spawns the turtle and goal at new random positions
    /// </summary>
    private void SpawnObjects()
    {

        // Reset the turtle's position and rotation
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        // Random direction and distance for the goal
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
        float randomDistance = Random.Range(1f, 2.5f);
        Vector3 goalPosition = transform.position + randomDirection * randomDistance;
        UnityEngine.AI.NavMeshHit hit;
        bool isOnNavMesh = UnityEngine.AI.NavMesh.SamplePosition(new Vector3(goalPosition.x, 0.3f, goalPosition.z), out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas);
        // Set goal's new position
        _goals[0].transform.position = hit.position;





    }

    /// <summary>
    /// Collects information (observations) that the neural network uses to learn
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalize all positions to stay between -1 and 1 for better learning

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 30f, layerMask);
        float goalX = _goals[0].localPosition.x / 5f;
        float goalZ = _goals[0].localPosition.z / 5f;

        float agentX = transform.localPosition.x / 5f;
        float agentZ = transform.localPosition.z / 5f;

        float agentRotY = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        sensor.AddObservation(hit.distance);
        sensor.AddObservation(goalX);
        sensor.AddObservation(goalZ);
        sensor.AddObservation(agentX);
        sensor.AddObservation(agentZ);
        sensor.AddObservation(agentRotY);
    }

    /// <summary>
    /// Allows keyboard control for testing the agent manually
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

#if ENABLE_LEGACY_INPUT_MANAGER
    if (Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 1;
    else if (Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 2;
    else if (Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 3;
#endif
    }

    /// <summary>
    /// Called every time the agent takes an action
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        if (hit.distance < 5f)
        {
            AddReward(-0.001f);
        }

        AddReward(+1f / 20000f);

        // Optional: Track cumulative reward for debugging
        _cumulativeReward = GetCumulativeReward();
    }

    /// <summary>
    /// Applies movement or rotation based on neural network's chosen action
    /// </summary>
    private void MoveAgent(ActionSegment<int> act)
    {
        int action = act[0];

        switch (action)
        {
            case 1:
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2:
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3:
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    /// <summary>
    /// Called when the agent touches something with a trigger (like the goal)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {

            GoalReached();
            foreach (GameObject zombie in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Destroy(zombie);
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Small penalty for hitting walls
            AddReward(-0.001f);

            // Turn turtle red as visual feedback
            if (_renderer != null)
            {
                _renderer.material.color = Color.red;
            }
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Continuous tiny penalty while touching wall
            AddReward(-0.0001f * Time.fixedDeltaTime);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Reset color to blue
            if (_renderer != null)
            {
                _renderer.material.color = Color.blue;
            }
        }
    }

    /// <summary>
    /// Handles the logic when the goal is reached
    /// </summary>
    private void GoalReached()
    {

        AddReward(-0.5f);

        // Mark that this episode was successful
        _reachedGoalLastEpisode = true;

        // End the current episode
        EndEpisode();
    }

    /// <summary>
    /// Called when the agent bumps into a wall
    /// </summary>

    /// <summary>
    /// Called while the agent is still in contact with a wall
    /// </summary>

    /// <summary>
    /// Called when the agent stops touching a wall
    /// </summary>
}
