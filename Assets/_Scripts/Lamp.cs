using Fusion.Addons.PositionDebugging;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Linq;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    [SerializeField] GameObject lightbulb;
    [SerializeField] GameObject workingLightbulb;
    [SerializeField] GameObject lampBody;
    [SerializeField] GameObject lightbulbClone;
    [SerializeField] GameObject lampBodyClone;

    public ObjectGrabbedEventSender lightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender workingLightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender lampBodyObjectGrabbedEventSender;

    bool realLightbulbIsTurnedOn = false;
    bool lightbulbIsConnected = true;
    bool workingLightbulbIsConnected = false;
    bool cloneIsSpawned = false;

    //mozda treba bacit u onNetworkAwake ili slicno
    void Start()
    {
        lightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleLightbulbGrabbed;
        lightbulbObjectGrabbedEventSender.onObjectReleased += HandleLightbulbReleased;
        workingLightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleWorkingLightbulbGrabbed;
        workingLightbulbObjectGrabbedEventSender.onObjectReleased += HandleWorkingLightbulbReleased;
        lampBodyObjectGrabbedEventSender.onObjectGrabbed += HandleLampBodyGrabbed;
    }

    void HandleLightbulbGrabbed(GameObject grabbedObject)
    {
        Debug.Log("Object grabbed: " + grabbedObject.transform.parent.name);

        if (lightbulbIsConnected)
        {
            lightbulbIsConnected = false;
        }
    }

    void HandleLightbulbReleased(GameObject grabbedObject)
    {
        Debug.Log("Object released: " + grabbedObject.transform.parent.name);

        RaycastHit hit;
        if (Physics.Raycast(lightbulb.transform.position, lightbulb.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            Debug.Log(hit.transform.gameObject.name);
            if(hit.transform.gameObject.name == "Lamp Body" && !workingLightbulbIsConnected)
            {
                lightbulbIsConnected = true;
            }
        }
    }

    void HandleWorkingLightbulbGrabbed(GameObject grabbedObject)
    {
        Debug.Log("Object grabbed: " + grabbedObject.transform.parent.name);

        if (workingLightbulbIsConnected)
        {
            workingLightbulbIsConnected = false;
        }
    }

    void HandleWorkingLightbulbReleased(GameObject grabbedObject)
    {
        Debug.Log("Object released: " + grabbedObject.transform.parent.name);

        RaycastHit hit;
        if (Physics.Raycast(workingLightbulb.transform.position, workingLightbulb.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject.name == "Lamp Body" && !lightbulbIsConnected)
            {
                workingLightbulbIsConnected = true;
            }
        }
    }

    void HandleLampBodyGrabbed(GameObject grabbedObject)
    {
        Debug.Log("Object grabbed: " + grabbedObject.transform.parent.name);

        if (!cloneIsSpawned)
        {
            cloneIsSpawned = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (workingLightbulbIsConnected && !realLightbulbIsTurnedOn)
        {
            realLightbulbIsTurnedOn = true;
            lightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        if (!workingLightbulbIsConnected && realLightbulbIsTurnedOn)
        {
            realLightbulbIsTurnedOn = false;
            lightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }

        if (lightbulbIsConnected && !workingLightbulbIsConnected)
        {
            lightbulb.transform.position = lampBody.transform.position + lampBody.transform.up * 0.25f;
            lightbulb.transform.rotation = lampBody.transform.rotation;
        }

        else if (workingLightbulbIsConnected && !lightbulbIsConnected) 
        {
            workingLightbulb.transform.position = lampBody.transform.position + lampBody.transform.up * 0.25f;
            workingLightbulb.transform.rotation = lampBody.transform.rotation;
        }

        if (!cloneIsSpawned)
        {
            lightbulb.transform.position = lightbulbClone.transform.position;
            lightbulb.transform.rotation = lampBodyClone.transform.rotation;
            lampBody.transform.position = lampBodyClone.transform.position;
            lampBody.transform.rotation = lampBodyClone.transform.rotation;
        }
    }
}
