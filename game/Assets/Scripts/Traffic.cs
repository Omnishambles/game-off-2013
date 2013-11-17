 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum TrafficLane
{
    North = 0,
    South = 1,
    East  = 2,
    West  = 3,
    None  = 4
}

public class Traffic : MonoBehaviour
{
    private System.Random _random;
    private float _lastSpawnTime;
    private Dictionary<TrafficLane, Car> _lastCars;
    private Dictionary<TrafficLane, iTweenPath[]> _lanePaths;
    private Dictionary<TrafficLane, GameObject> _laneBlocks;
    private TrafficLane _openOriginLane;
    private TrafficLane _openDestinationLane;
    private Dictionary<TrafficLane, List<Car>> _carsInLane;

    private void Start()
    {
        _openOriginLane = TrafficLane.None;
        _openDestinationLane = TrafficLane.None;
        _lastCars = new Dictionary<TrafficLane, Car>();
        _lanePaths = new Dictionary<TrafficLane, iTweenPath[]>();
        _laneBlocks = new Dictionary<TrafficLane, GameObject>();
        _carsInLane = new Dictionary<TrafficLane, List<Car>>();
        _carsInLane[TrafficLane.North] = new List<Car>();
        _carsInLane[TrafficLane.South] = new List<Car>();
        _carsInLane[TrafficLane.East] = new List<Car>();
        _carsInLane[TrafficLane.West] = new List<Car>();
        _random = new System.Random();

        // Pull out iTweenPath arrays from their container objects.
        GameObject northPaths = GameObject.Find("Paths: North");
        _lanePaths[TrafficLane.North] = northPaths.GetComponents<iTweenPath>();
        GameObject southPaths = GameObject.Find("Paths: South");
        _lanePaths[TrafficLane.South] = southPaths.GetComponents<iTweenPath>();
        GameObject eastPaths = GameObject.Find("Paths: East");
        _lanePaths[TrafficLane.East] = eastPaths.GetComponents<iTweenPath>();
        GameObject westPaths = GameObject.Find("Paths: West");
        _lanePaths[TrafficLane.West] = westPaths.GetComponents<iTweenPath>();

        // Pull out lane block trigger objects.
        _laneBlocks[TrafficLane.North] = GameObject.Find("NorthBlock");
        _laneBlocks[TrafficLane.South] = GameObject.Find("SouthBlock");
        _laneBlocks[TrafficLane.East] = GameObject.Find("EastBlock");
        _laneBlocks[TrafficLane.West] = GameObject.Find("WestBlock");
    }

    private void Update()
    {
        // Only spawn and move a new car once every random interval,
        // between 0.5 and 2.0 seconds.
        double dtAmount = (_random.NextDouble() * 2.0f) + 0.5f;
        float sinceLastSpawn = Time.time - _lastSpawnTime;
        if (sinceLastSpawn < dtAmount)
        {
            return;
        }

        // Get a random path that this car will take.
        TrafficLane lane = RandomLane();
        Vector3[] path = RandomPathInLane(lane);

        if (lane == _openOriginLane && _openDestinationLane != TrafficLane.None)
        {
            path = PathBetweenLanes(_openOriginLane, _openDestinationLane);
        }

        // Create a new car object and assign it's lane and front car.
        GameObject newCar = SpawnCar();
        Car carComponent = newCar.GetComponent<Car>();
        carComponent.Traffic = this;
        carComponent.Lane = lane;
        _carsInLane[lane].Add(carComponent);

        if (_lastCars.ContainsKey(lane))
        {
            _lastCars[lane].CarBehind = carComponent;
            carComponent.CarInFront = _lastCars[lane];
        }
        
        _lastCars[lane] = carComponent;
        
        // Set its initial position (first position on the path).
        newCar.transform.position = path[0];
        carComponent.Path = path;

        // Save the current time as the last spawn time.
        _lastSpawnTime = Time.time;
    }

    private GameObject SpawnCar()
    {
        // For now, we're just creating a cube.
        return Car.Create();
    }

    private TrafficLane RandomLane()
    {
        int randomLane = _random.Next(0, 4);
        return (TrafficLane)randomLane;
    }

    private Vector3[] RandomPathInLane(TrafficLane lane)
    {
        int randomPath = _random.Next(0, _lanePaths[lane].Length - 1);
        return _lanePaths[lane][randomPath].nodes.ToArray();
    }

    // Traffic lane status accessors.
    public void RemoveCar(Car car)
    {
        TrafficLane lane = car.Lane;
        _carsInLane[lane].Remove(car);
    }

    public bool IsLaneOpen(TrafficLane lane)
    {
        return (lane == _openOriginLane);
    }

    public void SetOriginLane(TrafficLane lane)
    {
        _openOriginLane = lane;
    }

    public void SetDestinationLane(TrafficLane lane)
    {
        _openDestinationLane = lane;

        if (_openOriginLane != TrafficLane.None && lane != TrafficLane.None)
        {
            var path = PathBetweenLanes(_openOriginLane, lane);
            foreach (Car car in _carsInLane[_openOriginLane])
            {
                car.Path = path;
            }
        }
    }

    public float MovementAmountForCar(Car car)
    {
        float distSq = float.MaxValue;
        Vector3 carPos = car.gameObject.transform.position;
        // First check the lane block if the lane isn't open.
        if (car.Lane != _openOriginLane || _openDestinationLane == TrafficLane.None)
        {
            GameObject block = _laneBlocks[car.Lane];
            distSq = (carPos - block.transform.position).sqrMagnitude;
        }

        // Check distance between car and car in front of it.
        if (car.CarInFront != null)
        {
            Vector3 carInFrontPos = car.CarInFront.gameObject.transform.position;
            float inFrontDist = (carPos - carInFrontPos).sqrMagnitude;
            if (inFrontDist < distSq)
            {
                distSq = inFrontDist;
            }
        }

        // Now that we've got the closest object's distance, we need to
        // determine the movement amount allowed for the car to move.
        float speed = Mathf.Min(distSq * 0.0004f, 0.004f);
        if (speed < 0.002f)
        {
            return 0.0f;
        }
        return speed;
    }

    public Vector3[] PathBetweenLanes(TrafficLane origin, TrafficLane destination)
    {
        string pathName = origin.ToString() + " to " + destination.ToString();
        var lanePaths = _lanePaths[origin];

        iTweenPath matchingPath = null;
        foreach (iTweenPath path in lanePaths)
        {
            if (path.pathName == pathName)
            {
                matchingPath = path;
                break;
            }
        }

        if (matchingPath != null)
        {
            return matchingPath.nodes.ToArray();
        }

        Debug.Log("Null path between " + pathName);

        return null;
    }

    public bool OriginLaneSet()
    {
        return _openOriginLane != TrafficLane.None;
    }
}
