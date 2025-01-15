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

    private GameObject virtualLightbulbCloneBulb;
    private GameObject virtualLightbulbCloneSocket;
    private GameObject virtualReplacementLightbulbCloneBulb;
    private GameObject virtualReplacementLightbulbCloneSocket;

    private Color defaultLightbulbOffColor = new Color(1f, 1f, 1f, 155f / 255f);
    private Color defaultLightbulbOnColor = new Color(1f, 0.92f, 0.016f, 200f / 255f);

    public ObjectGrabbedEventSender lightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender workingLightbulbObjectGrabbedEventSender;
    public ObjectGrabbedEventSender lampBodyObjectGrabbedEventSender;

    [Networked] private bool isStarted { get; set; } = false;

    [Networked] public bool realLampIsTurnedOn { get; set; }
    [Networked] public bool virtualLampCloneIsSpawned { get; set; } = false;
    [Networked] public bool virtualLampCloneIsTurnedOn { get; set; } = false;
    [Networked] public bool virtualLightbulbCloneIsConnectedToVirtualLampClone { get; set; } = true;
    [Networked] public bool virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone { get; set; } = false;

    [Networked] public bool virtualLightbulbCloneIsVisible { get; set; }
    [Networked] public bool virtualReplacementLightbulbCloneIsVisible { get; set; }
    [Networked] public bool virtualPlaceholderLightbulbCloneIsVisible { get; set; }
    [Networked] public bool virtualPlaceholderReplacementLightbulbCloneIsVisible { get; set; }


    //
    // za Branimira!
    // ovo traba staviti na true samo kad se prava lampa upali i false kad se ugasi
    // ChangeRealLampTurnedOnState(true);
    //

    void Start()
    {
        lightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleVirtualLightbulbCloneGrabbed;
        lightbulbObjectGrabbedEventSender.onObjectReleased += HandleVirtualLightbulbCloneReleased;
        workingLightbulbObjectGrabbedEventSender.onObjectGrabbed += HandleVirtualReplacementLightbulbCloneGrabbed;
        workingLightbulbObjectGrabbedEventSender.onObjectReleased += HandleVirtualReplacementLightbulbCloneReleased;
        lampBodyObjectGrabbedEventSender.onObjectGrabbed += HandleVirtualLampCloneGrabbed;

        virtualLightbulbCloneBulb = virtualLightbulbClone.transform.Find("Visual")
            .Find("Sphere").gameObject;
        virtualReplacementLightbulbCloneBulb = virtualReplacementLightbulbClone.transform.Find("Visual")
            .Find("Sphere").gameObject;
        virtualLightbulbCloneSocket = virtualLightbulbClone.transform.Find("Visual")
            .Find("Capsule").gameObject;
        virtualReplacementLightbulbCloneSocket = virtualReplacementLightbulbClone.transform.Find("Visual")
            .Find("Capsule").gameObject;
    }

    public override void Spawned()
    {
        base.Spawned();

        Debug.Log("LampNetworked script instance spawned.");

        ChangeVirtualPlacholderReplacementLightbulbCloneMaterialColorRpc(defaultLightbulbOnColor);

        if (!isStarted)
        {
            ShowVirtualLampCloneRpc(false);

            ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
            ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);

            ShowVirtualLightbulbCloneRpc(false);
            ShowVirtualPlaceholderLightbulbCloneRpc(true);
            ShowVirtualReplacementLightbulbCloneRpc(true);
            ShowVirtualPlaceholderReplacementLightbulbCloneRpc(false);

            isStarted = true;
        }
        else
        {
            ShowVirtualLampCloneRpc(virtualLampCloneIsSpawned);

            ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(virtualLightbulbCloneIsConnectedToVirtualLampClone);
            ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone);

            ShowVirtualLightbulbCloneRpc(virtualLightbulbCloneIsVisible);
            ShowVirtualPlaceholderLightbulbCloneRpc(virtualPlaceholderLightbulbCloneIsVisible);
            ShowVirtualReplacementLightbulbCloneRpc(virtualReplacementLightbulbCloneIsVisible);
            ShowVirtualPlaceholderReplacementLightbulbCloneRpc(virtualPlaceholderReplacementLightbulbCloneIsVisible);
        }
    }

    // call this when socket says lamp is on / off
    public void ChangeRealLampTurnedOnState(bool turnedOn)
    {
        ChangeRealLampTurnedOnStateRpc(turnedOn);
        if (turnedOn == true)
        {
            ChangeVirtualLightbulbMaterialColorRpc(defaultLightbulbOnColor);
        }
        else
        {
            ChangeVirtualLightbulbMaterialColorRpc(defaultLightbulbOffColor);
        }
    }

    public void ChangeVirtualLampCloneTurnedOnState()
    {
        ChangeVirtualLampCloneTurnedOnStateRpc(!virtualLampCloneIsTurnedOn);
        HandleVirtualReplacementLightbulbSocketed(); // draw new state for the lightbulb
    }

    // ### interaction handlers ####
    void HandleVirtualLampCloneGrabbed(GameObject grabbedObject)
    {
        if (!virtualLampCloneIsSpawned)
        {
            ChangeVirtualLampCloneSpawnedStateRpc(true);
            ShowVirtualLampCloneRpc(true);
        }
    }

    void HandleVirtualReplacementLightbulbSocketed()
    {
        if(virtualLampCloneIsTurnedOn)
        {
            ChangeVirtualPlacholderReplacementLightbulbCloneMaterialColorRpc(defaultLightbulbOnColor);
        }
        else
        {
            ChangeVirtualPlacholderReplacementLightbulbCloneMaterialColorRpc(defaultLightbulbOffColor);
        }
    }

    void HandleVirtualLightbulbCloneGrabbed(GameObject grabbedObject)
    {
        ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);
        ShowVirtualLightbulbCloneRpc(true);
        ShowVirtualPlaceholderLightbulbCloneRpc(false);
    }

    void HandleVirtualLightbulbCloneReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualLightbulbCloneSocket.transform.position, 
            virtualLightbulbCloneSocket.transform.TransformDirection(-Vector3.up), out hit, 0.125f))
        {
            if (hit.transform.gameObject.name == virtualLampClone.name 
                && !virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone)
            {
                ChangeVirtualLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
                ShowVirtualPlaceholderLightbulbCloneRpc(true);
                ShowVirtualLightbulbCloneRpc(false);
            }
        }
    }

    void HandleVirtualReplacementLightbulbCloneGrabbed(GameObject grabbedObject)
    {
        ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(false);
        ShowVirtualReplacementLightbulbCloneRpc(true);
        ShowVirtualPlaceholderReplacementLightbulbCloneRpc(false);
    }

    void HandleVirtualReplacementLightbulbCloneReleased(GameObject grabbedObject)
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualReplacementLightbulbCloneSocket.transform.position, 
            virtualReplacementLightbulbCloneSocket.transform.TransformDirection(-Vector3.up), out hit, 0.125f))
        {
            if (hit.transform.gameObject.name == virtualLampClone.name 
                && !virtualLightbulbCloneIsConnectedToVirtualLampClone)
            {
                ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(true);
                ShowVirtualPlaceholderReplacementLightbulbCloneRpc(true);
                ShowVirtualReplacementLightbulbCloneRpc(false);

                HandleVirtualReplacementLightbulbSocketed();
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeRealLampTurnedOnStateRpc(bool turnedOn)
    {
        realLampIsTurnedOn = turnedOn;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLampCloneSpawnedStateRpc(bool spawned)
    {
        virtualLampCloneIsSpawned = spawned;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLampCloneTurnedOnStateRpc(bool turnedOn)
    {
        virtualLampCloneIsTurnedOn = turnedOn;
    }

    // ### VISIBILTY ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualLampCloneRpc(bool visible)
    {
        virtualLampClone.transform.Find("Visual").gameObject.SetActive(visible);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualLightbulbCloneRpc(bool visible)
    {
        virtualLightbulbCloneIsVisible = visible;
        virtualLightbulbCloneBulb.GetComponent<MeshRenderer>().enabled = visible;
        virtualLightbulbCloneSocket.GetComponent<MeshRenderer>().enabled = visible;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualReplacementLightbulbCloneRpc(bool visible)
    {
        virtualReplacementLightbulbCloneIsVisible = visible;
        virtualReplacementLightbulbCloneBulb.GetComponent<MeshRenderer>().enabled = visible;
        virtualReplacementLightbulbCloneSocket.GetComponent<MeshRenderer>().enabled = visible;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualPlaceholderLightbulbCloneRpc(bool visible)
    {
        virtualPlaceholderLightbulbCloneIsVisible = visible;
        virtualPlaceholderLightbulbClone.GetComponent<MeshRenderer>().enabled = visible;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowVirtualPlaceholderReplacementLightbulbCloneRpc(bool visible)
    {
        virtualPlaceholderReplacementLightbulbCloneIsVisible = visible;
        virtualPlaceholderReplacementLightbulbClone.GetComponent<MeshRenderer>().enabled = visible;
    }

    // ### PARENTS ###
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc()
    {
        virtualLightbulbClone.transform.SetParent(virtualLampClone.transform, true);
        virtualLightbulbClone.transform
            .SetPositionAndRotation(virtualLampClone.transform.position
            + virtualLampClone.transform.up * 0.275f, virtualLampClone.transform.rotation);
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
            + virtualLampClone.transform.up * 0.275f, virtualLampClone.transform.rotation);
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

        if (connected)
        {
            ChangeVirtualLightbulbCloneParentToVirtualLampCloneRpc();
        }
        else
        {
            ChangeVirtualLightbulbCloneParentToNoneRpc();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeVirtualReplacementLightbulbCloneConnectedStateToVirtualLampCloneRpc(bool connected)
    {
        virtualReplacementLightbulbCloneIsConnectedToVirtualLampClone = connected;

        if (connected)
        {
            ChangeVirtualReplacementLightbulbCloneParentToVirtualLampCloneRpc();
        }
        else
        {
            ChangeVirtualReplacementLightbulbCloneParentToNoneRpc();
        }
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
