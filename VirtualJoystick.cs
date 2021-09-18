using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class VirtualJoystick : MonoBehaviour
{   
    // Hand Tracking Data
    [SerializeField]
    private GameObject interacting_hand;
    [SerializeField]
    private GameObject emulating_hand;
    private OVRHand ovrHand;
    private OVRSkeleton interacting_ovrSkeleton;
    private OVRSkeleton emulating_ovrSkeleton;
    private OVRBone interacting_boneToTrack;
    private OVRBone emulating_boneToTrack;
    private GameObject interacting_trackpoint;
    private GameObject emulating_trackpoint;
    
    // Gesture Recognition
    [SerializeField]
    private InterfaceRetrieval interfaceRetrieval;

    // HandInterfaces joystick
    [SerializeField]
    private GameObject joystick_top;
    [SerializeField]
    private GameObject joystickbase;
 
    // Threshold, flags and buffer parameters
    [SerializeField]
    private float distanceThreshold = 0.05f;// can be tuned down to 7mm
    [SerializeField]
    public float minFingerPinchDownStrength = 0.5f;
    public bool IsPinchingReleased = false;
    public float FingerPinchStrength;

    // Awake is called first and will be called even if the script component is disabled
    void Awake() 
    {
        // Initialize hand tracking data of the emualting hand and the interacting hand (two hands)
        ovrHand = interacting_hand.GetComponent<OVRHand>();
        interacting_ovrSkeleton = interacting_hand.GetComponent<OVRSkeleton>();
        emulating_ovrSkeleton = emulating_hand.GetComponent<OVRSkeleton>();
        
        // Initialize bones to track
        interacting_boneToTrack = interacting_ovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();     
        emulating_boneToTrack = emulating_ovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Thumb3)
                .SingleOrDefault();
    }

    void Update()
    {
        // Reinitialize bones to track if first attempt fails
        if (interacting_boneToTrack == null)
        {
            interacting_boneToTrack = interacting_ovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();
            if (interacting_boneToTrack != null)
                interacting_trackpoint = interacting_boneToTrack.Transform.gameObject;
        }

        if (emulating_boneToTrack == null)
        {
            emulating_boneToTrack = emulating_ovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Thumb3)
                .SingleOrDefault();
            if (emulating_boneToTrack != null)
                emulating_trackpoint = emulating_boneToTrack.Transform.gameObject;
        }

        // Enable joystick's interaction if it is the current object
        if (interfaceRetrieval.currentGesture_stable.name == "joystick")
            JoystickInteractionEnabler();
        
    }

    // Calculate distance between two GameObjects
    private float DistanceCalculator(GameObject interacting_trackpoint, GameObject emulating_trackpoint)
    {
        float distance = 0;
        distance = Vector3.Distance(interacting_trackpoint.transform.position,emulating_trackpoint.transform.position);
        return distance;
    }

    // Enable joystick's interaction
    private void JoystickInteractionEnabler()
    {
        FingerPinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        // Finger pinch down
        float h2h_distance = DistanceCalculator(interacting_trackpoint, emulating_trackpoint);
        
        // If the interacting hand is pinching the thumb of emulating hand, joystick will response
        if (FingerPinchStrength >= minFingerPinchDownStrength && h2h_distance < distanceThreshold )
        {
            joystickbase.transform.LookAt(emulating_trackpoint.transform.position);
            IsPinchingReleased = true;
            return;
        }
        // Finger pinch up
        if (IsPinchingReleased)
            IsPinchingReleased = false;
    }
}