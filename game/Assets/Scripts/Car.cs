using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour
{
    private bool _started;
    private float _percentAcrossPath;
    private bool _isMoving;
    private Vector3[] _initialPath;
    private Vector3[] _finalPath;
    private bool _onFinalPath;

    private string _tweenId;

    // Properties
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
    }

    public void SetFinalPath(Vector3[] path)
    {
        _finalPath = path;
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
            iTween.Resume(gameObject);
            
        }
    }

    private void carLeave()
    {
        this.Traffic.RemoveCar(this);
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
