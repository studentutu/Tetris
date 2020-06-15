using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace Scripts.Services
{
    public class JsonDotNetConverter : IConverter
    {
        [SerializeField] private string safeTo = "app.custom";
        public string SaveFileWithExtension => safeTo;

        public T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public T LoadData<T>(string fileWithExtension)
        {
            var path = Path.Combine(Application.persistentDataPath, fileWithExtension);
            return FromJson<T>(File.ReadAllText(path));
        }

        public void SaveFile(string fileWithExtension, object data)
        {
            var path = Path.Combine(Application.persistentDataPath, fileWithExtension);
            Debug.LogWarning("path : " + path);
            File.WriteAllText(path, ToJson(data));
        }

        public string ToJson<T>(T toJson)
        {
            return JsonConvert.SerializeObject(toJson);
        }
    }
}