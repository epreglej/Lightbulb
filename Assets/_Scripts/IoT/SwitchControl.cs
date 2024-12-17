using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Digiphy.IoT
{
    public class SwitchControl : MonoBehaviour
    {
        [SerializeField]
        private string baseUrl = "http://delock-3530.local/cm?";
        private bool wasDeviceOn = false;
        private float currentThreshold = 0.1f;

        private void Start()
        {
            // Start regularly checking the current every 2 seconds
            StartCoroutine(PeriodicCurrentCheck());
        }

        /// <summary>
        /// Toggles the power state of the switch.
        /// </summary>
        public void ToggleSwitch()
        {
            StartCoroutine(SendCommand("Power%20Toggle"));
        }

        /// <summary>
        /// Turns the switch on.
        /// </summary>
        public void TurnOnSwitch()
        {
            StartCoroutine(SendCommand("Power%20On"));
        }

        /// <summary>
        /// Turns the switch off.
        /// </summary>
        public void TurnOffSwitch()
        {
            StartCoroutine(SendCommand("Power%20Off"));
        }

        /// <summary>
        /// Periodically checks the current every 2 seconds and detects transitions.
        /// </summary>
        private IEnumerator PeriodicCurrentCheck()
        {
            while (true)
            {
                yield return StartCoroutine(GetStatusAndCheckCurrent());
                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Gets the current status and checks if the current has changed from below/above the threshold.
        /// </summary>
        private IEnumerator GetStatusAndCheckCurrent()
        {
            string url = $"{baseUrl}cmnd=Status%200";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
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
                            // Current went from <=0.1 to >0.1
                            DeviceTurnedOn();
                        }
                        else if (!isDeviceOn && wasDeviceOn)
                        {
                            // Current went from >0.1 to <=0.1
                            DeviceTurnedOff();
                        }

                        wasDeviceOn = isDeviceOn;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the device transitions from off to on (current passes above threshold).
        /// </summary>
        private void DeviceTurnedOn()
        {
            Debug.Log("Device has turned ON (current > 0.1A).");
            // Insert additional logic here...
        }

        /// <summary>
        /// Called when the device transitions from on to off (current goes below threshold).
        /// </summary>
        private void DeviceTurnedOff()
        {
            Debug.Log("Device has turned OFF (current <= 0.1A).");
            // Insert additional logic here...
        }

        private IEnumerator SendCommand(string command)
        {
            string url = $"{baseUrl}cmnd={command}";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
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
