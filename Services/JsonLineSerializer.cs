using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GhClickHeatmap.Services
{
  public static class JsonLineSerializer
  {
    private static readonly DataContractJsonSerializer EventSerializer =
      new DataContractJsonSerializer(typeof(UsabilityLogEvent));

    public static string Serialize(UsabilityLogEvent value)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        EventSerializer.WriteObject(stream, value);
        return Encoding.UTF8.GetString(stream.ToArray());
      }
    }

    public static UsabilityLogEvent? Deserialize(string text)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(text);
      using (MemoryStream stream = new MemoryStream(bytes))
      {
        return EventSerializer.ReadObject(stream) as UsabilityLogEvent;
      }
    }
  }
}
