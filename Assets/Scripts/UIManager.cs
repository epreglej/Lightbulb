using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Dropdown PollutantTypeDropdown;
    [SerializeField] private Button RefreshDataButton;
    [SerializeField] private Text NetworkStatusText;
    [SerializeField] private Text APIStatusText;

    [Header("AQI Toggle Buttons")]
    [SerializeField] private List<Toggle> ColorToggleButtons;

    [Header("Earth View Options")]
    [SerializeField] private EarthRenderer EarthRenderer;
    [SerializeField] private Slider EarthSaturationSlider;
    [SerializeField] private Slider CloudOpacitySlider;
    [SerializeField] private Slider SeaColorSlider;
    [SerializeField] private Slider MaterialSmoothnessSlider;

    [Header("Data Point Containers")]
    public GameObject PM25Container;
    public GameObject PM10Container;
    public GameObject NO2Container;
    public GameObject COContainer;
    public GameObject O3Container;
    public GameObject SO2Container;
    public GameObject DefaultContainer;

    private PhotonView photonView;
    private List<GameObject> PollutantTypeContainers;
    private List<Color> AQIColors;

    private void Awake()
    {
        Instance = this;
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Setting the inital values.
        PollutantTypeContainers = new List<GameObject>
        {
            PM25Container,
            PM10Container,
            NO2Container,
            COContainer,
            O3Container,
            SO2Container
        };

        int index = 0;
        for (int i = 0; i < PollutantTypeContainers.Count; i++)
        {
            PollutantTypeContainers[i].SetActive(index == i);
        }

        AQIColors = new List<Color>
        {
            AQIColor.Green,
            AQIColor.Yellow,
            AQIColor.Orange,
            AQIColor.Red,
            AQIColor.Purple,
            AQIColor.Maroon,
        };

        EarthSaturationSlider.value = EarthRenderer.colorSaturation;
        CloudOpacitySlider.value = EarthRenderer.cloudColor.a;
        SeaColorSlider.value = EarthRenderer.seaColor.a;
        MaterialSmoothnessSlider.value = EarthRenderer.smoothness;

        foreach (Toggle toggle in ColorToggleButtons)
        {
            toggle.isOn = true;
        }

        // Adding UI component listeners.
        RefreshDataButton.onClick.AddListener(delegate { OnRefreshButtonClicked(); });

        PollutantTypeDropdown.onValueChanged.AddListener(delegate { OnPollutantTypeDropdownValueChanged(); });

        ColorToggleButtons[0].onValueChanged.AddListener(delegate { OnColorButtonValueChanged(0); });
        ColorToggleButtons[1].onValueChanged.AddListener(delegate { OnColorButtonValueChanged(1); });
        ColorToggleButtons[2].onValueChanged.AddListener(delegate { OnColorButtonValueChanged(2); });
        ColorToggleButtons[3].onValueChanged.AddListener(delegate { OnColorButtonValueChanged(3); });
        ColorToggleButtons[4].onValueChanged.AddListener(delegate { OnColorButtonValueChanged(4); });
        ColorToggleButtons[5].onValueChanged.AddListener(delegate { OnColorButtonValueChanged(5); });

        EarthSaturationSlider.onValueChanged.AddListener(delegate { OnEarthSaturationSliderValueChanged(); });
        CloudOpacitySlider.onValueChanged.AddListener(delegate { OnCloudOpacitySliderValueChanged(); });
        SeaColorSlider.onValueChanged.AddListener(delegate { OnSeaColorSliderValueChanged(); });
        MaterialSmoothnessSlider.onValueChanged.AddListener(delegate { OnMaterialSmoothnessSliderValueChanged(); });
    }

    public void SetNetworkStatusText(string text)
    {
        NetworkStatusText.text = text;
    }

    public void SetAPIStatusText(string text)
    {
        APIStatusText.text = text;
    }

    public void SetUIObjectsInteractableProperty(bool value)
    {
        //RefreshDataButton.interactable = value;
        PollutantTypeDropdown.interactable = value;

        foreach (Toggle toggleButton in ColorToggleButtons)
        {
            toggleButton.interactable = value;
        }

        EarthSaturationSlider.interactable = value;
        CloudOpacitySlider.interactable = value;
        SeaColorSlider.interactable = value;
        MaterialSmoothnessSlider.interactable = value;
    }

    private void OnRefreshButtonClicked()
    {
        StartCoroutine(OpenAQDataVisualizer.Instance.SendRequest());
    }

    private void OnPollutantTypeDropdownValueChanged()
    {
        int index = PollutantTypeDropdown.value;
        for (int i = 0; i < PollutantTypeContainers.Count; i++)
        {
            PollutantTypeContainers[i].SetActive(index == i);
        }

        photonView.RPC("NetworkedPollutantTypeDropdownValueChanged", RpcTarget.Others, index);
    }

    [PunRPC]
    private void NetworkedPollutantTypeDropdownValueChanged(int index)
    {
        PollutantTypeDropdown.value = index;
        for (int i = 0; i < PollutantTypeContainers.Count; i++)
        {
            PollutantTypeContainers[i].SetActive(index == i);
        }
    }

    private void OnColorButtonValueChanged(int index)
    {
        List<DataPoint> dataPoints = OpenAQDataVisualizer.Instance.dataPoints;

        foreach (DataPoint dataPoint in dataPoints)
        {
            if (dataPoint.AQIcolor == AQIColors[index])
            {
                dataPoint.dataPointGameObject.SetActive(ColorToggleButtons[index].isOn);
            }
        }

        photonView.RPC("NetworkedOnColorButtonValueChanged", RpcTarget.Others, ColorToggleButtons[index].isOn, index);
    }

    [PunRPC]
    private void NetworkedOnColorButtonValueChanged(bool value, int index)
    {
        ColorToggleButtons[index].isOn = value;

        List<DataPoint> dataPoints = OpenAQDataVisualizer.Instance.dataPoints;
        foreach (DataPoint dataPoint in dataPoints)
        {
            if (dataPoint.AQIcolor == AQIColors[index])
            {
                dataPoint.dataPointGameObject.SetActive(ColorToggleButtons[index].isOn);
            }
        }
    }

    private void OnEarthSaturationSliderValueChanged()
    {
        if (EarthRenderer == null)
            return;
        
        EarthRenderer.colorSaturation = EarthSaturationSlider.value;

        photonView.RPC("NetworkedOnEarthSaturationSliderValueChanged", RpcTarget.Others, EarthSaturationSlider.value);
    }

    [PunRPC]
    private void NetworkedOnEarthSaturationSliderValueChanged(float value)
    {
        if (EarthRenderer == null)
            return;

        EarthSaturationSlider.value = value;
        EarthRenderer.colorSaturation = EarthSaturationSlider.value;
    }

    private void OnCloudOpacitySliderValueChanged()
    {
        if (EarthRenderer == null)
            return;

        EarthRenderer.cloudColor = new Color(EarthRenderer.cloudColor.r, EarthRenderer.cloudColor.g, EarthRenderer.cloudColor.b, CloudOpacitySlider.value);

        photonView.RPC("NetworkedOnCloudOpacitySliderValueChanged", RpcTarget.Others, CloudOpacitySlider.value);
    }

    [PunRPC]
    private void NetworkedOnCloudOpacitySliderValueChanged(float value)
    {
        if (EarthRenderer == null)
            return;

        CloudOpacitySlider.value = value;
        EarthRenderer.cloudColor = new Color(EarthRenderer.cloudColor.r, EarthRenderer.cloudColor.g, EarthRenderer.cloudColor.b, CloudOpacitySlider.value);
    }

    private void OnSeaColorSliderValueChanged()
    {
        if (EarthRenderer == null)
            return;

        EarthRenderer.seaColor = new Color(EarthRenderer.seaColor.r, EarthRenderer.seaColor.g, EarthRenderer.seaColor.b, SeaColorSlider.value);

        photonView.RPC("NetworkedOnSeaColorSliderValueChanged", RpcTarget.Others, SeaColorSlider.value);
    }

    [PunRPC]
    private void NetworkedOnSeaColorSliderValueChanged(float value)
    {
        if (EarthRenderer == null)
            return;

        SeaColorSlider.value = value;
        EarthRenderer.seaColor = new Color(EarthRenderer.seaColor.r, EarthRenderer.seaColor.g, EarthRenderer.seaColor.b, SeaColorSlider.value);
    }

    private void OnMaterialSmoothnessSliderValueChanged()
    {
        if (EarthRenderer == null)
            return;

        EarthRenderer.smoothness = MaterialSmoothnessSlider.value;

        photonView.RPC("NetworkedOnMaterialSmoothnessSliderValueChanged", RpcTarget.Others, MaterialSmoothnessSlider.value);
    }

    [PunRPC]
    private void NetworkedOnMaterialSmoothnessSliderValueChanged(float value)
    {
        if (EarthRenderer == null)
            return;

        MaterialSmoothnessSlider.value = value;
        EarthRenderer.smoothness = MaterialSmoothnessSlider.value;
    }
}
