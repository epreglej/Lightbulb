using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class OpenAQDataVisualizer : MonoBehaviour
{
    public static OpenAQDataVisualizer Instance;

    [SerializeField] private GameObject Earth;
    [SerializeField] private GameObject PointPrefab;
    [SerializeField] private Material PointMaterial;

    public List<DataPoint> dataPoints;

    private const string OpenAQ_URL = "https://api.openaq.org/v2/latest?limit=10000";
    private OpenAQResponse apiResponse;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dataPoints = new List<DataPoint>();

        StartCoroutine(SendRequest());
    }

    public IEnumerator SendRequest()
    {
        UIManager.Instance.SetUIObjectsInteractableProperty(false);
        UIManager.Instance.SetAPIStatusText("[APIManager] Sending API request...");

        if (dataPoints.Count > 0) 
        {
            for (int i = 0; i < dataPoints.Count; i++)
            {
                Destroy(dataPoints[i].dataPointGameObject);
            }
            dataPoints.Clear();
        }

        using (UnityWebRequest webRequest = UnityWebRequest.Get(OpenAQ_URL))
        {
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for network errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                UIManager.Instance.SetAPIStatusText("[APIManager] Error: " + webRequest.error);
            }
            else
            {
                // Received data (response)
                string jsonResponse = webRequest.downloadHandler.text;

                // Deserialize the JSON response to a data object
                apiResponse = JsonUtility.FromJson<OpenAQResponse>(jsonResponse);

                // Process the data (for example, print the first measurement)
                if (apiResponse.results != null && apiResponse.results.Count > 0)
                {
                    foreach (Result r in apiResponse.results)
                    {
                        foreach (Measurement m in r.measurements)
                        {
                            DataPoint dataPoint = new DataPoint(r.location, r.city, r.country, r.coordinates, m.parameter, m.value, m.lastUpdated, m.unit);
                            
                            if (dataPoint.pollutantType == PollutantType.Unsupported || dataPoint.unitType == UnitType.unsupported || dataPoint.AQIcolor == Color.black)
                            {
                                continue;
                            }

                            dataPoints.Add(dataPoint);
                        }
                    }

                    Debug.Log("dataPointsCount: " + dataPoints.Count);

                    CreateMeshes();

                    Debug.Log("Results fetched.");
                    UIManager.Instance.SetAPIStatusText("[APIManager] Results fetched.");

                    UIManager.Instance.SetUIObjectsInteractableProperty(true);
                }
                else
                {
                    Debug.Log("No results found.");
                    UIManager.Instance.SetAPIStatusText("[APIManager] No results found.");
                }
            }
        }
    }

    public void CreateMeshes()
    {
        GameObject point = Instantiate(PointPrefab);
        Vector3[] vertices = point.GetComponent<MeshFilter>().mesh.vertices;
        int[] indices = point.GetComponent<MeshFilter>().mesh.triangles;

        List<Vector3> meshVertices = new List<Vector3>(65000);
        List<int> meshIndices = new List<int>(117000);
        List<Color> meshColors = new List<Color>(65000);

        foreach (DataPoint dataPoint in dataPoints)
        {
            float lat = dataPoint.coordinates.latitude;
            float lng = dataPoint.coordinates.longitude;
            float AQIvalue = dataPoint.AQIvalue;
            Color AQIcolor = dataPoint.AQIcolor;
            GameObject pollutantContainer = dataPoint.pollutantContainer;

            string locationName = dataPoint.location + "," + dataPoint.city + ", " + dataPoint.country;
            AppendPointVertices(point, vertices, indices, lng, lat, AQIvalue, AQIcolor, meshVertices, meshIndices, meshColors, pollutantContainer);
            if (meshVertices.Count + vertices.Length > 65000)
            {
                CreateObject(meshVertices, meshIndices, meshColors, locationName, pollutantContainer, dataPoint);
                meshVertices.Clear();
                meshIndices.Clear();
                meshColors.Clear();
            }

            CreateObject(meshVertices, meshIndices, meshColors, locationName, pollutantContainer, dataPoint);
            meshVertices.Clear();
            meshIndices.Clear();
            meshColors.Clear();
        }

        Destroy(point);
    }

    private void AppendPointVertices(GameObject point, Vector3[] vertices, int[] indices, float lng, float lat, float value, Color valueColor, 
        List<Vector3> meshVertices, List<int> meshIndices, List<Color> meshColors, GameObject containerObject)
    {
        Vector3 pointPosition;
        pointPosition.x = 0.5f * Mathf.Cos(lng * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad);
        pointPosition.y = 0.5f * Mathf.Sin(lat * Mathf.Deg2Rad);
        pointPosition.z = 0.5f * Mathf.Sin(lng * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad);

        point.transform.parent = containerObject.transform;
        point.transform.localPosition = pointPosition;
        point.transform.localScale = new Vector3(1, 1, Mathf.Max(0.001f, value * 0.001f));
        point.transform.LookAt(pointPosition * 2 + Earth.transform.position);

        int prevVertCount = meshVertices.Count;

        for (int k = 0; k < vertices.Length; k++)
        {
            meshVertices.Add(point.transform.TransformPoint(vertices[k]));
            meshColors.Add(valueColor);
        }

        for (int k = 0; k < indices.Length; k++)
        {
            meshIndices.Add(prevVertCount + indices[k]);
        }
    }

    private void CreateObject(List<Vector3> meshVertices, List<int> meshIndices, List<Color> meshColors, string locationName, GameObject containerObject, DataPoint dataPoint)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshIndices.ToArray();
        mesh.colors = meshColors.ToArray();

        GameObject obj = new GameObject(locationName);
        obj.transform.parent = containerObject.transform;
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = PointMaterial;

        dataPoint.dataPointGameObject = obj;
    }
}