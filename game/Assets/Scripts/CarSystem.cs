using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class CarSystem : MonoBehaviour
{
    private ArrayList _cars;
    private iTweenPath[] _northPaths;
    private iTweenPath[] _southPaths;
    private iTweenPath[] _eastPaths;
    private iTweenPath[] _westPaths;
    private iTweenPath[] _combinedPaths;
    private float _lastSpawnTime;
    private System.Random _random;

    // Start method is called once when the object is created.
    private void Start()
    {
        // Set up random seed.
        _random = new System.Random();

        // Pull out iTweenPath arrays from their container objects.
        GameObject northPaths = GameObject.Find("Paths: North");
        _northPaths = northPaths.GetComponents<iTweenPath>();
        GameObject southPaths = GameObject.Find("Paths: South");
        _southPaths = southPaths.GetComponents<iTweenPath>();
        GameObject eastPaths = GameObject.Find("Paths: East");
        _eastPaths = eastPaths.GetComponents<iTweenPath>();
        GameObject westPaths = GameObject.Find("Paths: West");
        _westPaths = westPaths.GetComponents<iTweenPath>();

        _combinedPaths = _northPaths.Concat(_southPaths)
                                    .Concat(_eastPaths)
                                    .Concat(_westPaths)
                                    .ToArray();
    }

    // Update is called once per frame while the object is alive.
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
        Vector3[] path = RandomPath();
        // Create a new car object.
	    GameObject newCar = SpawnCar();
        // Set its initial position (first position on the path).
        newCar.transform.position = path[0];

        // Move new car along it's path over 5 seconds.
        iTween.MoveTo(newCar, iTween.Hash(
                          "path", path,
                          "easeType", iTween.EaseType.easeInOutSine,
                          "time", 5.0f));

        // Save the current time as the last spawn time.
        _lastSpawnTime = Time.time;
	}

    private GameObject SpawnCar()
    {
        // For now, we're just creating a cube.
        return GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    private Vector3[] RandomPath()
    {
        int randomPath = _random.Next(0, _combinedPaths.Length - 1);
        return _combinedPaths[randomPath].nodes.ToArray();
    }
}
