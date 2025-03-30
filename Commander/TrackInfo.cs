using System.Xml.Serialization;
using CsTools.Functional;

namespace Commander;

static class TrackInfo
{
    public static TrackInfoData Get(string path)
    {
        var serializer = new XmlSerializer(typeof(XmlTrackInfo));
        using var stream = File.OpenRead(path);
        var xmlTrackInfo = serializer.Deserialize(stream) as XmlTrackInfo;
        var old = xmlTrackInfo?.Track?.Info?.Date != null && DateTime.Parse(xmlTrackInfo?.Track?.Info?.Date!) < new DateTime(2021, 1, 1);
        var trackInfo = new TrackInfoData(
            xmlTrackInfo?.Track?.Name,
            xmlTrackInfo?.Track?.Description,
            xmlTrackInfo?.Track?.Info?.Distance ?? 0,
            xmlTrackInfo?.Track?.Info?.Duration ?? 0,
            xmlTrackInfo?.Track?.Info?.AverageSpeed ?? 0,
            (int)(xmlTrackInfo
                ?.Track
                ?.TrackSegment
                ?.TrackPoints
                ?.Select(n => n.HeartRate)
                ?.Where(n => n != -1)
                ?.Average() ?? 0),
            0,
            xmlTrackInfo
                ?.Track
                ?.TrackSegment
                ?.TrackPoints
                ?.Select(n => n.HeartRate)
                ?.Where(n => n != -1)
                ?.Max() ?? 0,
            xmlTrackInfo
                ?.Track
                ?.TrackSegment
                ?.TrackPoints
                ?.Select(n => new TrackPoint(n.Latitude, n.Longitude, n.Elevation, n.Time, n.HeartRate ?? 0, old ? n.Speed ?? 0 : (n.Speed ?? 0) * 3.6f))
                    .ToArray());
        return trackInfo;
    }
}

record GetTrackInfo(string Path);

record TrackInfoData(
    string? Name,
    string? Description,
    float Distance,
    int Duration,
    float AverageSpeed,
    int AverageHeartRate,
    float MaxSpeed,
    int MaxHeartRate,
    TrackPoint[]? TrackPoints
);

record TrackPoint(
    double Latitude,
    double Longitude,
    double Elevation,
    string? Time,
    int Heartrate,
    float Velocity
);

//[XmlRoot(ElementName = "gpx", Namespace = "http://www.topografix.com/GPX/1/0")]
[XmlRoot(ElementName = "gpx")]
public class XmlTrackInfo
{
    [XmlElement("trk")]
    public XmlTrack? Track;
}

public class Info
{
    [XmlElement("date")]
    public string? Date;
    [XmlElement("distance")]
    public float Distance;
    [XmlElement("duration")]
    public int Duration;
    [XmlElement("averageSpeed")]
    public float AverageSpeed;
}

public class XmlTrack
{
    [XmlElement("name")]
    public string? Name;
    
    [XmlElement("desc")]
    public string? Description;

    [XmlElement("info")]
    public Info? Info;

    [XmlElement("trkseg")]
    public XmlTrackSegment? TrackSegment;
}

public class XmlTrackSegment
{
    [XmlElement("trkpt")]
    public XmlTrackPoint[]? TrackPoints;
}

public class XmlTrackPoint
{
    [XmlAttribute("lat")]
    public double Latitude;
    [XmlAttribute("lon")]
    public double Longitude;

    [XmlElement("ele")]
    public double Elevation;

    [XmlElement("time")]
    public string? Time;

    [XmlElement("speed")]
    public float? Speed;

    [XmlElement("heartrate")]
    public int? HeartRate;
}
