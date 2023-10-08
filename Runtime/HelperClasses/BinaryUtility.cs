//使用utf-8
using System;
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

        public static T DeepClone<T>(T obj)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(obj));
            }

            // 创建一个内存流，用于序列化和反序列化
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(memoryStream);
            }
        }
    }
}
