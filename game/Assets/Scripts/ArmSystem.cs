using UnityEngine;
using System.Collections;

public class ArmSystem : MonoBehaviour {

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
		if (arm == leftArm) {
			heldKeyLeft = keypress;
			leftRaised = true;
		}
		else {
			heldKeyRight = keypress;
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
			heldKeyLeft = null;
			leftRaised = false;
		}
		else {
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
		raisedPositionNorth = new Vector3(-0.5f, 0.2f, 0);
		raisedPositionEast = new Vector3(0, 0.2f, 0.5f);
		raisedPositionSouth = new Vector3(0.5f, 0.2f, 0);
		raisedPositionWest = new Vector3(0, 0.2f, -0.5f);
		loweredRotation = new Vector3(0, 0, 0);
		raisedRotationNorth = new Vector3(0, 0, -90);
		raisedRotationEast = new Vector3(-90, 0, 0);
		raisedRotationSouth = new Vector3(0, 0, 90);
        raisedRotationWest = new Vector3(90, 0, 0);
	}

	// Update is called once per frame
	void Update () {

		GameObject arm = leftRaised ? rightArm : leftArm;
		Debug.Log (arm);

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



}
