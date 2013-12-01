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
    private Dictionary<TrafficLane, Car> _lastCarsRight;
    private Dictionary<TrafficLane, iTweenPath[]> _lanePaths;
    private Dictionary<TrafficLane, GameObject> _laneBlocks;
    private Dictionary<TrafficLane, GameObject> _rightLaneBlocks;
    private TrafficLane _openOriginLane;
    private TrafficLane _openDestinationLane;
    private TrafficLane _openOriginLaneRight;
    private TrafficLane _openDestinationLaneRight;
    private Dictionary<TrafficLane, List<Car>> _carsInLane;
    private Dictionary<TrafficLane, List<Car>> _carsInLaneRight;
    private int _maxCars = 5;
    private int _currentCars = 0;
    private float _lastIncreaseCarCountTime;

	public GameObject _car;

    public void IncreaseMaxCars()
    {
        _maxCars++;
    }

    public void SwitchCarIntersection(Car car)
    {
        if (car.Intersection == Intersection.Left)
        {
            SetNewCarOnIntersection(Intersection.Right);
        }
        else
        {
            SetNewCarOnIntersection(Intersection.Left);
        }
    }

    private void Start()
    {
        _openOriginLane = TrafficLane.None;
        _openDestinationLane = TrafficLane.None;
        _openOriginLaneRight = TrafficLane.None;
        _openDestinationLaneRight = TrafficLane.None;
        _lastCars = new Dictionary<TrafficLane, Car>();
        _lastCarsRight = new Dictionary<TrafficLane, Car>();
        _lanePaths = new Dictionary<TrafficLane, iTweenPath[]>();
        _laneBlocks = new Dictionary<TrafficLane, GameObject>();
        _rightLaneBlocks = new Dictionary<TrafficLane, GameObject>();
        _carsInLane = new Dictionary<TrafficLane, List<Car>>();
        _carsInLaneRight = new Dictionary<TrafficLane, List<Car>>();
        _carsInLane[TrafficLane.North] = new List<Car>();
        _carsInLane[TrafficLane.South] = new List<Car>();
        _carsInLane[TrafficLane.East] = new List<Car>();
        _carsInLane[TrafficLane.West] = new List<Car>();
        _carsInLaneRight[TrafficLane.North] = new List<Car>();
        _carsInLaneRight[TrafficLane.South] = new List<Car>();
        _carsInLaneRight[TrafficLane.East] = new List<Car>();
        _carsInLaneRight[TrafficLane.West] = new List<Car>();
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

        _rightLaneBlocks[TrafficLane.North] = GameObject.Find("NorthBlock 2");
        _rightLaneBlocks[TrafficLane.South] = GameObject.Find("SouthBlock 2");
        _rightLaneBlocks[TrafficLane.East] = GameObject.Find("EastBlock 2");
        _rightLaneBlocks[TrafficLane.West] = GameObject.Find("WestBlock 2");
    }

    private void Update()
    {
        if ((Time.time - _lastIncreaseCarCountTime) > 10.0f)
        {
            _maxCars++;
            _lastIncreaseCarCountTime = Time.time;
        }

        // Only spawn and move a new car once every random interval,
        // between 0.5 and 2.0 seconds.
        double dtAmount = (_random.NextDouble() * 2.0f) + 0.5f;
        float sinceLastSpawn = Time.time - _lastSpawnTime;
        if (sinceLastSpawn < dtAmount)
        {
            return;
        }

        if (_currentCars >= _maxCars)
        {
            return;
        }

        Intersection intersection = (Intersection)_random.Next(0, 2);
        SetNewCarOnIntersection(intersection);
        _currentCars++;
        _lastSpawnTime = Time.time;
    }

    private void SetNewCarOnIntersection(Intersection intersection)
    {
        // Get a random path that this car will take.
        TrafficLane lane = RandomLane();
        Vector3[] path = StartPathInLane(lane);

        Dictionary<TrafficLane, Car> lastCarsDict = null;
        if (intersection == Intersection.Left)
        {
            lastCarsDict = _lastCars;
        }
        else
        {
            lastCarsDict = _lastCarsRight;
        }

        // Create a new car object and assign it's lane and front car.
        GameObject newCar = SpawnCar();
        Car carComponent = newCar.GetComponent<Car>();
        carComponent.Intersection = intersection;
        carComponent.Traffic = this;
        carComponent.Lane = lane;
        if (intersection == Intersection.Left)
        {
            _carsInLane[lane].Add(carComponent);
        }
        else
        {
            _carsInLaneRight[lane].Add(carComponent);
        }

        if (lastCarsDict.ContainsKey(lane))
        {
            lastCarsDict[lane].CarBehind = carComponent;
            carComponent.CarInFront = lastCarsDict[lane];
        }
        
        lastCarsDict[lane] = carComponent;
        
        // Set its initial position (first position on the path).
        carComponent.SetInitialPath(path);
        carComponent.SetFirstPosition();

        if (lane == _openOriginLane && _openDestinationLane != TrafficLane.None)
        {
            Vector3[] finalPath = PathBetweenLanes(_openOriginLane, _openDestinationLane);
            carComponent.SetFinalPath(finalPath);
        }
    }

    private GameObject SpawnCar()
    {
        // For now, we're just creating a cube.
        //return Car.Create();
		GameObject car = (GameObject)Instantiate(_car);
		car.AddComponent<Car>();
		car.AddComponent<Rigidbody>();
        car.rigidbody.useGravity = false;
		return car;
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

    public void SetOriginLane(Intersection intersection, TrafficLane lane)
    {
        if (intersection == Intersection.Left)
        {
            _openOriginLane = lane;
        }
        else
        {
            _openOriginLaneRight = lane;
        }
    }

    public void SetDestinationLane(Intersection intersection, TrafficLane lane)
    {
        Dictionary<TrafficLane, List<Car>> cars = null;
        TrafficLane originLane = _openOriginLane;
        if (intersection == Intersection.Left)
        {
            _openDestinationLane = lane;
            cars = _carsInLane;
        }
        else
        {
            _openDestinationLaneRight = lane;
            cars = _carsInLaneRight;
            originLane = _openOriginLaneRight;
        }

        if (originLane != TrafficLane.None && lane != TrafficLane.None)
        {
            var path = PathBetweenLanes(originLane, lane);
            foreach (Car car in cars[originLane])
            {
                if (!car.IsOnFinalPath())
                {
                    car.SetFinalPath(path);
                }
            }
        }
    }

    public bool CanCarMove(Car car)
    {
        float distSq = float.MaxValue;
        Vector3 carPos = car.gameObject.transform.position;
        TrafficLane openOriginLane = _openOriginLane;
        TrafficLane openDestLane = _openDestinationLane;

        if (car.Intersection == Intersection.Right)
        {
            openOriginLane = _openOriginLaneRight;
            openDestLane = _openDestinationLaneRight;
        }

        // First check the lane block if the lane isn't open.
        if (car.Lane != openOriginLane || openDestLane == TrafficLane.None)
        {
            GameObject block = null;
            if (car.Intersection == Intersection.Left)
            {
                block = _laneBlocks[car.Lane];
            }
            else
            {
                block = _rightLaneBlocks[car.Lane];
            }
            
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

        if (distSq < 10.0f)
        {
            return false;
        }

        return true;
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
        return FindPathByName(pathName, origin);
    }

    public Vector3[] FindPathByName(string name, TrafficLane origin)
    {
        var lanePaths = _lanePaths[origin];

        iTweenPath matchingPath = null;
        foreach (iTweenPath path in lanePaths)
        {
            if (path.pathName == name)
            {
                matchingPath = path;
                break;
            }
        }

        if (matchingPath != null)
        {
            return matchingPath.nodes.ToArray();
        }

        return null;
    }

    public Vector3[] StartPathInLane(TrafficLane origin)
    {
        string pathName = origin.ToString() + " to Intersection";
        return FindPathByName(pathName, origin);
    }

    public bool OriginLaneSet()
    {
        return _openOriginLane != TrafficLane.None;
    }
}
