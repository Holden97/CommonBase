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
        [MenuItem("Tools/UTF_8ת��")]
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
            Debug.Log("UTF_8ת��");
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
                        return true; // �ļ��� UTF-8 BOM ��ʶ����ͷ
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // �����쳣
                return false;
            }
        }

        private static bool IsUTF8WithoutBOM(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8, true))
                {
                    // ��ͼ��ȡ�ļ����ݣ������ȡ�ɹ���˵���� UTF-8 ����
                    while (reader.Peek() >= 0)
                    {
                        reader.Read();
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                // �����쳣
                return false;
            }
        }

        public static void ConvertFileToUTF8(string sourceFilePath)
        {
            try
            {
                // ��ȡ GB2312 ������ļ�
                string content;
                using (StreamReader reader = new StreamReader(sourceFilePath, true))
                {
                    content = reader.ReadToEnd();
                }

                // ������д�� UTF-8 ��������ļ�
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
