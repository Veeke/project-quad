using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace ProjectQuad
{
    public class JsonDataService
    {
        public T LoadData<T>(string relativePath, bool createEmptyOnFail = false)
        {
            string path = Application.dataPath + relativePath;

            if (!File.Exists(path))
            {
                Debug.Log($"Cannot load file at {path}. File does not exist.");
                throw new FileNotFoundException($"{path} does not exist.");
            }
            try
            {
                T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to load data due to {e.Message} {e.StackTrace}");
                return default;
            }
        }

        public bool SaveData<T>(string relativePath, T data)
        {
            string path = Application.dataPath + relativePath;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    Debug.Log("Creating a file for the first time!");
                }
                using FileStream stream = File.Create(path);
                stream.Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(data));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to save data due to {e.Message} {e.StackTrace}");
                return false;
            }
        }
    }
}