using Fusion;
using Fusion.Addons.PositionDebugging;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Linq;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class LampNetworked : NetworkBehaviour
{
    [SerializeField] GameObject virtualLightbulbClone;
    [SerializeField] GameObject virtualReplacementLightbulbClone;
    [SerializeField] GameObject virtualLampClone;
    [SerializeField] GameObject virtualLightbulb;
    [SerializeField] GameObject virtualLamp;
    [SerializeField] GameObject virtualPlaceholderLightbulbClone; // used for fixing desync
    [SerializeField] GameObject virtualPlaceholderReplacementLightbulbClone; // used for fixing desync

    private GameObject virtualLightbulbCloneSocket;
    private GameObject virtualReplacementLightbulbCloneSocket;

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

        virtualLightbulbCloneSocket = virtualLightbulbClone.transform.Find("Visual")
            .Find("Capsule").gameObject;
        virtualReplacementLightbulbCloneSocket = virtualReplacementLightbulbClone.transform.Find("Visual")
            .Find("Capsule").gameObject;
    }

    public override void Spawned()
    {
        base.Spawned();



        ChangeVirtualLightbulbCloneMaterialColorRpc(Color.white);
        ChangeVirtualReplacementLightbulbCloneMaterialColorRpc(Color.gray);
        ChangeVirtualPlacholderLightbulbCloneMaterialColorRpc(Color.gray);
        ChangeVirtualPlacholderReplacementLightbulbCloneMaterialColorRpc(Color.yellow);

        ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
        ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc();

        ShowVirtualLightbulbCloneRpc(false);
        ShowVirtualPlaceholderLightbulbCloneRpc(true);
        ShowVirtualReplacementLightbulbCloneRpc(true);
        ShowVirtualPlaceholderReplacementLightbulbCloneRpc(false);
    }

    // call this when socket says lamp is on / off
    public void ChangeRealLampTurnedOnState(bool turnedOn)
    {
        ChangeRealLampTurnedOnStateRpc(turnedOn);
        if (turnedOn == true)
        {
            ChangeVirtualLightbulbMaterialColorRpc(Color.yellow);
        }
        else
        {
            ChangeVirtualLightbulbMaterialColorRpc(Color.white);
        }
    }

    // ### interaction handlers ####
    void HandleVirtualLampCloneGrabbed(GameObject grabbedObject)
    {
        if (!virtualCloneIsSpawned)
        {
            ChangeVirtualCloneSpawnedStateRpc(true);
        }
    }

    void HandleVirtualLightbulbCloneGrabbed(GameObject grabbedObject)
    {
        ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);
        ChangeVirtualLightbulbCloneParentToNoneRpc();
        ShowVirtualLightbulbCloneRpc(true);
        ShowVirtualPlaceholderLightbulbCloneRpc(false);
    }

    void HandleVirtualLightbulbCloneReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualLightbulbCloneSocket.transform.position, virtualLightbulbCloneSocket.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            if (hit.transform.gameObject.name == virtualLampClone.name && !virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone)
            {
                ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
                ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc();
                ShowVirtualPlaceholderLightbulbCloneRpc(true);
                ShowVirtualLightbulbCloneRpc(false);
            }
        }
    }

    void HandleVirtualReplacementLightbulbCloneGrabbed(GameObject grabbedObject)
    {
        ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);
        ChangeVirtualReplacementLightbulbCloneParentToNoneRpc();
        ShowVirtualReplacementLightbulbCloneRpc(true);
        ShowVirtualPlaceholderReplacementLightbulbCloneRpc(false);
    }

    void HandleVirtualReplacementLightbulbCloneReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualReplacementLightbulbCloneSocket.transform.position, virtualReplacementLightbulbCloneSocket.transform.TransformDirection(-Vector3.up), out hit, 0.5f))
        {
            if (hit.transform.gameObject.name == virtualLampClone.name && !virtualLightbulbCloneIsConnectedToVirtualLampClone)
            {
                ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
                ChangeVirtualReplacementLightbulbCloneParentToVirtualLampCloneRpc();
                ShowVirtualPlaceholderReplacementLightbulbCloneRpc(true);
                ShowVirtualReplacementLightbulbCloneRpc(false);
            }
        }
    }

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

    // ### VISIBILTY ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualLightbulbCloneRpc(bool visible)
    {
        var virtualLightbulbCloneTransform = virtualLightbulbClone.transform.Find("Visual");
        virtualLightbulbCloneTransform.Find("Sphere").GetComponent<MeshRenderer>().enabled = visible;
        virtualLightbulbCloneSocket.GetComponent<MeshRenderer>().enabled = visible;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualReplacementLightbulbCloneRpc(bool visible)
    {
        var virtualReplacementLightbulbCloneTransform = virtualReplacementLightbulbClone.transform.Find("Visual");
        virtualReplacementLightbulbCloneTransform.Find("Sphere").GetComponent<MeshRenderer>().enabled = visible;
        virtualReplacementLightbulbCloneSocket.GetComponent<MeshRenderer>().enabled = visible;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualPlaceholderLightbulbCloneRpc(bool visible)
    {
        virtualPlaceholderLightbulbClone.GetComponent<MeshRenderer>().enabled = visible;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualPlaceholderReplacementLightbulbCloneRpc(bool visible)
    {
        virtualPlaceholderReplacementLightbulbClone.GetComponent<MeshRenderer>().enabled = visible;
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
    public void ChangeVirtualLightbulbMaterialColorRpc(Color color)
    {
        virtualLightbulb.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

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
