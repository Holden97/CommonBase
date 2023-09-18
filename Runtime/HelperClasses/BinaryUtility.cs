//使用utf-8
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CommonBase
{
    public class BinaryUtility
    {
        public static void SaveWaveConfigData<T>(T data, string waveConfigPath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Create(waveConfigPath);
            Debug.Log($"已保存波次信息到:{waveConfigPath}");
            formatter.Serialize(fileStream, data);
            fileStream.Close();
        }

        public static T LoadWaveConfigData<T>(string path)
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = File.Open(path, FileMode.Open);
                T data = (T)formatter.Deserialize(fileStream);
                fileStream.Close();
                return data;
            }
            else
            {
                Debug.LogWarning($"不存在此二进制文件{path}！");
                return default(T);
            }
        }
    }
}
