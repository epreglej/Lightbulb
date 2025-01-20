using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Digiphy.IoT
{
    public class SwitchControl : MonoBehaviour
    {
        [SerializeField] LampNetworked lampNetworked;

        private string baseUrl = "http://delock-3530.local/cm?";
        private bool wasDeviceOn = false;
        private float currentThreshold = 0.05f;

        private void Start()
        {
            // Check the current every 2 seconds
            StartCoroutine(PeriodicCurrentCheck());
        }

        public void ToggleSwitch()
        {
            StartCoroutine(SendCommand("Power%20Toggle"));
        }

        public void TurnOnSwitch()
        {
            StartCoroutine(SendCommand("Power%20On"));
        }

        public void TurnOffSwitch()
        {
            StartCoroutine(SendCommand("Power%20Off"));
        }

        private IEnumerator PeriodicCurrentCheck()
        {
            while (true)
            {
                yield return StartCoroutine(GetStatusAndCheckCurrent());
                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator GetStatusAndCheckCurrent()
        {
            string url = $"{baseUrl}cmnd=Status%208";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error getting status: " + www.error);
                }
                else
                {
                    string json = www.downloadHandler.text;
                    StatusResponse statusResponse = JsonUtility.FromJson<StatusResponse>(json);

                    if (statusResponse != null && statusResponse.StatusSNS != null && statusResponse.StatusSNS.ENERGY != null)
                    {
                        float current = statusResponse.StatusSNS.ENERGY.Current;
                        bool isDeviceOn = current > currentThreshold;

                        // Check for state changes:
                        if (isDeviceOn && !wasDeviceOn)
                        {
                            DeviceTurnedOn();
                        }
                        else if (!isDeviceOn && wasDeviceOn)
                        {
                            DeviceTurnedOff();
                        }

                        wasDeviceOn = isDeviceOn;
                    }
                }
            }
        }

        private void DeviceTurnedOn()
        {
            Debug.Log("Device has turned ON (current > 0.05A).");
            lampNetworked.ChangeRealLampTurnedOnState(true);
        }

        private void DeviceTurnedOff()
        {
            Debug.Log("Device has turned OFF (current <= 0.05A).");
            lampNetworked.ChangeRealLampTurnedOnState(false);
        }

        private IEnumerator SendCommand(string command)
        {
            string url = $"{baseUrl}cmnd={command}";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)

                {
                    Debug.LogError("Error sending command: " + www.error);
                }
                else
                {
                    Debug.Log("Command Response: " + www.downloadHandler.text);
                }
            }
        }
    }
}
