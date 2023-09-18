using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CommonBase.Editor
{
    public class ConvertToUTF8
    {
        [MenuItem("Tools/UTF_8转换")]
        public static void ConvertFilesToUTF8()
        {

            //string folderPath = Environment.CurrentDirectory;
            string folderPath = "C:\\Repo\\Summon_War\\Assets\\Test\\SpawnManager.cs";

            string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

            ConvertFileToUTF8(folderPath);
            foreach (string filePath in files)
            {
                if (!IsUTF8(filePath))
                {
                    ConvertFileToUTF8(filePath);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("UTF_8转换");
        }

        private static bool IsUTF8Encoded(string input)
        {
            Encoding encoding = new UTF8Encoding(false);
            byte[] bytes = encoding.GetBytes(input);
            string decodedInput = encoding.GetString(bytes);

            return input.Equals(decodedInput);
        }

        public static bool IsUTF8(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    int byte1 = fs.ReadByte();
                    int byte2 = fs.ReadByte();
                    int byte3 = fs.ReadByte();

                    if (byte1 == 0xEF && byte2 == 0xBB && byte3 == 0xBF)
                    {
                        return true; // 文件以 UTF-8 BOM 标识符开头
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // 处理异常
                return false;
            }
        }

        private static bool IsUTF8WithoutBOM(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8, true))
                {
                    // 试图读取文件内容，如果读取成功则说明是 UTF-8 编码
                    while (reader.Peek() >= 0)
                    {
                        reader.Read();
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                // 处理异常
                return false;
            }
        }

        public static void ConvertFileToUTF8(string sourceFilePath)
        {
            try
            {
                // 读取 GB2312 编码的文件
                string content;
                using (StreamReader reader = new StreamReader(sourceFilePath, true))
                {
                    content = reader.ReadToEnd();
                }

                // 将内容写入 UTF-8 编码的新文件
                File.WriteAllText(sourceFilePath, content, new UTF8Encoding(false));

                Console.WriteLine("Conversion from GB2312 to UTF-8 completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }

}
