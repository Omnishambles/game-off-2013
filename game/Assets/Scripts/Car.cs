using UnityEngine;
using System.Collections;

public enum Intersection
{
    Left,
    Right
}

public class Car : MonoBehaviour
{
    private bool _started;
    private float _percentAcrossPath;
    private bool _isMoving;
    private Vector3[] _initialPath;
    private Vector3[] _finalPath;
    private bool _onFinalPath;
    private int _pauseFrameCount;

    private string _tweenId;

    // Properties
    public Intersection Intersection
    {
        get;
        set;
    }

    public Traffic Traffic
    {
        get;
        set;
    }

    public TrafficLane Lane
    {
        get;
        set;
    }

    public Car CarInFront
    {
        get;
        set;
    }

    public Car CarBehind
    {
        get;
        set;
    }

    public void SetInitialPath(Vector3[] path)
    {
        _initialPath = path;

        if (this.Intersection == Intersection.Right)
        {
            Vector3[] newPath = new Vector3[path.Length];
            int i = 0;

            foreach (Vector3 node in path)
            {
                newPath[i] = new Vector3(node.x - 51.96347f,
                                         node.y,
                                         node.z);
                i++;
            }

            _initialPath = newPath;
        }
    }

    public void SetFinalPath(Vector3[] path)
    {
        _finalPath = path;

        if (this.Intersection == Intersection.Right)
        {
            Vector3[] newPath = new Vector3[path.Length];
            int i = 0;

            foreach (Vector3 node in path)
            {
                newPath[i] = new Vector3(node.x - 51.96347f,
                                         node.y,
                                         node.z);
                i++;
            }

            _finalPath = newPath;
        }
    }

    public void SetFirstPosition()
    {
        transform.position = _initialPath[0];
    }

    public bool IsOnFinalPath()
    {
        return _onFinalPath;
    }

    /*
    public static GameObject Create()
    {
        //GameObject car = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		GameObject car = (GameObject)Instantiate(_car);
        car.AddComponent<Car>();
        car.AddComponent<Rigidbody>();
        return car;
    }
	 */
    
    private void Start()
    {
        _percentAcrossPath = 0.02f;
        _isMoving = true;
    }

    private void FixedUpdate()
    {
        if (!_started)
        {
            if (_initialPath != null)
            {
                StartPath(_initialPath);
                _started = true;
            }

        }

        if (!this.Traffic.CanCarMove(this))
        {
            _isMoving = false;
            iTween.Pause(gameObject);
        }
        else if (_started && !_isMoving)
        {
            // Resume after 10 frames.
            _pauseFrameCount++;
            if (_pauseFrameCount >= 20)
            {
                iTween.Resume(gameObject);
                _pauseFrameCount = 0;
            }
        }
    }

    private void carLeave()
    {
        this.Traffic.RemoveCar(this);
        this.Traffic.SwitchCarIntersection(this);
        GameObject.Destroy(this.gameObject);
        Score.updateScore(1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isMoving = false;
    }

    private void StartPath(Vector3[] path)
    {
        iTween.MoveTo(gameObject, iTween.Hash(
                          "easetype", "linear",
                          "path", path,
                          "time", 3.0f,
                          "orienttopath", true,
                          "onComplete", "FinishedPath"));
    }

    private void FinishedPath()
    {
        if (!_onFinalPath && _finalPath != null)
        {
            _onFinalPath = true;
            StartPath(_finalPath);
        }
        else if (_onFinalPath)
        {
            Debug.Log("Car finished path");
            carLeave();
        }
    }
}
