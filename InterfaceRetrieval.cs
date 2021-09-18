using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerData;
}

public class InterfaceRetrieval : MonoBehaviour
{
    // Hand Tracking Data
    public OVRSkeleton skeleton;
    private List<OVRBone> fingerBones;
    private GameObject hand;

    // Gesture Data
    public List<Gesture> gesture_dataset;
    private Gesture previousGesture;
    public Gesture currentGesture_stable;

    // HandInterfaces Object/Interface
    private GameObject joystick;
    private GameObject binoculars;
    private GameObject scissors;
    private Dictionary<string, GameObject> objectdict;

    // Sensitivity Matrices
    // // Numbers indicate diferent components of hand
    // // 0 indicates root frame of the hand, where the wrist is located
    // // 1~24 match each hand bones and fingertips
    private List<float> ss_joystick =  new List<float>{1,1,1,1,1,0.2f,// 0-5
                                                    1,1,1,1,1,// 6-10
                                                    1,1,1,1,1,// 11-15
                                                    1,1,1,0.1f,1,// 16-20
                                                    1,1,1,1};// 21-24
    private List<float> ss_binoculars =  new List<float>{1,1,1,1,1,1,1,// 0-5
                                                    1,1,1,1,1,// 6-10
                                                    1,1,1,1,1,// 11-15
                                                    1,1,1,2,2,// 16-20
                                                    2,2,2,1};// 21-24
    private List<float> ss_scissors =  new List<float>{1,1,1,1,1,1,// 0-5
                                                    1,2,2,1,1,// 6-10
                                                    1,1,1,1,1,// 11-15
                                                    1,1,1,1,1,// 16-20
                                                    1,1,1,1};// 21-24
    private Dictionary<string, List<float>> sensitivitydict;

    // Threshold, flags and buffer parameters
    public float threshold = 0.02f;
    public bool debugMode = true;
    private bool thereAreBones = false;
    private float startTime = 0f;
    private float timer = 0f;
    public float holdTime = 0.2f;//  200 milliseconds
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialize hand tracking data
        fingerBones = new List<OVRBone>(skeleton.Bones);
        hand = GameObject.FindGameObjectsWithTag("lefthand")[0];
        hand.GetComponent<Renderer>().enabled = true;

        // Initialize gesture data
        previousGesture = new Gesture();
        currentGesture_stable = new Gesture();
        
        // Initialize HandInterfaces' Objects
        joystick = GameObject.Find("joystick");
        binoculars = GameObject.Find("binocularsPhantom");
        scissors = GameObject.Find("scissors");
        objectdict = new Dictionary<string, GameObject>()
        {
            {"joystick", joystick},
            {"binoculars", binoculars},
            {"scissors", scissors}
        };
        foreach(var ges in objectdict)
            ChildrenRendering(ges.Value, false);

        // Initialize HandInterfaces' sensitivity matrices (W)
        sensitivitydict = new Dictionary<string, List<float>>()
        {
            {"joystick", ss_joystick},
            {"binoculars", ss_binoculars},
            {"scissors", ss_scissors}
        }; 
    }

    // Update is called once per frame
    void Update()
    {
        // thereAreBones == true when hand tracking is initialized successfully
        if (thereAreBones)
        {
            // Retrieve newest hand tracking data
            fingerBones= new List<OVRBone>(skeleton.Bones);

            // In debug mode, press space bar to save current hand gesture and create a gesture dataset. 
            // After that, we can use the dataset for gesture recognition and no longer need this line.
            if(debugMode && Input.GetKeyDown(KeyCode.Space)) Save();

            // Recognize gestures in the dataset. If nothing found, returns .name == null. 
            Gesture currentGesture = Recognize();


            // Implemented a hysteresis buffer to improve robustness if the gesture changes
            if (currentGesture.name != previousGesture.name){
                startTime = Time.time;
                timer = startTime;
            } else if (currentGesture.name == previousGesture.name){
                timer += Time.deltaTime;
                if (timer > (startTime + holdTime))
                {
                    currentGesture_stable = currentGesture;
                }
            }
            previousGesture = currentGesture;
            
            // Render relative visualization
            HandInterfaceRendering(currentGesture_stable);
        }
        // If hand tracking is not initialized successfully, re-attempt.
        else FindBones();
    }

    //Reinitialize hand tracking
    void FindBones()
    {
        if (skeleton.Bones.Count > 0)
            thereAreBones = true;
    }

    // Save the current gesture
    void Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach (var bone in fingerBones)
        {
            // Record finger position relative to root
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }
        g.fingerData = data;
        gesture_dataset.Add(g);
    }

    public Gesture Recognize()
    {
        // Initialize a blank gesture
        Gesture currentgesture = new Gesture();
        float currentMin = Mathf.Infinity;
        
        // Compare current gesture data (C) with all gesture templates in the dataset (D)
        foreach (var gesture in gesture_dataset)
        {
            string assumption = gesture.name;
            // c is a sum of Euclidean distances per pair of hand positions
            float sumDistance = 0;
            bool isDiscarded = false;
            List<float> ss = sensitivitydict[assumption];
            
            for (int i = 0; i < fingerBones.Count; i++)
            {
                // Current frame gesture data (C)
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData,gesture.fingerData[i]);
                // Apply sensitivity matrix (W)
                distance = distance*ss[i];
                // Discard if distance is unreasonably big
                if (distance>threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }

            // Look for the minimum difference score (S) among all gesture templates
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentgesture = gesture;
            }
        }
        // Return the gesture with smallest difference score (S)
        return currentgesture;
    }

    // Render relative visualization
    void HandInterfaceRendering(Gesture currentgesture)
    {
        string currentObject = currentgesture.name;

        // Initialize all objects
        foreach(var ges in objectdict)
            ChildrenRendering(ges.Value, false);//binoculars here is only a dummy
        // Only render the current object determined by the current gesture
        if(currentObject!=null)
        {
            if (caseSwitch=="binoculars"){
                // "binoculars" has its own script to render visualization. 
                // Only one hand cannot enable the rendering.
                return;
            }
            if (objectdict.ContainsKey(currentObject)){
                ChildrenRendering(objectdict[currentObject], true);
            }
        }   
    }

    // Make the object appear or disappear
    void ChildrenRendering(GameObject parent, bool isEnabled){
        foreach (Renderer r in parent.GetComponentsInChildren<Renderer>())
            r.enabled = isEnabled;
    }
 
}