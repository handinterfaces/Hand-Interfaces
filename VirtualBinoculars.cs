using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VirtualBinoculars : MonoBehaviour
{
    // Hand Tracking Data
    private OVRHand ovrHand;    
    private OVRSkeleton ovrSkeleton;
    private OVRSkeleton theOtherovrSkeleton;
    private OVRBone boneToTrack;
    private OVRBone theOtherboneToTrack;
    [SerializeField]
    private GameObject handToTrackMovement;
    [SerializeField]
    private GameObject theOtherhandToTrackMovement;

    // Gesture Recognition
    private InterfaceRetrieval interfaceRetrieval;

    // HandInterfaces binoculars
    [SerializeField]
    private GameObject binocularsView; // long-range view
    [SerializeField]
    private GameObject binoculars;
    [SerializeField]
    private GameObject eye;


    // Threshold, flags and buffer parameters
    [SerializeField]
    private float distanceThreshold = 0.1f; 
    [SerializeField]
    private float ViewDistanceThreshold = 0.4f;    

    // Awake is called first and will be called even if the script component is disabled
    void Awake() 
    {
        // Initialize hand tracking data of both emulating hands (two hands)
        ovrHand = handToTrackMovement.GetComponent<OVRHand>();
        ovrSkeleton = handToTrackMovement.GetComponent<OVRSkeleton>();
        theOtherovrSkeleton = theOtherhandToTrackMovement.GetComponent<OVRSkeleton>();
        
        // Initialize bones to track
        boneToTrack = ovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();
        theOtherboneToTrack = theOtherovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();

        // Initialize binoculars
        binocularsView.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Reinitialize bones to track if first attempt fails
        if (boneToTrack == null)
        {
            boneToTrack = ovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();
            if (boneToTrack != null){
                handToTrackMovement = boneToTrack.Transform.gameObject;
            }
        }
        if (theOtherboneToTrack == null)
        {
            theOtherboneToTrack = theOtherovrSkeleton.Bones
                .Where(b => b.Id == OVRSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();
            if (boneToTrack != null)
                theOtherhandToTrackMovement = theOtherboneToTrack.Transform.gameObject;
        }

        // Retrieve binoculars when both hands performing correctly
        BinocularsInteractionEnabler();
    }

    void BinocularsInteractionEnabler()
    {
        float h2h_distance = DistanceCalculator(theOtherhandToTrackMovement,handToTrackMovement);// hand to hand
        float o2e_distance = DistanceCalculator(eye,binoculars);// object to eye
        
        // Enable rendering if it is the current object
        if((h2h_distance < distanceThreshold) && (interfaceRetrieval.currentGesture_stable.name=="binoculars"))
            ChildrenRendering(binoculars, true);
        else 
            ChildrenRendering(binoculars,false);
        
        // Binoculars could be raised up close to users' eyes to transition their view from normal to long-range.
        if (o2e_distance < ViewDistanceThreshold && interfaceRetrieval.currentGesture_stable.name=="binoculars")
            binocularsView.SetActive(true);
        else
            binocularsView.SetActive(false);
    }

    // Calculate distance between two GameObjects
    private float DistanceCalculator(GameObject target, GameObject handToTrackMovement)
    {
        float distance = 0;      
        distance = Vector3.Distance(target.transform.position,handToTrackMovement.transform.position);
        return distance;
    }

    // Make the object appear or disappear
    void ChildrenRendering(GameObject parent, bool isEnabled)
    {
        foreach (Renderer r in parent.GetComponentsInChildren<Renderer>())
            r.enabled = isEnabled;
    } 

}
