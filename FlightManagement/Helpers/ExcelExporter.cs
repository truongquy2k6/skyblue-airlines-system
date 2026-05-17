using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FlightManagement.Helpers
{
    public static class ExcelExporter
    {
        public static void ExportDataGrid(DataGrid grid, string title, string defaultFileName)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName;
            bool isCsv = filePath.ToLower().EndsWith(".csv");

            if (isCsv)
            {
                ExportToCsv(grid, title, filePath);
                MessageBox.Show("Xuất CSV thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Thử xuất file Excel thật (.xlsx) qua COM Interop
            Type excelType = Type.GetTypeFromProgID("Excel.Application");
            if (excelType == null)
            {
                // Fallback sang CSV nếu không có Excel
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportToCsv(grid, title, filePath);
                MessageBox.Show("Hệ thống chưa cài đặt Microsoft Excel. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic excel = null;
            try
            {
                excel = Activator.CreateInstance(excelType);
                excel.Visible = false;
                dynamic workbooks = excel.Workbooks;
                dynamic workbook = workbooks.Add(1);
                dynamic worksheet = workbook.Sheets[1];
                worksheet.Name = "BaoCao";

                // Dòng tiêu đề lớn
                worksheet.Cells[1, 1] = title;
                dynamic titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, grid.Columns.Count]];
                titleRange.Merge();
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // #1565C0
                titleRange.HorizontalAlignment = -4108; // Center (xlCenter)

                // Xuất tiêu đề cột
                int colIndex = 1;
                foreach (var col in grid.Columns)
                {
                    if (col.Header != null)
                    {
                        worksheet.Cells[3, colIndex] = col.Header.ToString();
                        colIndex++;
                    }
                }
                dynamic headerRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3, grid.Columns.Count]];
                headerRange.Font.Bold = true;
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // #1565C0
                headerRange.HorizontalAlignment = -4108; // Center (xlCenter)

                // Xuất dòng dữ liệu
                var itemsSource = grid.ItemsSource as System.Data.DataView;
                if (itemsSource != null)
                {
                    int rowIndex = 4;
                    foreach (DataRowView rowView in itemsSource)
                    {
                        int colIdx = 1;
                        foreach (var col in grid.Columns)
                        {
                            string val = "";
                            if (col is DataGridTextColumn textCol)
                            {
                                string bindingPath = (textCol.Binding as System.Windows.Data.Binding)?.Path?.Path;
                                val = bindingPath != null ? rowView[bindingPath]?.ToString() ?? "" : "";
                            }
                            else if (col is DataGridTemplateColumn templateCol)
                            {
                                if (col.Header.ToString().Contains("Chênh lệch"))
                                {
                                    val = rowView["ChenhLech"]?.ToString() ?? "";
                                }
                            }

                            worksheet.Cells[rowIndex, colIdx] = val;
                            colIdx++;
                        }
                        rowIndex++;
                    }
                }

                // Tự động căn chỉnh độ rộng cột
                dynamic allCells = worksheet.Cells;
                allCells.EntireColumn.AutoFit();

                workbook.SaveAs(filePath, 51); // 51 = xlOpenXMLWorkbook (.xlsx)
                workbook.Close(true);
                MessageBox.Show("Xuất file Excel thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportToCsv(grid, title, filePath);
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                if (excel != null)
                {
                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }
            }
        }

        public static void ExportDataTable(DataTable dt, string title, string defaultFileName)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName;
            bool isCsv = filePath.ToLower().EndsWith(".csv");

            if (isCsv)
            {
                ExportDataTableToCsv(dt, title, filePath);
                MessageBox.Show("Xuất CSV thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Type excelType = Type.GetTypeFromProgID("Excel.Application");
            if (excelType == null)
            {
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportDataTableToCsv(dt, title, filePath);
                MessageBox.Show("Hệ thống chưa cài đặt Microsoft Excel. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic excel = null;
            try
            {
                excel = Activator.CreateInstance(excelType);
                excel.Visible = false;
                dynamic workbooks = excel.Workbooks;
                dynamic workbook = workbooks.Add(1);
                dynamic worksheet = workbook.Sheets[1];
                worksheet.Name = "BaoCao";

                // Tiêu đề lớn
                worksheet.Cells[1, 1] = title;
                dynamic titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, dt.Columns.Count]];
                titleRange.Merge();
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // #1565C0
                titleRange.HorizontalAlignment = -4108; // Center

                // Xuất tiêu đề cột
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    worksheet.Cells[3, col + 1] = dt.Columns[col].ColumnName;
                }
                dynamic headerRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3, dt.Columns.Count]];
                headerRange.Font.Bold = true;
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // #1565C0
                headerRange.HorizontalAlignment = -4108; // Center

                // Xuất dòng dữ liệu
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 4, col + 1] = dt.Rows[row][col]?.ToString() ?? "";
                    }
                }

                // Tự căn chỉnh cột
                dynamic allCells = worksheet.Cells;
                allCells.EntireColumn.AutoFit();

                workbook.SaveAs(filePath, 51); // 51 = xlOpenXMLWorkbook (.xlsx)
                workbook.Close(true);
                MessageBox.Show("Xuất file Excel thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportDataTableToCsv(dt, title, filePath);
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                if (excel != null)
                {
                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }
            }
        }

        private static void ExportToCsv(DataGrid grid, string title, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine();

            List<string> headers = new List<string>();
            foreach (var col in grid.Columns)
            {
                if (col.Header != null)
                    headers.Add(col.Header.ToString());
            }
            sb.AppendLine(string.Join(",", headers));

            var itemsSource = grid.ItemsSource as System.Data.DataView;
            if (itemsSource != null)
            {
                foreach (DataRowView rowView in itemsSource)
                {
                    List<string> cells = new List<string>();
                    foreach (var col in grid.Columns)
                    {
                        string val = "";
                        if (col is DataGridTextColumn textCol)
                        {
                            string bindingPath = (textCol.Binding as System.Windows.Data.Binding)?.Path?.Path;
                            val = bindingPath != null ? rowView[bindingPath]?.ToString() ?? "" : "";
                        }
                        else if (col is DataGridTemplateColumn templateCol)
                        {
                            if (col.Header.ToString().Contains("Chênh lệch"))
                            {
                                val = rowView["ChenhLech"]?.ToString() ?? "";
                            }
                        }

                        if (val.Contains(",") || val.Contains("\""))
                        {
                            val = "\"" + val.Replace("\"", "\"\"") + "\"";
                        }
                        cells.Add(val);
                    }
                    sb.AppendLine(string.Join(",", cells));
                }
            }

            WriteBomCsv(filePath, sb.ToString());
        }

        private static void ExportDataTableToCsv(DataTable dt, string title, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine();

            List<string> headers = new List<string>();
            foreach (DataColumn col in dt.Columns)
                headers.Add(col.ColumnName);
            sb.AppendLine(string.Join(",", headers));

            foreach (DataRow row in dt.Rows)
            {
                List<string> cells = new List<string>();
                foreach (var item in row.ItemArray)
                {
                    string val = item?.ToString() ?? "";
                    if (val.Contains(",") || val.Contains("\""))
                        val = "\"" + val.Replace("\"", "\"\"") + "\"";
                    cells.Add(val);
                }
                sb.AppendLine(string.Join(",", cells));
            }

            WriteBomCsv(filePath, sb.ToString());
        }

        private static void WriteBomCsv(string filePath, string content)
        {
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(bom, 0, bom.Length);
                byte[] info = Encoding.UTF8.GetBytes(content);
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
