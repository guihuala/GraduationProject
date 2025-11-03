using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GuiFramework
{
    /// <summary>
    /// SCFrame中的配表数据导出器，修改为导出为JSON格式
    /// </summary>
    public static class SCExcelExporter
    {
        public const string GAME_EXCEL_PATH = "Assets/Resources/RefData/Excels";
        public const string GAME_JSON_PATH = "Assets/Resources/RefData/ExportJson";  // 修改路径为JSON
        public const int TITLE_START_INDEX = 0;

        /// <summary>
        /// 导出所有的excel表 表在GAME_EXCEL_PATH里，修改为导出为JSON
        /// </summary>
        [MenuItem("Excel导出/导出全部的Excel为JSON")]
        public static void ExportAllExcelsAsJson()
        {
            bool hasImported = false;
            DirectoryInfo direction = new DirectoryInfo(GAME_EXCEL_PATH);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                // 查找excel的后缀
                if (Path.GetExtension(files[i].FullName) == ".xls" || Path.GetExtension(files[i].FullName) == ".xlsx")
                {
                    string excelName = Path.GetFileName(files[i].FullName);

                    if (excelName.StartsWith("~$"))
                    {
                        continue;
                    }

                    ExportExcelAsJson(excelName);

                    hasImported = true;
                }
            }

            if (!hasImported)
            {
                Debug.LogError("没有找到可以导出的Excel！！！");
            }
            else
            {
                Debug.Log("所有的Excel都导出成功！！！");
            }
        }

        /// <summary>
        /// 导出单个Excel表为JSON
        /// </summary>
        /// <param name="_excelName"></param>
        public static void ExportExcelAsJson(string _excelName)
        {
            string excelPath = GAME_EXCEL_PATH + "/" + _excelName;

            IWorkbook workbook = CreateWorkbook(excelPath);
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;

            var sheetData = new List<Dictionary<string, string>>(); // 存储每个sheet的JSON数据

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                sheet = workbook.GetSheetAt(i);
                if (sheet == null)
                    continue;

                var columnNames = new List<string>();

                // 获取标题行
                row = sheet.GetRow(TITLE_START_INDEX);
                for (int k = 0; k < row.LastCellNum; k++)
                {
                    cell = row.GetCell(k);
                    columnNames.Add(cell?.ToString() ?? "");
                }

                // 获取数据行
                for (int j = TITLE_START_INDEX + 1; j <= sheet.LastRowNum; j++)
                {
                    row = sheet.GetRow(j);
                    if (row == null)
                        continue;

                    var rowData = new Dictionary<string, string>();
                    for (int k = 0; k < row.LastCellNum; k++)
                    {
                        cell = row.GetCell(k);
                        if (cell != null)
                        {
                            rowData[columnNames[k]] = cell.ToString();
                        }
                    }

                    if (rowData.Count > 0)
                    {
                        sheetData.Add(rowData);
                    }
                }

                // 保存为JSON
                string json = JsonConvert.SerializeObject(sheetData, Formatting.Indented);
                string jsonFilePath = GAME_JSON_PATH + "/" + workbook.GetSheetName(i) + ".json";
                File.WriteAllText(jsonFilePath, json);

                Debug.Log($"导出{_excelName}的 {workbook.GetSheetName(i)} sheet 为 JSON 成功！");
            }
        }

        /// <summary>
        /// 创建工作簿
        /// </summary>
        /// <param name="_excelPath"></param>
        /// <returns></returns>
        private static IWorkbook CreateWorkbook(string _excelPath)
        {
            using (FileStream stream = File.Open(_excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Path.GetExtension(_excelPath) == ".xls")
                {
                    return new HSSFWorkbook(stream);
                }
                else
                {
                    return new XSSFWorkbook(stream);
                }
            }
        }
    }
}
