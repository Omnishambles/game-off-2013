using UnityEngine;
using System.Collections;

public class ArmSystem : MonoBehaviour {
    public Intersection _intersection = Intersection.Left;
    public Traffic trafficControl;
	public GameObject leftArm;
	public GameObject rightArm;

	bool leftRaised = false;

	Vector3 loweredPosition;
	Vector3 raisedPositionNorth;
    Vector3 raisedPositionEast;
    Vector3 raisedPositionSouth;
    Vector3 raisedPositionWest;
    Vector3 loweredRotation;
    Vector3 raisedRotationNorth;
    Vector3 raisedRotationEast;
    Vector3 raisedRotationSouth;
    Vector3 raisedRotationWest;

	public string northKey;
	public string southKey;
	public string westKey;
	public string eastKey;

	string heldKeyLeft;
	string heldKeyRight;

	void RaiseArm(Vector3 position, Vector3 rotation, string keypress, GameObject arm) {
        // Get traffic lane by key press.
        TrafficLane lane = LaneForKey(keypress);

		if (arm == leftArm) {
			heldKeyLeft = keypress;
			leftRaised = true;
            trafficControl.SetOriginLane(_intersection, lane);
		}
		else {
			heldKeyRight = keypress;
            trafficControl.SetDestinationLane(_intersection, lane);
		}
        
        
		iTween.MoveTo (arm, iTween.Hash ("position", position,
		                                        "time", 1.0f,
		                                        "islocal", true));
		iTween.RotateTo (arm, iTween.Hash ("rotation", rotation,
		                                        "time", 1.0f,
												"islocal", true));
	}

	void LowerArm(GameObject arm) {
		if (arm == leftArm) {
            trafficControl.SetOriginLane(_intersection, TrafficLane.None);
			heldKeyLeft = null;
			leftRaised = false;
		}
		else {
            trafficControl.SetDestinationLane(_intersection, TrafficLane.None);
			heldKeyRight = null;
		}
		iTween.MoveTo (arm, iTween.Hash ("position", loweredPosition,
		                                        "time", 1.0f,
		                                        "islocal", true));
		iTween.RotateTo (arm, iTween.Hash ("rotation", loweredRotation,
		                                          "time", 1.0f,
		                                          "islocal", true));
	}

	// Use this for initialization
	void Start () {
		loweredPosition = new Vector3 (0, 0, 0);
		raisedPositionEast = new Vector3(-0.5f, 0.2f, 0);
		raisedPositionSouth = new Vector3(0, 0.2f, 0.5f);
		raisedPositionWest = new Vector3(0.5f, 0.2f, 0);
		raisedPositionNorth = new Vector3(0, 0.2f, -0.5f);
		loweredRotation = new Vector3(0, 0, 0);
		raisedRotationEast = new Vector3(0, 0, -90);
		raisedRotationSouth = new Vector3(-90, 0, 0);
		raisedRotationWest = new Vector3(0, 0, 90);
        raisedRotationNorth = new Vector3(90, 0, 0);
	}

	// Update is called once per frame
	void Update () {

		GameObject arm = leftRaised ? rightArm : leftArm;

		if(Input.GetKeyDown(northKey)) {
			RaiseArm(raisedPositionNorth,raisedRotationNorth,northKey,arm);
		}
		if(Input.GetKeyDown(westKey)) {
			RaiseArm(raisedPositionWest,raisedRotationWest,westKey,arm);
		}
		if(Input.GetKeyDown(southKey)) {
			RaiseArm(raisedPositionSouth,raisedRotationSouth,southKey,arm);
		}
		if(Input.GetKeyDown(eastKey)) {
			RaiseArm(raisedPositionEast,raisedRotationEast,eastKey,arm);
		}
		if (heldKeyLeft != null && Input.GetKeyUp(heldKeyLeft)) {
			LowerArm(leftArm);
		}
		if (heldKeyRight != null && Input.GetKeyUp(heldKeyRight)) {
			LowerArm(rightArm);
		}

	}

    private TrafficLane LaneForKey(string key)
    {
        if (key == northKey) return TrafficLane.North;
        if (key == southKey) return TrafficLane.South;
        if (key == westKey) return TrafficLane.West;
        if (key == eastKey) return TrafficLane.East;
        return TrafficLane.None;
    }

}
