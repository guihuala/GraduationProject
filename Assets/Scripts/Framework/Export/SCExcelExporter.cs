using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuiFramework
{
    /// <summary>
    /// SCFrame中的配表数据导出器
    /// </summary>
    public static class SCExcelExporter
    {
        public const string GAME_EXCEL_PATH = "Assets/Resources/RefData/Excels";
        public const string GAME_TXT_PATH = "Assets/Resources/RefData/ExportTxt";
        public const int TITLE_START_INDEX = 0;

        /// <summary>
        /// 导出所有的excel表 表在GAME_EXCEL_PATH里
        /// </summary>
        [MenuItem("Excel导出/导出全部的Excel")]
        public static void ExportAllExcels()
        {
            bool hasImported = false;
            DirectoryInfo direction = new DirectoryInfo(GAME_EXCEL_PATH);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                //查找excel的后缀
                if (Path.GetExtension(files[i].FullName) == ".xls" || Path.GetExtension(files[i].FullName) == ".xlsx")
                {
                    string excelName = Path.GetFileName(files[i].FullName);

                    if (excelName.StartsWith("~$"))
                    {
                        continue;
                    }

                    ExportExcel(excelName);

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
        /// 导出Excel表 
        /// </summary>
        /// <param name="_excelName"></param>
        public static void ExportExcel(string _excelName)
        {
            string excelPath = GAME_EXCEL_PATH + "/" + _excelName;

            IWorkbook workbook = CreatWrokbook(excelPath);
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            string cellValue = "";
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                using (FileStream fs = File.Open(GAME_TXT_PATH + "/" + workbook.GetSheetName(i) + ".txt",
                           FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {
                        sheet = workbook.GetSheetAt(i);
                        if (sheet == null)
                            continue;
                        for (int j = TITLE_START_INDEX; j <= sheet.LastRowNum; j++)
                        {
                            row = sheet.GetRow(j);
                            if (row == null)
                                continue;
                            for (int k = 0; k <= row.LastCellNum; k++)
                            {
                                cell = row.GetCell(k);
                                if (cell == null)
                                    continue;
                                cellValue = cell?.ToString() ?? "";
                                sw.Write(cellValue);
                                if (k < row.LastCellNum - 1)
                                    sw.Write("\t");
                            }

                            sw.Write("\n");
                        }
                    }
                }
            }

            Debug.Log("导出" + _excelName + "成功！！！");
        }


        /// <summary>
        /// 创建工作簿
        /// </summary>
        /// <param name="_excelPath"></param>
        /// <returns></returns>
        private static IWorkbook CreatWrokbook(string _excelPath)
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