using Fusion;
using Fusion.Addons.PositionDebugging;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Linq;
using UnityEngine;

public class LampNetworked : NetworkBehaviour
{
    [SerializeField] GameObject lightbulb;
    [SerializeField] GameObject workingLightbulb;
    [SerializeField] GameObject lampBody;
    [SerializeField] GameObject lightbulbClone;
    [SerializeField] GameObject lampBodyClone;

    public ObjectGrabbedEventSender lightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender workingLightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender lampBodyObjectGrabbedEventSender;

    [Networked] public bool realLightbulbIsTurnedOn { get; set; }
    [Networked] public bool lightbulbIsConnected { get; set; } = true;
    [Networked] public bool workingLightbulbIsConnected { get; set; } = false;
    [Networked] public bool cloneIsSpawned { get; set; } = false;

    void Start()
    {
        lightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleLightbulbGrabbed;
        lightbulbObjectGrabbedEventSender.onObjectReleased += HandleLightbulbReleased;
        workingLightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleWorkingLightbulbGrabbed;
        workingLightbulbObjectGrabbedEventSender.onObjectReleased += HandleWorkingLightbulbReleased;
        lampBodyObjectGrabbedEventSender.onObjectGrabbed += HandleLampBodyGrabbed;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RealLightbulbIsTurnedOn()
    {
        realLightbulbIsTurnedOn = true;
    }

    void RealLightbulbIsTurnedOn()
    {
        if (Object.HasStateAuthority)
        {
            RPC_RealLightbulbIsTurnedOn();  
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RealLightbulbIsTurnedOff()
    {
        realLightbulbIsTurnedOn = false;
    }

    void RealLightbulbIsTurnedOff()
    {
        if (Object.HasStateAuthority)
        {
            RPC_RealLightbulbIsTurnedOff();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HandleLightbulbGrabbed()
    {
        lightbulbIsConnected = false;
    }

    void HandleLightbulbGrabbed(GameObject grabbedObject)
    {
        if (Object.HasStateAuthority)
        {
            RPC_HandleLightbulbGrabbed();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HandleLightbulbReleased(bool connected)
    {
        lightbulbIsConnected = connected;
    }

    void HandleLightbulbReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(lightbulb.transform.position, lightbulb.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            if (hit.transform.gameObject.name == "Lamp Body" && !workingLightbulbIsConnected)
            {
                if (Object.HasStateAuthority)
                {
                    RPC_HandleLightbulbReleased(true);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HandleWorkingLightbulbGrabbed()
    {
        workingLightbulbIsConnected = false;
    }

    void HandleWorkingLightbulbGrabbed(GameObject grabbedObject)
    {
        if (Object.HasStateAuthority)
        {
            RPC_HandleWorkingLightbulbGrabbed();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HandleWorkingLightbulbReleased(bool connected)
    {
        workingLightbulbIsConnected = connected;
    }

    void HandleWorkingLightbulbReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(workingLightbulb.transform.position, workingLightbulb.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            if (hit.transform.gameObject.name == "[BuildingBlock] Tijelo lampe" && !lightbulbIsConnected)
            {
                if (Object.HasStateAuthority)
                {
                    RPC_HandleWorkingLightbulbReleased(true);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HandleLampBodyGrabbed()
    {
        cloneIsSpawned = true;
    }

    void HandleLampBodyGrabbed(GameObject grabbedObject)
    {
        if (!Object.HasStateAuthority)
        {
            Object.RequestStateAuthority();
        }

        if (!cloneIsSpawned)
        {
            RPC_HandleLampBodyGrabbed();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        /*
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
        */

        if (workingLightbulbIsConnected && workingLightbulb.GetComponentInChildren<MeshRenderer>().material.color != Color.yellow)
        {
            workingLightbulb.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
        }
        else if (!workingLightbulbIsConnected && workingLightbulb.GetComponentInChildren<MeshRenderer>().material.color == Color.yellow)
        {
            workingLightbulb.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
        }

        if (realLightbulbIsTurnedOn && lightbulbClone.GetComponentInChildren<MeshRenderer>().material.color != Color.yellow)
        {
            lightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
        }
        else if (!realLightbulbIsTurnedOn && lightbulbClone.GetComponentInChildren<MeshRenderer>().material.color != Color.yellow)
        {
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
