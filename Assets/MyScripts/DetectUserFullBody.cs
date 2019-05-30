using System;
using System.Collections;
using System.Collections.Generic;
using LightBuzz.Vitruvius;
using UnityEngine;

public class DetectUserFullBody : MonoBehaviour {

    private List<nuitrack.JointType> typeJoints = new List<nuitrack.JointType>();
    private string message;

    bool isDectedCorrectlyOnce;

    // Use this for initialization
    void Start () {

        foreach (nuitrack.JointType i in Enum.GetValues(typeof(nuitrack.JointType))) {
            if(i != nuitrack.JointType.LeftFingertip && i != nuitrack.JointType.RightFingertip 
                && i != nuitrack.JointType.RightFoot && i != nuitrack.JointType.LeftFoot && i != nuitrack.JointType.None)
            typeJoints.Add(i);
        }

        isDectedCorrectlyOnce = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (!isDectedCorrectlyOnce)
        {
            if (CurrentUserTracker.CurrentUser != 0)
            {
                nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
                message = "Skeleton found";

                bool flag = true;

                
                for (int q = 0; q < typeJoints.Count; q++)
                {
                    nuitrack.Joint joint = skeleton.GetJoint(typeJoints[q]);

                    if (joint.Confidence < 0.5)
                    {
                        flag = false;
                        //Debug.Log(joint.Type.ToString() + " " + joint.Confidence);
                        break;

                    }
                }

                if (!flag)
                {
                    message = "Skeleton NOT match";
                }
                else
                {
                    message = "Skeleton match";



                    //fill static UserMeasurement class
                    Vector3 l = skeleton.GetJoint(nuitrack.JointType.LeftShoulder).Real.ToVector3();
                    Vector3 r = skeleton.GetJoint(nuitrack.JointType.RightShoulder).Real.ToVector3();

                    UserMeasurement.width = Vector3.Distance(l, r);

                    //fill static UserMeasurement class
                    Vector3 neck = skeleton.GetJoint(nuitrack.JointType.Neck).Real.ToVector3();
                    Vector3 torso = skeleton.GetJoint(nuitrack.JointType.Waist).Real.ToVector3();

                    //UserMeasurement.neckWaistDist = Vector3.Distance(neck, torso);
                    
                    //fill static UserMeasurement class
                    float distFromSensor = torso.z;

                    //Debug.Log("Shoulder dist: " + UserMeasurement.shoulderDistance);
                    //Debug.Log("Nect Torso dist: " + UserMeasurement.neckWaistDist);

                    isDectedCorrectlyOnce = true;
                }
            }
            else
            {
                message = "Skeleton not found";
            }
        }
    }

    public bool isUserDetectedOnce()
    {
        return isDectedCorrectlyOnce;
    }

    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }

}
