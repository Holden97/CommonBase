using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace CommonBase
{
    public class XmlUtility
    {
        public static void ReadAllConfigXmlIn(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] files = directory.GetFiles("*", SearchOption.TopDirectoryOnly);
            foreach (var item in files)
            {
                if (item.Name.EndsWith(".xml"))
                {
                    ReadConfigXml(TrimXmlName(item.Name));
                }
            }
        }

        public static string TrimXmlName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf("\\") + 1, (fullPath.LastIndexOf(".") - fullPath.LastIndexOf("\\") - 1));
        }

        /// <summary>
        /// itemType和itemCode一定要存在
        /// </summary>
        /// <param name="path"></param>
        public static void ReadConfigXml(string path)
        {
            Debug.Log($"开始读取xml数据,位置:{path}");
            XmlDocument xml = new XmlDocument();
            xml.Load($"{Application.streamingAssetsPath}/{path}.xml");

            XmlNode root = xml.SelectSingleNode("items");
            XmlNodeList itemsList = root.SelectNodes("item");
            foreach (XmlNode item in itemsList)
            {
                var typeName = item.SelectSingleNode("itemType").InnerText;
                var itemName = item.SelectSingleNode("itemName").InnerText;
                if (string.IsNullOrEmpty(typeName))
                {
                    Debug.LogError($"类型名称为空:{typeName}");
                }
                Type nodeType = Type.GetType($"LittleWorld.Item.{typeName}Info");

                try
                {
                    object instance = Activator.CreateInstance(nodeType);
                    var fieldDic = new Dictionary<string, FieldInfo>();
                    var fields = nodeType.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        if (!fieldDic.TryAdd(field.Name, field))
                        {
                            Debug.LogError($"fieldName:{nodeType}.{field.Name}已存在！请检查");
                        }

                    }

                    foreach (XmlNode attribute in item.ChildNodes)
                    {
                        if (string.IsNullOrEmpty(attribute.InnerText))
                        {
                            continue;
                        }
                        try
                        {
                            if (fieldDic.TryGetValue(attribute.Name, out var curFieldInfo))
                            {
                                MethodInfo mi = typeof(Convert).GetMethod("To" + curFieldInfo.FieldType.Name, new[] { typeof(string) });
                                object value = mi.Invoke(null, new object[] { attribute.InnerText });
                                curFieldInfo.SetValue(instance, value);

                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"执行文本转换时出现问题,文本内容:{attribute.InnerText},具体问题：{e}");
                        }
                    }

                    //ObjectConfig.ObjectInfoDic.Add(((BaseInfo)instance).itemCode, (BaseInfo)instance);

                }
                catch (Exception e)
                {
                    Debug.LogError($"生成物体信息出现问题,类型名称:{typeName},物体名称:{itemName},具体问题：{e}");
                }

            }
        }
    }
}
