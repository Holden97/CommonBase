#if UNITY_EDITOR
//使用UTF-8
using ExcelDataReader;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            csFilePath = PlayerPrefs.GetString("csFilePath", "Assets/Scripts/Configs");
            soFilePath = PlayerPrefs.GetString("soFilePath", "Assets/Resources/SOConfigs");

            // 自动加载上次打开的文件（数据表编辑器功能）
            string lastOpenedFile = PlayerPrefs.GetString("DataEditor_LastFile", "");
            if (!string.IsNullOrEmpty(lastOpenedFile) && File.Exists(lastOpenedFile))
            {
                selectedFile = lastOpenedFile;
                currentFilePath = lastOpenedFile;
                LoadFileForEditor(currentFilePath);
            }
        }

        public static string m_InputProtoDirectoryPath;

        [LabelText("命名空间名称")]
        [OnValueChanged("OnNameSpaceValueChanged")] // 当值改变时调用 OnNameSpaceValueChanged 方法
        public string nameSpaceOfData;

        [LabelText("C#文件输出路径")]
        [FolderPath]
        [OnValueChanged("OnCsFilePathChanged")]
        public string csFilePath = "Assets/Scripts/Configs";

        [LabelText("SO文件输出路径")]
        [FolderPath]
        [OnValueChanged("OnSoFilePathChanged")]
        public string soFilePath = "Assets/Resources/SOConfigs";

        // 命名空间值改变时调用的方法
        private void OnNameSpaceValueChanged()
        {
            // 将命名空间名称保存到 PlayerPrefs 中
            Debug.Log("nameSpaceOfData:" + nameSpaceOfData);
            PlayerPrefs.SetString("nameSpaceOfData", nameSpaceOfData);
        }

        // C#文件路径改变时调用的方法
        private void OnCsFilePathChanged()
        {
            PlayerPrefs.SetString("csFilePath", csFilePath);
            Debug.Log("C#文件输出路径:" + csFilePath);
        }

        // SO文件路径改变时调用的方法
        private void OnSoFilePathChanged()
        {
            PlayerPrefs.SetString("soFilePath", soFilePath);
            Debug.Log("SO文件输出路径:" + soFilePath);
        }

        // [Button("更新命名空间", ButtonSizes.Medium, Stretch = false)]
        // public void UpdateNameSpace()
        // {
        //     // 保持原有的更新逻辑，可通过按钮手动触发
        //     Debug.Log("nameSpaceOfData:" + nameSpaceOfData);
        //     PlayerPrefs.SetString("nameSpaceOfData", nameSpaceOfData);
        // }

        [BoxGroup("Titles", ShowLabel = true, LabelText = "数据表转ScriptableObject")]

        [HorizontalGroup("Titles/ButtonGroup", 600f)]
        [LabelText("数据表路径")]
        public string excelPath;

        [Button("浏览数据表文件夹", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Titles/ButtonGroup", PaddingLeft = 0)]
        private void BrowseProtoDirectory()
        {
            string directory = EditorUtility.OpenFolderPanel("Select data files folder", m_InputProtoDirectoryPath,
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
        [Button("生成C#文件")]
        public void ExcelConvertToCSharpFile()
        {
            if (excelPath == null)
            {
                Debug.LogError("Excel字段为空，请检查！");
                return;
            }
            // 清空 C# 文件输出路径下的所有文件
            DeleteOldFiles(csFilePath);
            //几个规定
            //1.第一行写SO名称
            //2.第二行写字段名称
            //3.第三行写字段类型
            //做两步工作
            //1.根据字段生成对应类型
            //2.根据数据生成SO文件

            // 打开数据文件
            string assetPath = excelPath.Substring(excelPath.IndexOf("Assets"));
            var allDataFiles = GetAllExcelFiles(assetPath);
            foreach (var path in allDataFiles)
            {
                try
                {
                    // 判断文件类型
                    if (path.EndsWith(".csv"))
                    {
                        // CSV文件处理
                        DataTable csvTable = ReadCsvToDataTable(path);
                        csvTable.TableName = Path.GetFileNameWithoutExtension(path);
                        HandleSingleSheet(csvTable);
                    }
                    else
                    {
                        // Excel文件处理
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
                                    HandleSingleSheet(sheet);
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
                catch (Exception e)
                {
                    Debug.LogError($"处理文件 {path} 时出错: {e}");
                    throw;
                }
            }

        }

        private static void DeleteOldFiles(string path)
        {
            string configPath = path;

            // 如果目录不存在，则创建它
            if (!Directory.Exists(configPath))
            {
                try
                {
                    Directory.CreateDirectory(configPath);
                    Debug.Log($"创建目录: {configPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"创建目录 {configPath} 时出错: {e.Message}");
                    return;
                }
            }
            else
            {
                // 目录存在，删除其中的所有文件
                string[] allFiles = Directory.GetFiles(configPath, "*.*", SearchOption.AllDirectories);
                if (allFiles.Length > 0)
                {
                    foreach (string file in allFiles)
                    {
                        try
                        {
                            File.Delete(file);
                            Debug.Log($"已删除文件: {file}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"删除文件 {file} 时出错: {e.Message}");
                        }
                    }
                }
                else
                {
                    Debug.Log($"目录 {configPath} 已存在且为空");
                }
            }
        }

        private string[] GetAllExcelFiles(string path)
        {
            // 检查目录是否存在
            if (Directory.Exists(path))
            {
                // 获取目录下所有的.xlsx、.xls和.csv文件
                string[] excelFiles = Directory.GetFiles(path, "*.xlsx").FindAll(x => !x.Contains("~")).ToArray();
                string[] oldExcelFiles = Directory.GetFiles(path, "*.xls").FindAll(x => !x.Contains("~")).ToArray();
                string[] csvFiles = Directory.GetFiles(path, "*.csv").FindAll(x => !x.Contains("~")).ToArray();

                // 合并三个数组
                string[] allDataFiles = new string[excelFiles.Length + oldExcelFiles.Length + csvFiles.Length];
                excelFiles.CopyTo(allDataFiles, 0);
                oldExcelFiles.CopyTo(allDataFiles, excelFiles.Length);
                csvFiles.CopyTo(allDataFiles, excelFiles.Length + oldExcelFiles.Length);
                return allDataFiles;
            }
            else
            {
                Debug.LogError("Directory does not exist: " + path);
                return null;
            }

        }

        // CSV文件读取并转换为DataTable
        private DataTable ReadCsvToDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    // 先读取第一行以确定列数
                    string firstLine = sr.ReadLine();
                    if (string.IsNullOrEmpty(firstLine))
                    {
                        Debug.LogError($"CSV文件为空: {filePath}");
                        return dataTable;
                    }

                    string[] firstLineValues = ParseCsvLine(firstLine);
                    int columnCount = firstLineValues.Length;

                    // 创建列（使用通用列名，因为真正的列名在第3行）
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataTable.Columns.Add($"Column{i}");
                    }

                    // 将第一行作为数据行添加
                    DataRow firstRow = dataTable.NewRow();
                    for (int i = 0; i < firstLineValues.Length; i++)
                    {
                        firstRow[i] = firstLineValues[i].Trim();
                    }
                    dataTable.Rows.Add(firstRow);

                    // 继续读取剩余行
                    while (!sr.EndOfStream)
                    {
                        string[] rows = ParseCsvLine(sr.ReadLine());
                        DataRow dr = dataTable.NewRow();
                        for (int i = 0; i < columnCount && i < rows.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dataTable.Rows.Add(dr);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"读取CSV文件失败: {filePath}, 错误: {e}");
                throw;
            }

            return dataTable;
        }

        // 解析CSV行，处理引号内的逗号
        private string[] ParseCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return new string[0];

            List<string> result = new List<string>();
            StringBuilder currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }

        private void HandleSingleSheet(DataTable dataTable)
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
            GenerateSingleItemCsFile(configTypeInfo, info, $"{csFilePath}/{configTypeInfo}.cs", baseClassInfo);
            GenerateSOItemCsFile(configTypeInfo, configTypeInfo, $"{csFilePath}/{configTypeInfo}SO.cs");
        }

        [PropertyOrder(2)]
        [ButtonGroup("Titles/BG1")]
        [Button("生成样例CSV")]
        public void GenerateSampleCSV()
        {
            if (excelPath.IsNullOrEmpty())
            {
                Debug.LogError("请先选择数据表路径！");
                return;
            }

            string targetPath = excelPath;
            // 如果路径包含 "Assets"，则提取Assets后的路径
            if (excelPath.Contains("Assets"))
            {
                targetPath = excelPath.Substring(excelPath.IndexOf("Assets"));
            }

            // 确保目录存在
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // 生成简单示例CSV
            string simpleExamplePath = Path.Combine(targetPath, "ExampleData.csv");
            StringBuilder simpleCsv = new StringBuilder();
            simpleCsv.AppendLine("type=ExampleData|base=,,,");
            simpleCsv.AppendLine("数据ID,数据名称,数据值,描述信息");
            simpleCsv.AppendLine("id,name,value,description");
            simpleCsv.AppendLine("int,string,float,string");
            simpleCsv.AppendLine("1,示例数据1,100.5,这是第一条示例数据");
            simpleCsv.AppendLine("2,示例数据2,200.8,这是第二条示例数据");
            simpleCsv.AppendLine("3,示例数据3,300.3,这是第三条示例数据");
            File.WriteAllText(simpleExamplePath, simpleCsv.ToString(), Encoding.UTF8);
            Debug.Log($"简单示例CSV已生成: {simpleExamplePath}");

            // 生成复杂示例CSV
            string complexExamplePath = Path.Combine(targetPath, "ComplexExampleData.csv");
            StringBuilder complexCsv = new StringBuilder();
            complexCsv.AppendLine("type=ItemData|base=,,,,,,,");
            complexCsv.AppendLine("物品ID,物品名称,物品价格,是否可堆叠,最大堆叠数,物品等级,标签列表,属性加成列表");
            complexCsv.AppendLine("itemId,itemName,price,stackable,maxStack,level,tags,bonuses");
            complexCsv.AppendLine("int,string,float,bool,int,int,List<string>,List<int>");
            complexCsv.AppendLine("1001,生命药水,50.5,1,99,1,药水;消耗品,10;0;0");
            complexCsv.AppendLine("1002,魔法药水,75.8,1,99,1,药水;消耗品,0;20;0");
            complexCsv.AppendLine("1003,钢铁剑,500.0,0,1,5,武器;近战,50;0;10");
            complexCsv.AppendLine("1004,木质盾牌,200.0,0,1,3,防具;盾牌,0;30;5");
            File.WriteAllText(complexExamplePath, complexCsv.ToString(), Encoding.UTF8);
            Debug.Log($"复杂示例CSV已生成: {complexExamplePath}");

            AssetDatabase.Refresh();
            Debug.Log($"样例CSV文件已生成在: {targetPath}");
        }

        [PropertyOrder(2)]
        [ButtonGroup("Titles/BG1")]
        [GUIColor(0f, 1f, 0f, 1f)]
        [Button("生成SO")]
        public void ExcelConvertToSO()
        {
            if (excelPath.IsNullOrEmpty())
            {
                Debug.LogError("Excel字段为空，请检查！");
                return;
            }
            DeleteOldFiles(soFilePath);

            //几个规定
            //1.第一行写SO名称
            //2.第二行写字段名称
            //3.第三行写字段类型
            //做两步工作
            //1.根据字段生成对应类型
            //2.根据数据生成SO文件

            // 打开数据文件
            string assetPath = excelPath.Substring(excelPath.IndexOf("Assets"));
            var allDataFiles = GetAllExcelFiles(assetPath);
            foreach (var dataFile in allDataFiles)
            {
                try
                {
                    // 判断文件类型
                    if (dataFile.EndsWith(".csv"))
                    {
                        // CSV文件处理
                        DataTable csvTable = ReadCsvToDataTable(dataFile);
                        csvTable.TableName = Path.GetFileNameWithoutExtension(dataFile);
                        try
                        {
                            GenerateSOBySingleSheet(csvTable);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"CSV表{csvTable.TableName}生成数据时出错，错误{e}");
                            throw;
                        }
                    }
                    else
                    {
                        // Excel文件处理
                        using (FileStream stream = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                }
                catch (Exception e)
                {
                    Debug.LogError($"数据文件生成SO失败: {dataFile}");
                    throw e;
                }
            }
            // AssetDatabase.Refresh();

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
                        case "PathGUID":
                        case "AssetReference":
                            // AssetReference类型：使用Addressables的AssetReference
                            string pathGuid = dataTable.Rows[row][col].ToString();
                            if (!string.IsNullOrEmpty(pathGuid))
                            {
                                // 验证GUID是否有效
                                string assetPath = AssetDatabase.GUIDToAssetPath(pathGuid);
                                if (string.IsNullOrEmpty(assetPath))
                                {
                                    Debug.LogWarning($"GUID '{pathGuid}' 无效或资源不存在");
                                }

                                // 使用构造函数创建AssetReference实例
                                var assetRef = new AssetReference(pathGuid);
                                curData.Add(info[col].name, assetRef);
                            }
                            else
                            {
                                // 创建空的AssetReference（使用空字符串构造）
                                var assetRef = new AssetReference("");
                                curData.Add(info[col].name, assetRef);
                            }
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
            var assembly = Assembly.Load(nameSpaceOfData);
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

            // 在指定目录下创建ScriptableObject文件
            string path = $"{soFilePath}/{data.GetType().Name}Data.asset";
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
                var assembly = Assembly.Load(nameSpaceOfData);
                baseClassType = assembly.GetType(baseClassName);
                baseFields = baseClassType.GetFields();
            }

            // 检查是否需要 Addressables 命名空间（PathGUID会映射为AssetReference）
            bool needsAddressables = paramsArray.Any(info =>
                info.type != null && (info.type.Contains("AssetReference") || info.type.Contains("PathGUID")));

            // 生成 C# 文件内容
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\n");

            // 如果需要 AssetReference，添加 Addressables 命名空间
            if (needsAddressables)
            {
                sb.AppendLine($"using UnityEngine.AddressableAssets;\r\n");
            }

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
                        // 将PathGUID类型映射为AssetReference
                        string typeName = curInfo.type == "PathGUID" ? "AssetReference" : curInfo.type;
                        sb.AppendLine($"    public {typeName} {curInfo.name};");
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
                        // 将PathGUID类型映射为AssetReference
                        string typeName = curInfo.type == "PathGUID" ? "AssetReference" : curInfo.type;
                        sb.AppendLine($"    public {typeName} {curInfo.name};");
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

        // ==================== 数据表编辑器功能 ====================

        [BoxGroup("Editor", ShowLabel = true, LabelText = "数据表编辑器")]
        [ValueDropdown("GetAvailableFiles")]
        [LabelText("选择文件")]
        [OnValueChanged("OnFileSelected")]
        public string selectedFile;

        [BoxGroup("Editor")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("当前文件")]
        private string currentFileName => string.IsNullOrEmpty(currentFilePath) ? "未选择" : Path.GetFileName(currentFilePath);

        [BoxGroup("Editor")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("数据统计")]
        private string dataStats => $"{tableData.Count} 行 x {columnCount} 列";

        private string currentFilePath;
        private int columnCount = 0;
        private List<List<string>> tableData = new List<List<string>>();
        private Vector2 scrollPosition = Vector2.zero;
        private int columnWidth = 150;
        private int selectedRow = -1;
        private int selectedCol = -1;

        // 区域选择相关
        private int selectionStartRow = -1;
        private int selectionStartCol = -1;
        private int selectionEndRow = -1;
        private int selectionEndCol = -1;
        private bool isDraggingSelection = false;

        private IEnumerable GetAvailableFiles()
        {
            if (string.IsNullOrEmpty(excelPath) || !Directory.Exists(excelPath))
            {
                return new ValueDropdownList<string>();
            }

            var files = new ValueDropdownList<string>();

            // 获取所有CSV和Excel文件
            string[] csvFiles = Directory.GetFiles(excelPath, "*.csv");
            string[] xlsxFiles = Directory.GetFiles(excelPath, "*.xlsx");
            string[] xlsFiles = Directory.GetFiles(excelPath, "*.xls");

            foreach (var file in csvFiles.Concat(xlsxFiles).Concat(xlsFiles))
            {
                if (!file.Contains("~")) // 排除临时文件
                {
                    string fileName = Path.GetFileName(file);
                    files.Add(fileName, file);
                }
            }

            return files;
        }

        private void OnFileSelected()
        {
            if (string.IsNullOrEmpty(selectedFile))
                return;

            currentFilePath = selectedFile;
            LoadFileForEditor(currentFilePath);
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/FileOps")]
        [Button("刷新文件列表", ButtonSizes.Medium)]
        private void RefreshFileList()
        {
            // 触发下拉列表刷新
            selectedFile = null;
            currentFilePath = null;
            tableData.Clear();
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/FileOps")]
        [Button("重命名文件", ButtonSizes.Medium)]
        [GUIColor(1f, 1f, 0.5f, 1f)]
        private void RenameFile()
        {
            if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
            {
                Debug.LogError("没有选择有效的文件！");
                return;
            }

            string directory = Path.GetDirectoryName(currentFilePath);
            string extension = Path.GetExtension(currentFilePath);
            string oldFileName = Path.GetFileNameWithoutExtension(currentFilePath);

            // 使用EditorUtility.SaveFilePanel获取新文件名
            string newFilePath = EditorUtility.SaveFilePanel(
                "重命名文件",
                directory,
                oldFileName,
                extension.TrimStart('.')
            );

            if (string.IsNullOrEmpty(newFilePath))
            {
                Debug.Log("取消重命名操作");
                return;
            }

            string newFileName = Path.GetFileNameWithoutExtension(newFilePath);

            if (newFileName == oldFileName && Path.GetDirectoryName(newFilePath) == directory)
            {
                Debug.Log("文件名未更改");
                return;
            }

            // 检查新文件是否已存在
            if (File.Exists(newFilePath) && newFilePath != currentFilePath)
            {
                if (!EditorUtility.DisplayDialog("文件已存在", $"文件 {Path.GetFileName(newFilePath)} 已存在，是否覆盖？", "是", "否"))
                {
                    Debug.Log("取消重命名操作");
                    return;
                }
            }

            try
            {
                // 先保存当前更改
                if (tableData.Count > 0 && currentFilePath.EndsWith(".csv"))
                {
                    SaveCSV(currentFilePath);
                }

                // 重命名/移动文件
                if (File.Exists(newFilePath) && newFilePath != currentFilePath)
                {
                    File.Delete(newFilePath);
                }
                File.Move(currentFilePath, newFilePath);

                Debug.Log($"文件已重命名: {Path.GetFileName(currentFilePath)} -> {Path.GetFileName(newFilePath)}");

                // 更新当前文件路径
                currentFilePath = newFilePath;
                selectedFile = newFilePath;

                // 保存最后打开的文件路径
                PlayerPrefs.SetString("DataEditor_LastFile", newFilePath);
                PlayerPrefs.Save();

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"重命名文件失败: {e.Message}");
            }
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/FileOps")]
        [Button("清除数据", ButtonSizes.Medium)]
        [GUIColor(1f, 0.5f, 0.5f, 1f)]
        private void ClearData()
        {
            if (tableData.Count <= 4)
            {
                Debug.LogWarning("没有数据可清除（仅有表头）");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认清除数据", "此操作将清除所有数据行（保留表头），是否继续？", "确定", "取消"))
            {
                return;
            }

            // 保留前4行表头，删除其余数据
            while (tableData.Count > 4)
            {
                tableData.RemoveAt(4);
            }

            Debug.Log("数据已清除，保留了表头信息");
        }

        [BoxGroup("Editor")]
        [Button("新建CSV文件", ButtonSizes.Large)]
        [GUIColor(0.5f, 1f, 1f, 1f)]
        private void CreateNewCSV()
        {
            if (excelPath.IsNullOrEmpty())
            {
                Debug.LogError("请先选择数据表路径！");
                return;
            }

            string targetPath = excelPath;
            // 如果路径包含 "Assets"，则提取Assets后的路径
            if (excelPath.Contains("Assets"))
            {
                targetPath = excelPath.Substring(excelPath.IndexOf("Assets"));
            }

            // 确保目录存在
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // 生成默认文件名
            string defaultFileName = "NewData";
            string filePath = Path.Combine(targetPath, $"{defaultFileName}.csv");
            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(targetPath, $"{defaultFileName}{counter}.csv");
                counter++;
            }

            // 创建带元数据的CSV模板
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("type=NewData|base=,,,");
            csvContent.AppendLine("字段1,字段2,字段3,字段4");
            csvContent.AppendLine("field1,field2,field3,field4");
            csvContent.AppendLine("int,string,float,bool");

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
            Debug.Log($"新CSV文件已创建: {filePath}");

            AssetDatabase.Refresh();

            // 自动加载新创建的文件
            selectedFile = filePath;
            currentFilePath = filePath;
            LoadFileForEditor(currentFilePath);
        }

        [BoxGroup("Editor")]
        [Button("保存文件", ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f, 1f)]
        private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                Debug.LogError("没有选择文件！");
                return;
            }

            if (currentFilePath.EndsWith(".csv"))
            {
                SaveCSV(currentFilePath);
            }
            else
            {
                Debug.LogWarning("目前仅支持保存CSV文件，Excel文件请转换为CSV后编辑");
            }
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Row")]
        [LabelText("行号")]
        [LabelWidth(40)]
        public int targetRow = 1;

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Row")]
        [Button("在此行之前插入", ButtonSizes.Medium)]
        private void InsertRowBefore()
        {
            InsertRowAt(targetRow - 1);
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Row")]
        [Button("删除此行", ButtonSizes.Medium)]
        [GUIColor(1f, 0.5f, 0.5f, 1f)]
        private void DeleteRow()
        {
            DeleteRowAt(targetRow - 1);
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Row")]
        [Button("新建一行（末尾）", ButtonSizes.Medium)]
        [GUIColor(0.5f, 1f, 0.5f, 1f)]
        private void AddRowAtEnd()
        {
            var newRow = new List<string>();
            for (int i = 0; i < columnCount; i++)
            {
                newRow.Add("");
            }
            tableData.Add(newRow);
            Debug.Log($"在末尾添加新行，当前共{tableData.Count}行");
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Col")]
        [LabelText("列号")]
        [LabelWidth(40)]
        public int targetCol = 1;

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Col")]
        [Button("在此列之前插入", ButtonSizes.Medium)]
        private void InsertColumnBefore()
        {
            InsertColumnAt(targetCol - 1);
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Col")]
        [Button("删除此列", ButtonSizes.Medium)]
        [GUIColor(1f, 0.5f, 0.5f, 1f)]
        private void DeleteColumn()
        {
            DeleteColumnAt(targetCol - 1);
        }

        [BoxGroup("Editor")]
        [HorizontalGroup("Editor/Col")]
        [Button("新建一列（末尾）", ButtonSizes.Medium)]
        [GUIColor(0.5f, 1f, 0.5f, 1f)]
        private void AddColumnAtEnd()
        {
            columnCount++;
            foreach (var row in tableData)
            {
                row.Add("");
            }
            Debug.Log($"在末尾添加新列，当前共{columnCount}列");
        }

        // 辅助方法：在指定索引处插入行
        private void InsertRowAt(int index)
        {
            if (index < 0 || index > tableData.Count)
            {
                Debug.LogWarning($"行索引超出范围！应在0-{tableData.Count}之间");
                return;
            }

            var newRow = new List<string>();
            for (int i = 0; i < columnCount; i++)
            {
                newRow.Add("");
            }
            tableData.Insert(index, newRow);
            Debug.Log($"在第{index + 1}行之前插入新行");
        }

        // 辅助方法：删除指定索引的行
        private void DeleteRowAt(int index)
        {
            if (index < 0 || index >= tableData.Count)
            {
                Debug.LogWarning($"行索引超出范围！应在0-{tableData.Count - 1}之间");
                return;
            }
            if (index < 4)
            {
                Debug.LogWarning("不能删除前4行元数据！");
                return;
            }

            tableData.RemoveAt(index);
            Debug.Log($"已删除第{index + 1}行");
        }

        // 辅助方法：在指定索引处插入列
        private void InsertColumnAt(int index)
        {
            if (index < 0 || index > columnCount)
            {
                Debug.LogWarning($"列索引超出范围！应在0-{columnCount}之间");
                return;
            }

            columnCount++;
            foreach (var row in tableData)
            {
                row.Insert(index, "");
            }
            Debug.Log($"在第{index + 1}列之前插入新列");
        }

        // 辅助方法：删除指定索引的列
        private void DeleteColumnAt(int index)
        {
            if (index < 0 || index >= columnCount)
            {
                Debug.LogWarning($"列索引超出范围！应在0-{columnCount - 1}之间");
                return;
            }
            if (columnCount <= 1)
            {
                Debug.LogWarning("至少需要保留一列！");
                return;
            }

            columnCount--;
            foreach (var row in tableData)
            {
                if (row.Count > index)
                {
                    row.RemoveAt(index);
                }
            }
            Debug.Log($"已删除第{index + 1}列");
        }

        [BoxGroup("Editor", ShowLabel = true, LabelText = "批量资源导入")]
        [OnInspectorGUI]
        private void DrawAssetDropArea()
        {
            EditorGUILayout.Space(5);

            // 绘制拖拽区域
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "将Unity资源拖拽到此处\n自动创建/更新PathGUID列", EditorStyles.helpBox);

            // 处理拖拽事件
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                        {
                            HandleDroppedAsset(draggedObject);
                        }
                    }
                    Event.current.Use();
                    break;
            }
        }

        private void HandleDroppedAsset(UnityEngine.Object asset)
        {
            if (tableData.Count < 4)
            {
                Debug.LogError("表格数据不完整！需要至少4行表头数据");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning($"无法获取资源路径: {asset.name}");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // 检查key列是否存在（先创建key列）
            int keyColumnIndex = FindKeyColumn();

            if (keyColumnIndex == -1)
            {
                // 创建key列
                keyColumnIndex = CreateKeyColumn();
                Debug.Log($"已创建key列（第{keyColumnIndex + 1}列）");
            }

            // 检查PathGUID列是否存在（后创建PathGUID列）
            int pathGuidColumnIndex = FindPathGUIDColumn();

            if (pathGuidColumnIndex == -1)
            {
                // 创建PathGUID列
                pathGuidColumnIndex = CreatePathGUIDColumn();
                Debug.Log($"已创建PathGUID列（第{pathGuidColumnIndex + 1}列）");
            }

            // 添加新数据行
            var newRow = new List<string>();
            for (int i = 0; i < columnCount; i++)
            {
                if (i == pathGuidColumnIndex)
                {
                    newRow.Add(guid);
                }
                else if (i == keyColumnIndex)
                {
                    newRow.Add(assetName);
                }
                else
                {
                    newRow.Add("");
                }
            }
            tableData.Add(newRow);

            Debug.Log($"已添加资源: {assetName} (GUID: {guid})");
        }

        private int FindPathGUIDColumn()
        {
            if (tableData.Count < 3)
                return -1;

            // 在第3行（字段名）中查找pathGuid列
            for (int col = 0; col < tableData[2].Count; col++)
            {
                if (tableData[2][col].ToLower() == "pathguid")
                {
                    return col;
                }
            }

            return -1;
        }

        private int CreatePathGUIDColumn()
        {
            // 在末尾添加新列
            columnCount++;

            // 为每一行添加新列
            for (int row = 0; row < tableData.Count; row++)
            {
                if (row == 1)
                {
                    // 第2行：中文标签
                    tableData[row].Add("资源GUID");
                }
                else if (row == 2)
                {
                    // 第3行：字段名
                    tableData[row].Add("pathGuid");
                }
                else if (row == 3)
                {
                    // 第4行：字段类型
                    tableData[row].Add("PathGUID");
                }
                else
                {
                    tableData[row].Add("");
                }
            }

            return columnCount - 1;
        }

        private int FindKeyColumn()
        {
            if (tableData.Count < 3)
                return -1;

            // 在第3行（字段名）中查找key列
            for (int col = 0; col < tableData[2].Count; col++)
            {
                if (tableData[2][col].ToLower() == "key")
                {
                    return col;
                }
            }

            return -1;
        }

        private int CreateKeyColumn()
        {
            // 在末尾添加新列
            columnCount++;

            // 为每一行添加新列
            for (int row = 0; row < tableData.Count; row++)
            {
                if (row == 1)
                {
                    // 第2行：中文标签
                    tableData[row].Add("资源名称");
                }
                else if (row == 2)
                {
                    // 第3行：字段名
                    tableData[row].Add("key");
                }
                else if (row == 3)
                {
                    // 第4行：字段类型
                    tableData[row].Add("string");
                }
                else
                {
                    tableData[row].Add("");
                }
            }

            return columnCount - 1;
        }

        [BoxGroup("Editor", ShowLabel = true, LabelText = "表格数据")]
        [OnInspectorGUI]
        private void DrawExcelTable()
        {
            if (tableData.Count == 0)
            {
                EditorGUILayout.HelpBox("请选择一个文件以开始编辑", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);

            // 列宽控制
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("列宽:", GUILayout.Width(50));
            columnWidth = EditorGUILayout.IntSlider(columnWidth, 80, 400);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 开始滚动视图
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(600));

            // 绘制表头（行号标签和列标签）
            EditorGUILayout.BeginHorizontal();

            // 左上角空白
            Rect cornerRect = GUILayoutUtility.GetRect(50, 20, GUILayout.Width(50));
            EditorGUI.LabelField(cornerRect, "行号", EditorStyles.boldLabel);

            // 列标签（支持右键菜单）
            for (int col = 0; col < columnCount; col++)
            {
                int currentCol = col; // 捕获当前列索引
                Rect colLabelRect = GUILayoutUtility.GetRect(columnWidth, 20, GUILayout.Width(columnWidth));
                EditorGUI.LabelField(colLabelRect, $"列{col + 1}", EditorStyles.boldLabel);

                // 检测列标签右键点击
                if (Event.current.type == EventType.ContextClick && colLabelRect.Contains(Event.current.mousePosition))
                {
                    ShowColumnContextMenu(currentCol);
                    Event.current.Use();
                }
            }

            EditorGUILayout.EndHorizontal();

            // 绘制分隔线
            Rect separatorRect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(separatorRect, new Color(0.5f, 0.5f, 0.5f, 1));

            // 绘制数据行
            for (int row = 0; row < tableData.Count; row++)
            {
                int currentRow = row; // 捕获当前行索引
                EditorGUILayout.BeginHorizontal();

                // 行号（支持右键菜单）
                Color originalColor = GUI.backgroundColor;
                if (row < 4)
                {
                    // 前4行使用不同的背景色（元数据行）
                    GUI.backgroundColor = new Color(0.8f, 0.9f, 1f, 1f);
                }

                Rect rowLabelRect = GUILayoutUtility.GetRect(50, 18, GUILayout.Width(50));
                EditorGUI.LabelField(rowLabelRect, $"{row + 1}");

                // 检测行号右键点击
                if (Event.current.type == EventType.ContextClick && rowLabelRect.Contains(Event.current.mousePosition))
                {
                    ShowRowContextMenu(currentRow);
                    Event.current.Use();
                }

                // 单元格数据
                for (int col = 0; col < columnCount && col < tableData[row].Count; col++)
                {
                    int currentCol = col; // 捕获当前列索引

                    // 检查是否在选择范围内
                    bool isSelected = IsCellSelected(row, col);

                    // 如果在选择范围内，改变背景色
                    if (isSelected)
                    {
                        GUI.backgroundColor = new Color(0.5f, 0.7f, 1f, 0.5f);
                    }
                    else if (row < 4)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.9f, 1f, 1f);
                    }

                    // 绘制TextField并获取Rect
                    Rect cellRect = GUILayoutUtility.GetRect(columnWidth, 18, GUILayout.Width(columnWidth));
                    tableData[row][col] = EditorGUI.TextField(cellRect, tableData[row][col]);

                    // 处理鼠标事件进行选择
                    HandleCellSelection(cellRect, row, col);
                }

                GUI.backgroundColor = originalColor;
                EditorGUILayout.EndHorizontal();
            }

            // 处理键盘事件（复制粘贴）
            HandleKeyboardEvents();

            EditorGUILayout.EndScrollView();
        }

        // 显示行右键菜单
        private void ShowRowContextMenu(int rowIndex)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent($"在第{rowIndex + 1}行之前插入"), false, () => InsertRowAt(rowIndex));
            menu.AddItem(new GUIContent($"在第{rowIndex + 1}行之后插入"), false, () => InsertRowAt(rowIndex + 1));

            if (rowIndex >= 4) // 只有非元数据行才能删除
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent($"删除第{rowIndex + 1}行"), false, () => DeleteRowAt(rowIndex));
            }
            else
            {
                menu.AddSeparator("");
                menu.AddDisabledItem(new GUIContent($"删除第{rowIndex + 1}行（元数据行不可删除）"));
            }

            menu.ShowAsContext();
        }

        // 显示列右键菜单
        private void ShowColumnContextMenu(int colIndex)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent($"在第{colIndex + 1}列之前插入"), false, () => InsertColumnAt(colIndex));
            menu.AddItem(new GUIContent($"在第{colIndex + 1}列之后插入"), false, () => InsertColumnAt(colIndex + 1));

            if (columnCount > 1) // 至少保留一列
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent($"删除第{colIndex + 1}列"), false, () => DeleteColumnAt(colIndex));
            }
            else
            {
                menu.AddSeparator("");
                menu.AddDisabledItem(new GUIContent("删除列（至少保留一列）"));
            }

            menu.ShowAsContext();
        }

        private void LoadFileForEditor(string filePath)
        {
            try
            {
                tableData.Clear();

                if (filePath.EndsWith(".csv"))
                {
                    LoadCSVForEditor(filePath);
                }
                else if (filePath.EndsWith(".xlsx") || filePath.EndsWith(".xls"))
                {
                    LoadExcelForEditor(filePath);
                }

                // 保存最后打开的文件路径
                PlayerPrefs.SetString("DataEditor_LastFile", filePath);
                PlayerPrefs.Save();

                Debug.Log($"已加载文件: {Path.GetFileName(filePath)}, 共 {tableData.Count} 行");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载文件失败: {e.Message}");
            }
        }

        private void LoadCSVForEditor(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                bool isFirstLine = true;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] cells = ParseCsvLineForEditor(line);

                    if (isFirstLine)
                    {
                        columnCount = cells.Length;
                        isFirstLine = false;
                    }

                    tableData.Add(new List<string>(cells));
                }
            }
        }

        private void LoadExcelForEditor(string filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                DataSet dataSet = excelReader.AsDataSet();

                if (dataSet.Tables.Count > 0)
                {
                    DataTable sheet = dataSet.Tables[0];
                    columnCount = sheet.Columns.Count;

                    for (int rowIndex = 0; rowIndex < sheet.Rows.Count; rowIndex++)
                    {
                        var row = new List<string>();
                        for (int colIndex = 0; colIndex < sheet.Columns.Count; colIndex++)
                        {
                            var cellValue = sheet.Rows[rowIndex][colIndex];
                            row.Add(cellValue != DBNull.Value ? cellValue.ToString() : "");
                        }
                        tableData.Add(row);
                    }
                }

                excelReader.Close();
            }
        }

        private void SaveCSV(string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (var row in tableData)
                {
                    List<string> escapedCells = new List<string>();
                    foreach (var cell in row)
                    {
                        // 如果单元格包含逗号、引号或换行符，需要用引号包裹
                        if (cell.Contains(",") || cell.Contains("\"") || cell.Contains("\n"))
                        {
                            escapedCells.Add("\"" + cell.Replace("\"", "\"\"") + "\"");
                        }
                        else
                        {
                            escapedCells.Add(cell);
                        }
                    }
                    sb.AppendLine(string.Join(",", escapedCells));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                Debug.Log($"文件已保存: {Path.GetFileName(filePath)}");
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"保存文件失败: {e.Message}");
            }
        }

        private string[] ParseCsvLineForEditor(string line)
        {
            if (string.IsNullOrEmpty(line))
                return new string[0];

            List<string> result = new List<string>();
            StringBuilder currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 双引号转义
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }

        // 区域选择相关方法
        private bool IsCellSelected(int row, int col)
        {
            if (selectionStartRow == -1 || selectionEndRow == -1)
                return false;

            int minRow = Mathf.Min(selectionStartRow, selectionEndRow);
            int maxRow = Mathf.Max(selectionStartRow, selectionEndRow);
            int minCol = Mathf.Min(selectionStartCol, selectionEndCol);
            int maxCol = Mathf.Max(selectionStartCol, selectionEndCol);

            return row >= minRow && row <= maxRow && col >= minCol && col <= maxCol;
        }

        private void HandleCellSelection(Rect cellRect, int row, int col)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (cellRect.Contains(evt.mousePosition) && evt.button == 0)
                    {
                        selectionStartRow = row;
                        selectionStartCol = col;
                        selectionEndRow = row;
                        selectionEndCol = col;
                        isDraggingSelection = true;
                        evt.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDraggingSelection && cellRect.Contains(evt.mousePosition))
                    {
                        selectionEndRow = row;
                        selectionEndCol = col;
                        evt.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (evt.button == 0)
                    {
                        isDraggingSelection = false;
                    }
                    break;
            }
        }

        private void HandleKeyboardEvents()
        {
            Event evt = Event.current;

            if (evt.type == EventType.KeyDown)
            {
                // Ctrl+C 复制
                if (evt.control && evt.keyCode == KeyCode.C)
                {
                    CopySelection();
                    evt.Use();
                }
                // Ctrl+V 粘贴
                else if (evt.control && evt.keyCode == KeyCode.V)
                {
                    PasteSelection();
                    evt.Use();
                }
                // Ctrl+S 保存
                else if (evt.control && evt.keyCode == KeyCode.S)
                {
                    SaveFile();
                    evt.Use();
                }
            }
        }

        private void CopySelection()
        {
            if (selectionStartRow == -1 || selectionEndRow == -1)
            {
                Debug.LogWarning("没有选中任何单元格！");
                return;
            }

            int minRow = Mathf.Min(selectionStartRow, selectionEndRow);
            int maxRow = Mathf.Max(selectionStartRow, selectionEndRow);
            int minCol = Mathf.Min(selectionStartCol, selectionEndCol);
            int maxCol = Mathf.Max(selectionStartCol, selectionEndCol);

            StringBuilder sb = new StringBuilder();

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (row < tableData.Count && col < tableData[row].Count)
                    {
                        sb.Append(tableData[row][col]);
                    }

                    if (col < maxCol)
                    {
                        sb.Append("\t");
                    }
                }
                if (row < maxRow)
                {
                    sb.AppendLine();
                }
            }

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log($"已复制 {maxRow - minRow + 1} 行 x {maxCol - minCol + 1} 列");
        }

        private void PasteSelection()
        {
            string clipboardText = EditorGUIUtility.systemCopyBuffer;

            if (string.IsNullOrEmpty(clipboardText))
            {
                Debug.LogWarning("剪贴板为空！");
                return;
            }

            if (selectionStartRow == -1)
            {
                Debug.LogWarning("请先选择一个起始单元格！");
                return;
            }

            // 解析剪贴板内容
            string[] lines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int startRow = Mathf.Min(selectionStartRow, selectionEndRow);
            int startCol = Mathf.Min(selectionStartCol, selectionEndCol);

            for (int i = 0; i < lines.Length; i++)
            {
                int targetRow = startRow + i;

                // 如果目标行不存在，自动添加新行
                while (targetRow >= tableData.Count)
                {
                    var newRow = new List<string>();
                    for (int c = 0; c < columnCount; c++)
                    {
                        newRow.Add("");
                    }
                    tableData.Add(newRow);
                }

                string[] cells = lines[i].Split('\t');

                for (int j = 0; j < cells.Length; j++)
                {
                    int targetCol = startCol + j;

                    // 如果目标列不存在，自动添加新列
                    if (targetCol >= columnCount)
                    {
                        columnCount = targetCol + 1;
                        foreach (var row in tableData)
                        {
                            while (row.Count < columnCount)
                            {
                                row.Add("");
                            }
                        }
                    }

                    // 确保当前行有足够的列
                    while (tableData[targetRow].Count <= targetCol)
                    {
                        tableData[targetRow].Add("");
                    }

                    tableData[targetRow][targetCol] = cells[j];
                }
            }

            Debug.Log($"已粘贴 {lines.Length} 行数据");
        }
    }

    public class SteamDebugMenu : ScriptableObject
    {
        [BoxGroup("Steam", ShowLabel = true, LabelText = "Steam调试工具")]
        [Button("导出对局数据", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Steam/ButtonGroup", PaddingLeft = 0)]
        public void ExportGameRecord()
        {
            Debug.Log("导出对局数据功能待实现");
        }

        [Button("清除成就", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Steam/ButtonGroup", PaddingLeft = 0)]
        [GUIColor(1f, 0.5f, 0.5f, 1f)]
        public void CleanAchievements()
        {
            // LocalDataManager.Instance.CleanAchievements();
            Debug.Log("清除成就功能待实现");
        }

        [BoxGroup("Steam", ShowLabel = true, LabelText = "存档管理")]
        [Button("打开存档文件夹", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Steam/SaveGroup", PaddingLeft = 0)]
        [GUIColor(0.5f, 1f, 0.5f, 1f)]
        public void OpenSaveFolder()
        {
#if UNITY_EDITOR
            string folderPath = UnityEngine.Application.persistentDataPath;

            if (System.IO.Directory.Exists(folderPath))
            {
                // Windows平台
                System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
                Debug.Log($"已打开存档文件夹: {folderPath}");
            }
            else
            {
                Debug.LogError($"存档文件夹不存在: {folderPath}");
            }
#else
            Debug.LogWarning("此功能仅在编辑器模式下可用");
#endif
        }

        [Button("打印存档路径", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Steam/SaveGroup", PaddingLeft = 0)]
        public void PrintSavePath()
        {
            string savePath = UnityEngine.Application.persistentDataPath;
            Debug.Log($"=== 存档路径信息 ===");
            Debug.Log($"PersistentDataPath: {savePath}");
            Debug.Log($"DataPath: {UnityEngine.Application.dataPath}");
            Debug.Log($"StreamingAssetsPath: {UnityEngine.Application.streamingAssetsPath}");

            // 复制路径到剪贴板
            GUIUtility.systemCopyBuffer = savePath;
            Debug.Log("存档路径已复制到剪贴板！");
        }

        [Button("清空所有存档数据", ButtonSizes.Medium, Stretch = false)]
        [HorizontalGroup("Steam/SaveGroup", PaddingLeft = 0)]
        [GUIColor(1f, 0.3f, 0.3f, 1f)]
        public void ClearAllSaveData()
        {
            if (EditorUtility.DisplayDialog("警告", "确定要清空所有存档数据吗？此操作不可撤销！", "确定", "取消"))
            {
                string savePath = UnityEngine.Application.persistentDataPath;
                string saveFile = System.IO.Path.Combine(savePath, "SaveData.json");
                string backupFile = System.IO.Path.Combine(savePath, "SaveData.backup.json");

                try
                {
                    if (System.IO.File.Exists(saveFile))
                    {
                        System.IO.File.Delete(saveFile);
                        Debug.Log($"已删除存档文件: {saveFile}");
                    }

                    if (System.IO.File.Exists(backupFile))
                    {
                        System.IO.File.Delete(backupFile);
                        Debug.Log($"已删除备份文件: {backupFile}");
                    }

                    Debug.Log("存档数据已清空！");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"清空存档失败: {e.Message}");
                }
            }
        }
    }

    public class DataConvertWindow : OdinMenuEditorWindow
    {
        public DataConvertMenu dataConvertMenu;
        public SteamDebugMenu steamDebugMenu;
        // public WaveConfigSO waveConfigSO;

        [MenuItem("Tools/数据转换 %t")]
        private static void OpenWindow()
        {
            GetWindow<DataConvertWindow>().Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            dataConvertMenu = ScriptableObject.CreateInstance<DataConvertMenu>();
            steamDebugMenu = ScriptableObject.CreateInstance<SteamDebugMenu>();
            // waveConfigSO = AssetDatabase.LoadAssetAtPath<WaveConfigSO>("Assets/Resources/SO/WaveConfigSO.asset");

            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
{
    { "数据表管理",                             dataConvertMenu,                           EditorIcons.GridBlocks  },
    { "Steam调试",                             steamDebugMenu,                            EditorIcons.SettingsCog  },
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