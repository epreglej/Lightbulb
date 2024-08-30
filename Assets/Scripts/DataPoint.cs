using UnityEngine;

public class DataPoint
{
    public string location;
    public string city;
    public string country;
    public Coordinates coordinates;
    public string parameter;
    public PollutantType pollutantType;
    public float value;
    public float convertedValue;
    public float AQIvalue;
    public string lastUpdated;
    public string unit;
    public UnitType unitType;
    public Color AQIcolor;
    public GameObject pollutantContainer;
    public GameObject dataPointGameObject;

    public DataPoint(string location, string city, string country, Coordinates coordinates, 
        string parameter, float value, string lastUpdated, string unit)
    {
        this.location = location;
        this.city = city;
        this.country = country;
        this.coordinates = coordinates;
        this.parameter = parameter;
        this.value = value;
        this.lastUpdated = lastUpdated;
        this.unit = unit;
        pollutantType = GetPollutantType(parameter);
        unitType = GetUnitType(unit);
        convertedValue = GetConvertedValue(value);
        AQIvalue = GetAQIValue(convertedValue);
        AQIcolor = GetAQIColor(AQIvalue);
        pollutantContainer = GetPollutantContainer(parameter);
    }

    private PollutantType GetPollutantType(string parameter)
    {
        if (parameter.Equals("pm25"))
        {
            return PollutantType.PM25;
        }
        else if (parameter.Equals("pm10"))
        {
            return PollutantType.PM10;
        } 
        else if (parameter.Equals("no2"))
        {
            return PollutantType.NO2;
        } 
        else if (parameter.Equals("co"))
        {
            return PollutantType.CO;
        }
        else if (parameter.Equals("o3"))
        {
            return PollutantType.O3;
        }
        else if (parameter.Equals("so2"))
        {
            return PollutantType.SO2;
        }
        else
        {
            Debug.Log("Unsupported Pollutant Type: " + parameter);
            return PollutantType.Unsupported;
        }
    }

    private UnitType GetUnitType(string unit)
    {
        if (unit.Equals("µg/m³"))
        {
            return UnitType.µg_m3;
        }
        else if (unit.Equals("ppm"))
        {
            return UnitType.ppm;
        }
        else if (unit.Equals("ppb"))
        {
            return UnitType.ppb;
        }
        else
        {
            Debug.Log("Unsupported Unit Type: " + unit);
            return UnitType.unsupported;
        }
    }

    private float GetConvertedValue(float value)
    {
        switch (pollutantType)
        {
            case PollutantType.PM25:
            case PollutantType.PM10:
                if (unitType == UnitType.µg_m3)
                {
                    return value;
                } 
                else
                {
                    Debug.Log("Unsupported unit type for PM2.5 or PM10. Unit: " + unit);
                    return -1;
                }
            case PollutantType.NO2:
                if (unitType == UnitType.ppb)
                {
                    return value;
                }
                else if (unitType == UnitType.ppm)
                {
                    return value * 1000.0f;
                }
                else if (unitType == UnitType.µg_m3)
                {
                    return value / 1.88f;
                }
                else
                {
                    Debug.Log("Unsupported unit type for NO2. Unit: " + unit);
                    return -1;
                }
            case PollutantType.CO:
                if (unitType == UnitType.ppm)
                {
                    return value;
                }
                else if (unitType == UnitType.ppb)
                {
                    return value / 1000.0f;
                }
                else if (unitType == UnitType.µg_m3)
                {
                    return (value / 1000.0f) / 1.15f;
                }
                else
                {
                    Debug.Log("Unsupported unit type for CO. Unit: " + unit);
                    return -1;
                }
            case PollutantType.O3:
                if (unitType == UnitType.ppm)
                {
                    return value;
                }
                else if (unitType == UnitType.ppb)
                {
                    return value / 1000.0f;
                }
                else if (unitType == UnitType.µg_m3)
                {
                    return (value / 1000.0f) / 1.15f;
                }
                else
                {
                    Debug.Log("Unsupported unit type for O3. Unit: " + unit);
                    return -1;
                }
            case PollutantType.SO2:
                if (unitType == UnitType.ppb)
                {
                    return value;
                }
                else if (unitType == UnitType.ppm)
                {
                    return value * 1000.0f;
                }
                else if (unitType == UnitType.µg_m3)
                {
                    return value / 2.62f;
                }
                else
                {
                    Debug.Log("Unsupported unit type for SO2. Unit: " + unit);
                    return -1;
                }
            default:
                Debug.Log("Unsupported pollutant type. Pollutant type: " + parameter);
                return -1;
        }
    }
    
    private float GetAQIValue(float concentration)
    {
        // I --> Index values of Air Quality Index scale
        // C --> Concentration values of pollutant
        // Each array contains their breakpoints.

        int[] I_breakpoints = new int[] { 0, 50, 100, 150, 200, 300, 500 };
        float[] C_breakpoints;

        switch (pollutantType)
        {
            case PollutantType.PM25:
                C_breakpoints = new float[] { 0.0f, 12.0f, 35.5f, 55.5f, 150.5f, 250.5f, 350.5f }; // in µg/m³, 24-hour
                break;
            case PollutantType.PM10:
                C_breakpoints = new float[] { 0.0f, 55.0f, 155.0f, 255.0f, 355.0f, 425.0f, 505.0f }; // in µg/m³, 24-hour
                break;
            case PollutantType.NO2:
                C_breakpoints = new float[] { 0.0f, 54.0f, 101.0f, 361.0f, 650.0f, 1250.0f, 1650.0f }; // in ppb, 1-hour
                break;
            case PollutantType.CO:
                C_breakpoints = new float[] { 0.0f, 4.5f, 9.5f, 12.5f, 15.5f, 30.5f, 50.5f }; // in ppm, 8-hour
                break;
            case PollutantType.O3:
                C_breakpoints = new float[] { 0.0f, 0.055f, 0.071f, 0.086f, 0.106f, 0.201f, 1000.0f }; // in ppm, 8-hour
                break;
            case PollutantType.SO2:
                C_breakpoints = new float[] { 0.0f, 36.0f, 76.0f, 186.0f, 305.0f, 605.0f, 805.0f }; // in ppb, 1-hour
                break;
            default:
                Debug.Log("Unsupported pollutant type. Pollutant type: " + parameter);
                return -1;
        }

        for (int i = 0; i < C_breakpoints.Length - 1; i++)
        {
            if (concentration >= C_breakpoints[i] && concentration <= C_breakpoints[i + 1])
            {
                float Clow = C_breakpoints[i];
                float Chigh = C_breakpoints[i + 1];
                int Ilow = I_breakpoints[i];
                int Ihigh = I_breakpoints[i + 1];

                int AQI = (int)((Ihigh - Ilow) / (Chigh - Clow) * (concentration - Clow) + Ilow);
                return AQI;
            }
        }

        // If concentration value is outside the defined range, return -1 or handle accordingly.
        return -1;
    }

    private Color GetAQIColor(float AQIvalue)
    {
        if (AQIvalue >= 0 && AQIvalue <= 50)
        {
            return AQIColor.Green;
        }
        else if (AQIvalue >= 51 && AQIvalue <= 100)
        {
            return AQIColor.Yellow;
        }
        else if (AQIvalue >= 101 && AQIvalue <= 150)
        {
            return AQIColor.Orange;
        }
        else if (AQIvalue >= 151 && AQIvalue <= 200)
        {
            return AQIColor.Red;
        }
        else if (AQIvalue >= 201 && AQIvalue <= 300)
        {
            return AQIColor.Purple;
        }
        else if (AQIvalue >= 301)
        {
            return AQIColor.Maroon;
        }
        else
        {
            return Color.black;
        }
    }

    private GameObject GetPollutantContainer(string parameter)
    {
        if (parameter.Equals("pm25"))
        {
            return UIManager.Instance.PM25Container;
        }
        else if (parameter.Equals("pm10"))
        {
            return UIManager.Instance.PM10Container;
        }
        else if (parameter.Equals("no2"))
        {
            return UIManager.Instance.NO2Container;
        }
        else if (parameter.Equals("co"))
        {
            return UIManager.Instance.COContainer;
        }
        else if (parameter.Equals("o3"))
        {
            return UIManager.Instance.O3Container;
        }
        else if (parameter.Equals("so2"))
        {
            return UIManager.Instance.SO2Container;
        }
        else
        {
            Debug.Log("Unsupported Pollutant Type: " + parameter);
            return UIManager.Instance.DefaultContainer;
        }
    }
}
