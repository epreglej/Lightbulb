using System;
using System.Collections.Generic;

[Serializable]
public class Coordinates
{
    public float latitude;
    public float longitude;
}

[Serializable]
public class Measurement
{
    public string parameter;
    public float value;
    public string lastUpdated;
    public string unit;
}

[Serializable]
public class Result
{
    public string location;
    public string city;
    public string country;
    public Coordinates coordinates;
    public List<Measurement> measurements;
}

[Serializable]
public class OpenAQResponse
{
    public List<Result> results;
}