using Fusion;
using Fusion.Addons.PositionDebugging;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Linq;
using UnityEngine;

public class LampNetworked : NetworkBehaviour
{
    [SerializeField] GameObject virtualLightbulbClone;
    [SerializeField] GameObject virtualReplacementLightbulbClone;
    [SerializeField] GameObject virtualLampClone;
    [SerializeField] GameObject virtualLightbulb;
    [SerializeField] GameObject virtualLamp;
    [SerializeField] GameObject virtualPlaceholderLightbulbClone; // used for fixing desync
    [SerializeField] GameObject virtualPlaceholderReplacementLightbulbClone; // used for fixing desync

    public ObjectGrabbedEventSender lightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender workingLightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender lampBodyObjectGrabbedEventSender;

    [Networked] public bool realLampIsTurnedOn { get; set; }
    [Networked] public bool virtualCloneIsSpawned { get; set; } = false;
    [Networked] public bool virtualLightbulbCloneIsConnectedToVirtualLampClone { get; set; } = true;
    [Networked] public bool virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone { get; set; } = false;
    

    void Start()
    {
        lightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleVirtualLightbulbCloneGrabbed;
        lightbulbObjectGrabbedEventSender.onObjectReleased += HandleVirtualLightbulbCloneReleased;
        workingLightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleVirtualReplacementLightbulbCloneGrabbed;
        workingLightbulbObjectGrabbedEventSender.onObjectReleased += HandleVirtualReplacementLightbulbCloneReleased;
        lampBodyObjectGrabbedEventSender.onObjectGrabbed += HandleVirtualLampCloneGrabbed;
    }

    void RealLampIsTurnedOn()
    {
        ChangeRealLampTurnedOnStateRpc(true);
    }

    void RealLampIsTurnedOff()
    {
        ChangeRealLampTurnedOnStateRpc(false);
    }

    // ### interaction handlers ####
    void HandleVirtualLampCloneGrabbed(GameObject grabbedObject)
    {
        if (!virtualCloneIsSpawned)
        {
            ChangeVirtualCloneSpawnedStateRpc(true);

            ChangeVirtualLightbulbCloneMaterialColorRpc(Color.white);
            ChangeVirtualReplacementLightbulbCloneMaterialColorRpc(Color.gray);

            ChangeVirtualPlacholderLightbulbCloneMaterialColorRpc(Color.gray);
            ChangeVirtualPlacholderReplacementLightbulbCloneMaterialColorRpc(Color.yellow);

            ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
            UseVirtualPlaceholderLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(true);
            UseVirtualPlaceholderReplacementLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(false);
            ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc();
        }

        /*
        if (virtualLightbulbCloneIsConnectedToVirtualLampClone)
        {
            ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc();
        }
        else if (virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone)
        {
            ChangeVirtualReplacementLightbulbCloneParentToVirtualLampCloneRpc();
        }
        */
    }

    void HandleVirtualLightbulbCloneGrabbed(GameObject grabbedObject)
    {
        ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);
        UseVirtualPlaceholderLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(false);
        ChangeVirtualLightbulbCloneParentToNoneRpc();
    }

    void HandleVirtualLightbulbCloneReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualLightbulbClone.transform.position, virtualLightbulbClone.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            if (hit.transform.gameObject.name == virtualLampClone.name && !virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone)
            {
                // ChangeVirtualPlacholderLightbulbCloneMaterialColorRpc(Color.gray);
                ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
                UseVirtualPlaceholderLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(true);
                ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc();
            }
        }
    }

    void HandleVirtualReplacementLightbulbCloneGrabbed(GameObject grabbedObject)
    {
        ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);
        UseVirtualPlaceholderReplacementLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(false);
        ChangeVirtualReplacementLightbulbCloneParentToNoneRpc();
    }

    void HandleVirtualReplacementLightbulbCloneReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualReplacementLightbulbClone.transform.position, virtualReplacementLightbulbClone.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            if (hit.transform.gameObject.name == virtualLampClone.name && !virtualLightbulbCloneIsConnectedToVirtualLampClone)
            {
                // ChangeVirtualPlacholderLightbulbCloneMaterialColorRpc(Color.yellow);
                ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
                UseVirtualPlaceholderReplacementLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(true);
                ChangeVirtualReplacementLightbulbCloneParentToVirtualLampCloneRpc();
                
                // ChangeVirtualReplacementLightbulbCloneMaterialColorRpc(Color.yellow);
            }
        }
    }

    //
    // RPCs
    //

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeRealLampTurnedOnStateRpc(bool turnedOn)
    {
        realLampIsTurnedOn = turnedOn;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualCloneSpawnedStateRpc(bool spawned)
    {
        virtualCloneIsSpawned = spawned;
    }

    // ### INTERCHABALITY ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UseVirtualPlaceholderLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(bool use)
    {
        virtualLightbulbClone.transform.Find("Visual").transform.Find("Sphere").GetComponent<MeshRenderer>().enabled = !use;
        virtualPlaceholderLightbulbClone.GetComponent<MeshRenderer>().enabled = use;
    }

    /*
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UseVirtualPlaceholderLightbulbCloneInsteadOfVirtualReplacementLightbulbCloneRpc(bool use)
    {
        virtualReplacementLightbulbClone.transform.Find("Visual").transform.Find("Sphere").GetComponent<MeshRenderer>().enabled = !use;
        virtualPlaceholderLightbulbClone.GetComponent<MeshRenderer>().enabled = use;
    }
    */

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UseVirtualPlaceholderReplacementLightbulbCloneInsteadOfVirtualLightbulbCloneRpc(bool use)
    {
        virtualReplacementLightbulbClone.transform.Find("Visual").transform.Find("Sphere").GetComponent<MeshRenderer>().enabled = !use;
        virtualPlaceholderReplacementLightbulbClone.GetComponent<MeshRenderer>().enabled = use;
    }
    


    // ### PARENTS ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc()
    {
        virtualLightbulbClone.transform.SetParent(virtualLampClone.transform, true);
        virtualLightbulbClone.transform
            .SetPositionAndRotation(virtualLampClone.transform.position
            + virtualLampClone.transform.up * 0.25f, virtualLampClone.transform.rotation);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLightbulbCloneParentToNoneRpc()
    {
        virtualLightbulbClone.transform.SetParent(virtualLampClone.transform.parent, true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualReplacementLightbulbCloneParentToVirtualLampCloneRpc()
    {
        virtualReplacementLightbulbClone.transform.SetParent(virtualLampClone.transform, true);
        virtualReplacementLightbulbClone.transform
            .SetPositionAndRotation(virtualLampClone.transform.position 
            + virtualLampClone.transform.up * 0.25f, virtualLampClone.transform.rotation);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualReplacementLightbulbCloneParentToNoneRpc()
    {
        virtualReplacementLightbulbClone.transform.SetParent(virtualLampClone.transform.parent, true);
    }

    // ### CONNECTED STATES ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(bool connected)
    {
        virtualLightbulbCloneIsConnectedToVirtualLampClone = connected;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(bool connected)
    {
        virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone = connected;
    }

    // ### COLORS ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLightbulbCloneMaterialColorRpc(Color color)
    {
        virtualLightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualReplacementLightbulbCloneMaterialColorRpc(Color color)
    {
        virtualReplacementLightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualPlacholderLightbulbCloneMaterialColorRpc(Color color)
    {
        virtualPlaceholderLightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualPlacholderReplacementLightbulbCloneMaterialColorRpc(Color color)
    {
        virtualPlaceholderReplacementLightbulbClone.GetComponentInChildren<MeshRenderer>().material.color = color;
    }
}
