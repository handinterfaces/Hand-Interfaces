using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VirtualScissors : MonoBehaviour
{
    // Hand Tracking Data
    private OVRHand ovrHand;
    private index_ovrSkeleton index_ovrSkeleton;
    private index_ovrSkeleton middle_ovrSkeleton;
    private OVRBone index_boneToTrack;
    private OVRBone middle_boneToTrack;
    [SerializeField]
    private GameObject emulating_index;
    [SerializeField]
    private GameObject emulating_middle;

    // Gesture Recognition
    [SerializeField]
    private InterfaceRetrieval interfaceRetrieval;

    // HandInterfaces scissors
    [SerializeField]
    private GameObject blade_index;
    [SerializeField]
    private GameObject blade_middle;
    private Vector3 closePlace;

    // Threshold, flags and buffer parameters
    [SerializeField]
    private float distanceThreshold = 0.05f;
    private bool lastState = true;
    private bool isDistant = true;
    [HideInInspector]
    public bool IsCutting = false;
    
    // Awake is called first and will be called even if the script component is disabled
    void Awake() 
    {
        // Initialize hand tracking data of the emulating hand (single hand)
        ovrHand = emulating_index.GetComponent<OVRHand>();
        index_ovrSkeleton = emulating_index.GetComponent<index_ovrSkeleton>();
        middle_ovrSkeleton = emulating_middle.GetComponent<index_ovrSkeleton>();

        // Initialize bones to track
        index_boneToTrack = index_ovrSkeleton.Bones
                .Where(b => b.Id == index_ovrSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();
        middle_boneToTrack = middle_ovrSkeleton.Bones
                .Where(b => b.Id == index_ovrSkeleton.BoneId.Hand_Middle3)
                .SingleOrDefault();  
        
    }
    void Start()
    {
        // Record where blades are at the beginning
        closePlace = blade_index.transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure hand tracking is initialized successfully
        if (index_boneToTrack == null)
        {
            index_boneToTrack = index_ovrSkeleton.Bones
                .Where(b => b.Id == index_ovrSkeleton.BoneId.Hand_Index3)
                .SingleOrDefault();
            if (index_boneToTrack != null)
                emulating_index = index_boneToTrack.Transform.gameObject;
        }

        if (middle_boneToTrack == null)
        { 
            middle_boneToTrack = middle_ovrSkeleton.Bones
                .Where(b => b.Id == index_ovrSkeleton.BoneId.Hand_Middle3)
                .SingleOrDefault();
            if (index_boneToTrack != null)
                emulating_middle = middle_boneToTrack.Transform.gameObject;
        }

        // Enable scissors's interaction if it is the current object
        if (interfaceRetrieval.currentGesture_stable.name=="scissors")
            ScissorsInteractionEnabler();      
    }

    // Calculate distance between two GameObjects
    private float DistanceCalculator(GameObject emulating_middle,GameObject emulating_index)
    {
        float distance = 0;  
        distance = Vector3.Distance(emulating_middle.transform.position,emulating_index.transform.position);
        return distance;
    }

    // Get closest GameObject to cut
    (GameObject,float) GetClosestObject(GameObject[] cuttables)
    {
        GameObject cMin = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject cuttable in cuttables)
        {
            float dist = DistanceCalculator(cuttable, emulating_index);
            if (dist < minDist)
            {
                cMin = cuttable;
                minDist = dist;
            }
        }
        return (cMin,minDist);
    }

    // Detect cutting movement of scissors' gesture
    private void ScissorsInteractionEnabler()
    {
        float h2h_distance = DistanceCalculator(emulating_middle,emulating_index); // distance between two finger tips
        GameObject[] cuttables = FindGameObjectsInLayer(3);// lay 3 contains all cuttable things
        (GameObject cuttable, float h2o_distance) = GetClosestObject(cuttables);

        isDistant = (h2h_distance > distanceThreshold); // false if in the proximity
        IsCutting = isDistant && !lastState; // cut it if two blades approach - touch - release           
        lastState = isDistant;
        ScissorsAnimation();                 
    }

    // Return all objects in a certain layer
    GameObject[] FindGameObjectsInLayer(int layer)
    {
        var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        var goList = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == 3)
                goList.Add(goArray[i]);
        }
        if (goList.Count == 0)
            return null;
        return goList.ToArray();
    }

    // Render animation of scissors' blade cutting
    private void ScissorsAnimation()
    {
        // Open Scissors
        if (isDistant)
        {
            blade_index.transform.localEulerAngles = new Vector3(closePlace.x,closePlace.y+30f,closePlace.z);
            blade_middle.transform.localEulerAngles = new Vector3(closePlace.x,closePlace.y-30f,closePlace.z);
        } 
        // Close Scissors
        else 
        {
            blade_index.transform.localEulerAngles = closePlace;
            blade_middle.transform.localEulerAngles = closePlace;
        }
    }
}
