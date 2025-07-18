using System;
using System.Collections.Generic;


/// <summary>
/// EVA Mapping configuration model.
/// </summary>
[Serializable]
public class MapEntry
{
    public string key;
    public string path;
}


[Serializable]
public class POIEntry
{
    public string description;
    public float x; // normalized [0,1]
    public float y;
    public string type;
}


[System.Serializable]
public class LandmarkEntry
{
    public string type;
    public string description;
    public float x;
    public float y;
}


[System.Serializable]
public class RoutePoint
{
    public float x;
    public float y;
}


[System.Serializable]
public class PredefinedRoute
{
    public string type; // currently is only "walk" or "drive"
    public List<RoutePoint> points;
}


[Serializable]
public class Mapping
{
    public float mapScale;
    public List<MapEntry> Maps;
    public List<POIEntry> POIs;
    public List<LandmarkEntry> Landmarks;
    public List<PredefinedRoute> PredefinedRoutes;


}


[Serializable]
public class MappingConfigRoot
{
    public Mapping Mapping;
}



