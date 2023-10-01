using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    [SerializeField] private GameObject _agentGhost;
    private NavMeshAgent _agent;

    public float MaxEnergy { get; private set; }
    public float MovementEnergy { get; private set; }

    private bool _canMove => !_isMoving && MovementEnergy >= 0.05f;
    private bool _isMoving;


    public float PredictedEnergy => _predictedEnergy;
    private float _predictedEnergy;

    private void Start()
    {
        PlayerEvents.Instance.OnEnergyReset += OnEnergyReset;
        InitializeCharacter();

    }

    private void InitializeCharacter()
    {
        _agent = GetComponent<NavMeshAgent>();
        MaxEnergy = 6;
        MovementEnergy = MaxEnergy;    //For debugging purposes 
    }


    void Update()
    {
        if (_canMove)
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
        {
            PredictPath(hit.point, out Vector3 reachablePosition);
            if (Input.GetMouseButtonDown(0))
            {
                ClickToMove(reachablePosition);
            }
        }
        else
        {
            _predictedEnergy = MovementEnergy;
        }
    }


    private void ClickToMove(Vector3 hitPoint)
    {
        MovementEnergy = _predictedEnergy;
        _isMoving = true;
        _agent.SetDestination(hitPoint);
        MovementAsync();

    }


    private void PredictPath(Vector3 hitPoint, out Vector3 reachablePosition)
    {
        
        _agentGhost.SetActive(true);
        _agentGhost.transform.SetParent(null);
        
        reachablePosition = Vector3.zero;
        float energy = MovementEnergy;
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, hitPoint, NavMesh.AllAreas, path))
        {
            LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                ConfigureLineRenderer(lineRenderer);
            }
            else
            {
                lineRenderer.enabled = true;
            }

            List<Vector3> pathWithEnergy = GetPathWithEnergy(path, ref energy);
            lineRenderer.positionCount = pathWithEnergy.Count;
            lineRenderer.SetPositions(pathWithEnergy.ToArray());
            if (pathWithEnergy.Count > 0)
            {
                Vector3 targetPoint = pathWithEnergy[pathWithEnergy.Count - 1];
                _agentGhost.transform.position = targetPoint;
                reachablePosition = targetPoint;
            }
        }
    }

    private List<Vector3> GetPathWithEnergy(NavMeshPath path, ref float energy)
    {
        _predictedEnergy = energy;
        List<Vector3> finalPath = new List<Vector3>();
        finalPath.Add(path.corners[0]);

        float totalDistance = 0f;
        float segmentLength;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            segmentLength = Vector3.Distance(path.corners[i], path.corners[i + 1]);
            if (totalDistance + segmentLength > energy)
            {
                // Calculate the ratio of the remaining energy to the segment length
                float ratio = (energy - totalDistance) / segmentLength;
                Vector3 interpolatedPoint = Vector3.Lerp(path.corners[i], path.corners[i + 1], ratio);
                finalPath.Add(interpolatedPoint);
                float lastLength = Vector3.Distance(path.corners[i], interpolatedPoint);
                _predictedEnergy -= lastLength;
                break;
            }
            else
            {
                totalDistance += segmentLength;
                _predictedEnergy -= segmentLength;
                finalPath.Add(path.corners[i + 1]);
            }
        }

        return finalPath;
    }
    

    private void ResetPredictionVisuals()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        _agentGhost.SetActive(false);
        _agentGhost.transform.SetParent(_agent.transform);
    }

    void ConfigureLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.material.color = Color.red;
    }
    
    private async void MovementAsync()
    {
        while (_agent.isStopped)    // The navmesh agent waits a frame to start moving
        {
            await Task.Yield();
        }

        while ((_agent.pathPending || _agent.hasPath) && !_canMove)
        {
            await Task.Yield();
        }

        _isMoving = false;
        ResetPredictionVisuals();
    }


    public void OnEnergyReset()
    {
        MovementEnergy = MaxEnergy;
    }

}
