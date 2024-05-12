using Newtonsoft.Json;

public abstract class BaseJsonObject<T>
{
    public string ConvertToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static T FromJSON(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
