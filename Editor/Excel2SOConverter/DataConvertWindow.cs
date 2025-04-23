#if UNITY_EDITOR
//使用UTF-8
using ExcelDataReader;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CommonBase.Editor
{
    public class CSMemberInfo
    {
        public string type;
        public string name;
        public string annotation;
        public CSMemberInfo(string type, string name, string annotation)
        {
            this.type = type.Trim();
            this.name = name.Trim();
            this.annotation = annotation.Trim();
        }

        public bool IsValidField => !type.IsNullOrEmpty() && !name.IsNullOrEmpty() && !annotation.IsNullOrEmpty();

        public CSMemberInfo()
        {
        }
    }

    public class DataConvertMenu : ScriptableObject
    {
        private int FirstDataRow = 4;
        private void OnEnable()
        {
            excelPath = PlayerPrefs.GetString("xlsxPath");
            nameSpaceOfData = PlayerPrefs.GetString("nameSpaceOfData");
        }

        public static string m_InputProtoDirectoryPath;

        [LabelText("命名空间名称")]
        public string nameSpaceOfData;

        [Button("更新命名空间", ButtonSizes.Medium, Stretch = false)]
        public void UpdateNameSpace()
        {
            // 将命名空间名称保存到 PlayerPrefs 中
            Debug.Log("nameSpaceOfData:" + nameSpaceOfData);
            PlayerPrefs.SetString("nameSpaceOfData", nameSpaceOfData);
        }

        [BoxGroup("Titles", ShowLabel = true, LabelText = "Excel转ScriptableObject")]

        [HorizontalGroup("Titles/ButtonGroup", 600f)]
        [LabelText("Excel路径")]
        public string excelPath;

        [Button("浏览Excel文件夹", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Titles/ButtonGroup", PaddingLeft = 0)]
        private void BrowseProtoDirectory()
        {
            string directory = EditorUtility.OpenFolderPanel("Select xlsx folder", m_InputProtoDirectoryPath,
                string.Empty);
            if (!string.IsNullOrEmpty(directory))
            {
                // 将原始的 proto 文件目录路径的值保存到本地 PlayerPrefs 中
                PlayerPrefs.SetString("xlsxPath", directory);

                excelPath = directory;
            }
        }

        [PropertyOrder(2)]
        [ButtonGroup("Titles/BG1")]
        [Button("Excel生成C#文件")]
        public void ExcelConvertToCSharpFile()
        {
            if (excelPath == null)
            {
                Debug.LogError("Excel字段为空，请检查！");
                return;
            }
            //几个规定
            //1.第一行写SO名称
            //2.第二行写字段名称
            //3.第三行写字段类型
            //做两步工作
            //1.根据字段生成对应类型
            //2.根据数据生成SO文件

            // 打开 Excel 文件
            string assetPath = excelPath.Substring(excelPath.IndexOf("Assets"));
            var allExcels = GetAllExcelFiles(assetPath);
            foreach (var path in allExcels)
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    try
                    {

                        // 读取整张工作表
                        DataSet dataSet = excelReader.AsDataSet();
                        // 获取工作表
                        foreach (DataTable sheet in dataSet.Tables)
                        {
                            GetSingleSheet(sheet);
                        }
                        // 关闭 Excel 读取器
                        excelReader.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("生成失败:" + e.ToString());
                        excelReader.Close();
                        throw;
                    }
                }

            }

        }

        private string[] GetAllExcelFiles(string path)
        {
            // 检查目录是否存在
            if (Directory.Exists(path))
            {
                // 获取目录下所有的.xlsx和.xls文件
                string[] excelFiles = Directory.GetFiles(path, "*.xlsx").FindAll(x => !x.Contains("~")).ToArray();
                string[] oldExcelFiles = Directory.GetFiles(path, "*.xls").FindAll(x => !x.Contains("~")).ToArray();

                // 合并两个数组
                string[] allExcelFiles = new string[excelFiles.Length + oldExcelFiles.Length];
                excelFiles.CopyTo(allExcelFiles, 0);
                oldExcelFiles.CopyTo(allExcelFiles, excelFiles.Length);
                return allExcelFiles;
            }
            else
            {
                Debug.LogError("Directory does not exist: " + path);
                return null;
            }

        }

        private void GetSingleSheet(DataTable dataTable)
        {
            // 获取行数和列数
            int numRows = dataTable.Rows.Count;
            int numColumns = dataTable.Columns.Count;
            var configInfo = dataTable.Rows[0][0] as string;
            var configInfoString = configInfo.Split("|");
            string configTypeInfo = default;
            string baseClassInfo = default;
            foreach (var c in configInfoString)
            {
                if (c.Contains("type"))
                {
                    configTypeInfo = c.Split("=")[1].Trim();
                }
                if (c.Contains("base"))
                {
                    baseClassInfo = c.Split("=")[1].Trim();
                }
            }

            //生成cs文件
            var info = GenerateCSMemberInfo(dataTable);
            GenerateSingleItemCsFile(configTypeInfo, info, $"Assets/Scripts/Configs/{configTypeInfo}.cs", baseClassInfo);
            GenerateSOItemCsFile(configTypeInfo, configTypeInfo, $"Assets/Scripts/Configs/{configTypeInfo}SO.cs");
        }

        [PropertyOrder(2)]
        [ButtonGroup("Titles/BG1")]
        [GUIColor(0f, 1f, 0f, 1f)]
        [Button("Excel生成SO")]
        public void ExcelConvertToSO()
        {
            if (excelPath.IsNullOrEmpty())
            {
                Debug.LogError("Excel字段为空，请检查！");
                return;
            }
            //几个规定
            //1.第一行写SO名称
            //2.第二行写字段名称
            //3.第三行写字段类型
            //做两步工作
            //1.根据字段生成对应类型
            //2.根据数据生成SO文件

            // 打开 Excel 文件
            string assetPath = excelPath.Substring(excelPath.IndexOf("Assets"));
            var allExcels = GetAllExcelFiles(assetPath);
            foreach (var excel in allExcels)
            {
                try
                {
                    using (FileStream stream = File.Open(excel, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        // 读取整张工作表
                        DataSet dataSet = excelReader.AsDataSet();
                        try
                        {
                            foreach (DataTable sheet in dataSet.Tables)
                            {
                                try
                                {
                                    GenerateSOBySingleSheet(sheet);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"Excel表{sheet.TableName}生成数据时出错，错误{e}");
                                    throw;
                                }
                            }
                            // 关闭 Excel 读取器
                            excelReader.Close();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("生成失败:" + e.ToString());
                            excelReader.Close();
                            throw e;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Excel生成SO失败");
                    throw e;
                }
            }
        }

        [BoxGroup("Buff", ShowLabel = true, LabelText = "Buff相关")]
        [Button("BuffSO->Buff脚本生成", ButtonSizes.Medium, Stretch = true)]
        [HorizontalGroup("Buff/ButtonGroup", PaddingLeft = 0)]
        private void CreateBuffBySO()
        {
            CreateBuffScriptBySO();
            AssetDatabase.Refresh();
        }

        // [Button("Buff脚本->预设生成", ButtonSizes.Medium, Stretch = true)]
        // [HorizontalGroup("Buff/ButtonGroup", PaddingLeft = 0)]
        // private void CreateBuffPrefabs()
        // {
        //     var typeList = FindScriptsOfType<RuntimeBuffBase>();
        //     foreach (var item in typeList)
        //     {
        //         CreateBuffPrefab(item);
        //     }
        //     AssetDatabase.Refresh();
        // }


        private void CreateSkillPrefab(Type type)
        {
            GameObject emptyObject = new GameObject(type.Name);
            emptyObject.AddComponent(type);

            // 在这里可以设置脚本的属性，根据需要进行调整

            // 将创建的物体保存为预设
            string prefabPath = "Assets/Resources/Prefabs/SkillPrefabs/" + type.Name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(emptyObject, prefabPath);

            // 销毁在场景中的实例，因为已经保存为预设
            DestroyImmediate(emptyObject);
            Debug.Log("Prefab created at path: " + prefabPath);
        }

        private void CreateTalentScriptByName(string talentId, string annotation)
        {
            if (talentId.IsNullOrEmpty())
            {
                var buffCounts = GetCSFileCountOnTop("Assets/Scripts/Talent/Talents");
                talentId = (buffCounts + 1001).ToString();
            }
            string talentPath = $"Assets/Scripts/Talent/Talents/Talent{talentId}Runtime.cs";
            // 生成 C# 文件内容
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\n");
            sb.AppendLine($"namespace {nameSpaceOfData} {{");
            if (!annotation.IsNullOrEmpty())
            {
                sb.AppendLine($"    ///<summary>");
                sb.AppendLine($"    ///{annotation}");
                sb.AppendLine($"    ///</summary>");
            }
            sb.AppendLine($"public  class Talent{talentId}Runtime:RuntimeTalentBase {{");

            sb.AppendLine("}");
            sb.AppendLine("}");

            // 将生成的内容写入 C# 文件
            File.WriteAllText(talentPath, sb.ToString());

            Debug.Log($"C# file generated at: {talentPath}");

        }

        private void CreateBuff(string buffId, string annotation)
        {
            CreateBuffScriptByName(buffId, annotation);
            AssetDatabase.Refresh();
        }

        private void CreateBuffs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateBuffScriptByName(null, null);
            }
            AssetDatabase.Refresh();
        }

        private void CreateBuffScriptBySO()
        {
            // var buffDataSO = AssetDatabase.LoadAssetAtPath<BuffDataSO>("Assets/Resources/SOConfigs/BuffDataSOData.asset");
            // foreach (var buffInfo in buffDataSO.info)
            // {
            //     string buffPath = $"Assets/Scripts/Buff/Buffs/Buff{buffInfo.id}Runtime.cs";

            //     if (File.Exists(buffPath))
            //     {
            //         //尝试更新buff summary
            //         UpdateSummary(buffPath, LocalizationManager.LocalizeChineseSimplified(buffInfo.description));
            //         continue;
            //     }
            //     else
            //     {
            //         CreateBuffScriptByName(buffInfo.id.ToString(), buffInfo.description);
            //     }
            // }
        }

        public static void UpdateSummary(string filePath, string newSummary)
        {
            // 读取文件内容
            string fileContent = File.ReadAllText(filePath);

            // 定义正则表达式匹配 summary 标签内容
            string summaryPattern = @"(///\s*<summary>\s*///)(.*?)(\s*///\s*</summary>)";
            Regex regex = new Regex(summaryPattern, RegexOptions.Singleline);

            // 替换 summary 标签内容
            string replacement = $"///<summary>\n\t/// {newSummary}\n\t///</summary>";
            string newFileContent = regex.Replace(fileContent, replacement);

            // 写回文件
            File.WriteAllText(filePath, newFileContent);

            Console.WriteLine("Summary updated successfully.");
        }

        int GetCSFileCountOnTop(string path)
        {
            // 检查目录是否存在
            if (Directory.Exists(path))
            {
                // 获取目录下所有的.cs文件
                string[] csFiles = Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly);

                // 返回.cs文件的数量
                return csFiles.Length;
            }
            else
            {
                Debug.LogError("Directory does not exist: " + path);
                return 0;
            }
        }

        private void CreateBuffScriptByName(string buffId, string annotation)
        {
            if (buffId.IsNullOrEmpty())
            {
                var buffCounts = GetCSFileCountOnTop("Assets/Scripts/Buff/Buffs");
                buffId = (buffCounts + 1001).ToString();
            }
            string buffPath = $"Assets/Scripts/Buff/Buffs/Buff{buffId}Runtime.cs";
            // 生成 C# 文件内容
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\n");
            sb.AppendLine($"namespace {nameSpaceOfData} {{");
            if (!annotation.IsNullOrEmpty())
            {
                sb.AppendLine($"    ///<summary>");
                sb.AppendLine($"    ///{annotation}");
                sb.AppendLine($"    ///</summary>");
            }
            sb.AppendLine($"public  class Buff{buffId}Runtime:RuntimeBuffBase {{");

            sb.AppendLine("}");
            sb.AppendLine("}");

            // 将生成的内容写入 C# 文件
            File.WriteAllText(buffPath, sb.ToString());

            Debug.Log($"C# file generated at: {buffPath}");

        }

        private void CreateSkillScriptByName(string skillId, string annotation)
        {
            if (skillId.IsNullOrEmpty())
            {
                var buffCounts = GetCSFileCountOnTop("Assets/Scripts/Skill/Skills");
                skillId = (buffCounts + 1001).ToString();
            }
            string skillPath = $"Assets/Scripts/Skill/Skills/Skill{skillId}Runtime.cs";
            // 生成 C# 文件内容
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\n");
            sb.AppendLine($"namespace {nameSpaceOfData} {{");
            if (!annotation.IsNullOrEmpty())
            {
                sb.AppendLine($"    ///<summary>");
                sb.AppendLine($"    ///{annotation}");
                sb.AppendLine($"    ///</summary>");
            }
            sb.AppendLine($"public  class Skill{skillId}Runtime:RuntimeSkillBase {{");

            sb.AppendLine("}");
            sb.AppendLine("}");

            // 将生成的内容写入 C# 文件
            File.WriteAllText(skillPath, sb.ToString());

            Debug.Log($"C# file generated at: {skillPath}");

        }

        Type GetTypeByNameMethod(string name)
        {
            // 从当前已加载的程序集列表中查找类型
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        [Button("导出对局数据", ButtonSizes.Medium, Stretch = false)]
        public void ExportGameRecord()
        {
        }

        [Button("清除成就", ButtonSizes.Medium, Stretch = false)]
        public void CleanAchievements()
        {
            // LocalDataManager.Instance.CleanAchievements();
        }

        private static void CreateBuffPrefab(Type type)
        {
            // 创建一个空物体
            GameObject emptyObject = new GameObject(type.Name);

            // 添加你的脚本到空物体上
            emptyObject.AddComponent(type);

            // 在这里可以设置脚本的属性，根据需要进行调整

            // 将创建的物体保存为预设
            string prefabPath = "Assets/Resources/Prefabs/BuffPrefabs/" + type.Name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(emptyObject, prefabPath);

            // 销毁在场景中的实例，因为已经保存为预设
            DestroyImmediate(emptyObject);

            Debug.Log("Prefab created at path: " + prefabPath);
        }

        List<Type> FindScriptsOfType<T>() where T : MonoBehaviour
        {
            List<Type> scriptTypes = GetDerivedTypes(typeof(T).FullName);

            // 遍历所有找到的脚本类型
            foreach (Type scriptType in scriptTypes)
            {
                // 输出找到的脚本类型的名称
                Debug.Log("Found script type: " + scriptType.FullName);
            }
            return scriptTypes;
        }

        List<Type> GetDerivedTypes<T>()
        {
            // 获取所有程序集
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 存储找到的脚本类型的列表
            List<Type> derivedTypes = new List<Type>();

            // 遍历所有程序集
            foreach (Assembly assembly in assemblies)
            {
                // 获取程序集中所有的类型
                Type[] types = assembly.GetTypes();

                // 找到所有继承自指定类型的类型
                IEnumerable<Type> derived = types.Where(type => typeof(T).IsAssignableFrom(type) && type != typeof(T));

                // 将找到的类型添加到列表中
                derivedTypes.AddRange(derived);
            }

            return derivedTypes;
        }

        List<Type> GetDerivedTypes(string baseType)
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var baseTypeClass = Type.GetType(baseType);
            if (baseTypeClass == null)
            {
                Debug.LogError(baseTypeClass + "为空，请检查");
                return null;
            }
            List<Type> foundScripts = new List<Type>();

            foreach (string assetPath in allAssetPaths)
            {
                if (assetPath.EndsWith(".cs")) // 仅处理 C# 脚本
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    if (script != null && script.GetClass() != null && script.GetClass().IsSubclassOf(baseTypeClass))
                    {
                        foundScripts.Add(script.GetClass());
                    }
                }
            }

            return foundScripts;
        }

        private void GenerateSOBySingleSheet(DataTable dataTable)
        {
            // 获取行数和列数
            int numRows = dataTable.Rows.Count;
            int numColumns = dataTable.Columns.Count;
            var configInfo = dataTable.Rows[0][0] as string;
            var configInfoString = configInfo.Split("|");
            string configTypeInfo = default;
            foreach (var c in configInfoString)
            {
                if (c.Contains("type"))
                {
                    configTypeInfo = c.Split("=")[1].Trim();
                }
            }

            //获取info信息
            var info = GenerateCSMemberInfo(dataTable);
            // 读取所有数据
            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
            for (int row = FirstDataRow; row < numRows; row++)
            {
                var curData = new Dictionary<string, object>();
                for (int col = 0; col < numColumns; col++)
                {
                    if (info.Length <= col)
                    {
                        continue;
                    }
                    if (dataTable.Rows[row][col] == DBNull.Value)
                    {
                        if (info[col] != null && info[col].IsValidField)
                        {
                            curData.Add(info[col].name, null);
                        }
                        continue;
                    }
                    switch (info[col].type)
                    {
                        case "int":
                            curData.Add(info[col].name, Convert.ToInt32(dataTable.Rows[row][col]));
                            break;
                        case "List<int>":
                            var intArray = dataTable.Rows[row][col].ToString().Split(';');
                            var result = new List<int>();
                            foreach (var item in intArray)
                            {
                                try
                                {
                                    if (item.IsNullOrEmpty())
                                    {
                                        result.Add(0);
                                    }
                                    else
                                    {
                                        result.Add(Convert.ToInt32(item));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError("List<int>转换出错:" + item + "," + e);
                                    throw;
                                }
                            }
                            curData.Add(info[col].name, result);
                            break;
                        case "List<string>":
                            var stringArray = dataTable.Rows[row][col].ToString().Split(';');
                            var stringArrayResult = new List<string>();
                            for (int i = 0; i < stringArray.Length; i++)
                            {
                                string item = stringArray[i];
                                try
                                {
                                    if (item.IsNullOrEmpty() && i == stringArray.Length - 1)
                                    {
                                        continue;
                                    }
                                    stringArrayResult.Add(item);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError("List<string>转换出错:" + item + "," + e);
                                    throw;
                                }
                            }
                            curData.Add(info[col].name, stringArrayResult);
                            break;
                        case "double":
                            curData.Add(info[col].name, Convert.ToDouble(dataTable.Rows[row][col]));
                            break;
                        case "float":
                            var d = Convert.ToDouble(dataTable.Rows[row][col]);
                            curData.Add(info[col].name, (float)d);
                            break;
                        //约定:bool值采用1和0表示，1代表true，0代表false
                        case "bool":
                            var b = Convert.ToInt32(dataTable.Rows[row][col]);
                            curData.Add(info[col].name, b == 1);
                            break;
                        case "Sprite":
                            if (dataTable.Rows[row][col].ToString().IsNullOrEmpty())
                            {
                                curData.Add(info[col].name, null);
                            }
                            else
                            {
                                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(dataTable.Rows[row][col].ToString());
                                curData.Add(info[col].name, sprite);
                            }
                            break;
                        case "GameObject":
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(dataTable.Rows[row][col].ToString());
                            curData.Add(info[col].name, go);
                            break;
                        case "AudioClip":
                            AudioClip a = AssetDatabase.LoadAssetAtPath<AudioClip>(dataTable.Rows[row][col].ToString());
                            curData.Add(info[col].name, a);
                            break;
                        //string替换掉所有换行符和制表符
                        case "string":
                            curData.Add(info[col].name, dataTable.Rows[row][col].ToString().Replace("\\n", "\n").Replace("\\t", "\t"));
                            break;
                        default:
                            break;
                    }
                }
                dataList.Add(curData);
            }
            //生成SO文件
            CreateSOByName(configTypeInfo, dataList);
        }

        private void CreateSOByName(string typeName, List<Dictionary<string, object>> data)
        {
            // 通过字符串获取类型
            //这里不能用Assembly.Load加载nameSpaceOfData，而是得通过Assembly-CSharp加载，不清楚为什么，Assembly-CSharp包含了nameSpaceOfData？
            var assembly = Assembly.Load("Assembly-CSharp");
            Type soType = assembly.GetType(nameSpaceOfData + "." + typeName + "SO");
            Type infoType = assembly.GetType(nameSpaceOfData + "." + typeName);

            var result = Activator.CreateInstance(soType);
            // 获取类型的属性信息
            FieldInfo infoField = soType.GetField("info");
            FieldInfo[] properties = infoType.GetFields();

            List<object> dataList = new List<object>();

            foreach (var item in data)
            {
                object instance = Activator.CreateInstance(infoType);
                foreach (var kv in item)
                {
                    var curField = properties.FindNotNull(x => x.Name == kv.Key);
                    if (curField != null)
                    {
                        if (kv.Value != DBNull.Value && kv.Value != null)
                        {
                            // 设置属性值
                            try
                            {
                                curField.SetValue(instance, kv.Value);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"{curField}设置出现问题{e},键:{kv.Key},值:{kv.Value}");
                                throw;
                            }
                        }
                    }

                }

                dataList.Add(instance);
            }

            // 使用反射创建泛型类型
            Type genericType = typeof(List<>);
            Type specificListType = genericType.MakeGenericType(infoType);

            // 创建 List<T> 的实例
            object specificList = Activator.CreateInstance(specificListType);

            // 将 List<object> 转换为 List<T>
            MethodInfo addMethod = specificListType.GetMethod("Add");
            foreach (var item in dataList)
            {
                addMethod.Invoke(specificList, new[] { Convert.ChangeType(item, infoType) });
            }

            infoField.SetValue(result, specificList);

            if (soType != null)
            {
                // 构造泛型方法
                var genericMethod = this.GetType().GetMethod("CreateSO").MakeGenericMethod(soType, infoType);
                object[] paramsArray = { result };
                genericMethod.Invoke(this, paramsArray);
            }
            else
            {
                Debug.LogError("Type not found.");
            }
        }

        public void CreateSO<T, K>(T data) where T : AutoListSO<K>
        {
            // 创建新的ScriptableObject实例
            AutoListSO<K> myDataInstance = ScriptableObject.CreateInstance<T>();

            // 设置ScriptableObject的字段值
            myDataInstance = data;

            // 在Assets目录下创建ScriptableObject文件
            //TODO:这里需要根据类型来创建文件夹，目前是直接在SOConfigs文件夹下创建
            string path = $"Assets/Resources/SOConfigs/{data.GetType().Name}Data.asset";
            AssetDatabase.CreateAsset(myDataInstance, path);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            Debug.Log("data created at: " + path);
        }

        private CSMemberInfo[] GenerateCSMemberInfo(DataTable dataTable)
        {
            // 获取行数和列数
            List<CSMemberInfo> list = new List<CSMemberInfo>();
            int numRows = dataTable.Rows.Count;
            int numColumns = dataTable.Columns.Count;

            // 读取所有数据
            for (int col = 0; col < numColumns; col++)
            {
                if (dataTable.Rows[2][col] == DBNull.Value
                    || dataTable.Rows[3][col] == DBNull.Value)
                {
                    list.Add(new CSMemberInfo());
                    continue;
                }
                var annotation = dataTable.Rows[1][col] as string;
                var memberName = dataTable.Rows[2][col] as string;
                //双斜杠注释不会出现在生成的代码中了
                var nameAndAnnotation = memberName.Split("//");
                var memberType = dataTable.Rows[3][col] as string;
                list.Add(new CSMemberInfo(memberType, nameAndAnnotation[0], annotation));
            }

            return list.ToArray();
        }

        void GenerateSingleItemCsFile(string csFileName, CSMemberInfo[] paramsArray, string csPath, string baseClassName = null)
        {
            Type baseClassType = default;
            FieldInfo[] baseFields = default;
            string baseClassDescription = baseClassName != null ? ": " + baseClassName : null;
            if (baseClassName != null)
            {
                baseClassType = Type.GetType(baseClassName + "," + nameSpaceOfData);
                baseFields = baseClassType.GetFields();
            }
            // 生成 C# 文件内容
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\n");
            sb.AppendLine($"namespace {nameSpaceOfData} {{");
            sb.AppendLine($"[Serializable]\r\n");
            sb.AppendLine($"public partial class {csFileName}{baseClassDescription} {{");

            // 生成属性
            for (int col = 0; col < paramsArray.Length; col++)
            {
                CSMemberInfo curInfo = paramsArray[col];
                if (baseClassName != null)
                {
                    if (baseFields.FindNotNull(x => x.Name == curInfo.name) == null
                        && curInfo.type != null && !string.IsNullOrEmpty(curInfo.type.Trim()))
                    {
                        if (!curInfo.annotation.IsNullOrEmpty())
                        {
                            sb.AppendLine($"    ///<summary>");
                            sb.AppendLine($"    ///{curInfo.annotation}");
                            sb.AppendLine($"    ///</summary>");
                        }
                        sb.AppendLine($"    public {curInfo.type} {curInfo.name};");
                    }
                }
                else
                {
                    if (curInfo.annotation != null)
                    {
                        sb.AppendLine($"    ///<summary>");
                        sb.AppendLine($"    ///{curInfo.annotation}");
                        sb.AppendLine($"    ///</summary>");
                    }
                    if (curInfo.type != null && !string.IsNullOrEmpty(curInfo.type.Trim()))
                    {
                        sb.AppendLine($"    public {curInfo.type} {curInfo.name};");
                    }
                }
            }

            sb.AppendLine("}");
            sb.AppendLine("}");

            // 将生成的内容写入 C# 文件
            File.WriteAllText(csPath, sb.ToString());

            Debug.Log($"C# file generated at: {csPath}");
            AssetDatabase.Refresh();
        }

        void GenerateSOItemCsFile(string itemTypeName, string csFileName, string csPath)
        {
            // 生成 C# 文件内容
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\nusing CommonBase;\r\n");

            sb.AppendLine($"namespace {nameSpaceOfData}{{");
            sb.AppendLine($"[CreateAssetMenu(fileName = \"New {csFileName}\", menuName = \"SO/{csFileName}\")]");
            sb.AppendLine($"public class {csFileName}SO:AutoListSO<{itemTypeName}> {{");

            // 生成属性
            //string resultString = char.ToLower(csFileName[0]) + csFileName.Substring(1);
            //sb.AppendLine($"public List<{itemTypeName}> {resultString};");

            sb.AppendLine("}");
            sb.AppendLine("}");

            // 将生成的内容写入 C# 文件
            File.WriteAllText(csPath, sb.ToString());

            Debug.Log($"C# file generated at: {csPath}");
            AssetDatabase.Refresh();
        }
    }

    public class DataConvertWindow : OdinMenuEditorWindow
    {
        public DataConvertMenu dataConvertMenu;
        // public WaveConfigSO waveConfigSO;

        [MenuItem("Tools/数据转换 %t")]
        private static void OpenWindow()
        {
            GetWindow<DataConvertWindow>().Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            dataConvertMenu = ScriptableObject.CreateInstance<DataConvertMenu>();
            // waveConfigSO = AssetDatabase.LoadAssetAtPath<WaveConfigSO>("Assets/Resources/SO/WaveConfigSO.asset");

            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
{
    { "SO<->Excel",                           dataConvertMenu,                           EditorIcons.Cut       },
    // { "怪物波次配置",                           waveConfigSO,                           EditorIcons.Airplane        },
    //{ "bytes<->Excel",                           dataConvertMenu,                           EditorIcons.Cut       },
    //{ "Odin Settings",                  null,                           SdfIconType.GearFill    },
    //{ "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
    //{ "Camera current",                 Camera.current                                          },
};

            //tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
            //    .AddThumbnailIcons();

            //tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");

            //var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
            //tree.MenuItems.Insert(2, customMenuItem);

            //tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
            //tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));

            return tree;
        }

        [HorizontalGroup]
        [Button(ButtonSizes.Large)]
        public void ReadExcelToBytes()
        {

        }

        [TableList]
        public List<SomeType> SomeTableData;
    }

    public class SomeType
    {
        [TableColumnWidth(50)]
        public bool Toggle;

        [AssetsOnly]
        public GameObject SomePrefab;

        public string Message;

        [TableColumnWidth(160)]
        [HorizontalGroup("Actions")]
        public void Test1() { }

        [HorizontalGroup("Actions")]
        public void Test2() { }
    }
}
#endif