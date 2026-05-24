using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FlightManagement.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ xuất dữ liệu ra file Excel (.xlsx) hoặc CSV (.csv).
    /// Hỗ trợ cơ chế tự động chuyển đổi (fallback) sang định dạng CSV nếu máy client chưa cài đặt Microsoft Excel.
    /// </summary>
    public static class ExcelExporter
    {
        /// <summary>
        /// Xuất dữ liệu từ một WPF DataGrid ra file Excel hoặc CSV.
        /// </summary>
        /// <param name="grid">DataGrid chứa dữ liệu cần xuất.</param>
        /// <param name="title">Tiêu đề lớn hiển thị ở dòng đầu tiên của file báo cáo.</param>
        /// <param name="defaultFileName">Tên file mặc định gợi ý khi lưu.</param>
        public static void ExportDataGrid(DataGrid grid, string title, string defaultFileName)
        {
            // Cấu hình hộp thoại lưu file (SaveFileDialog)
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                FileName = defaultFileName
            };

            // Nếu người dùng hủy hộp thoại, dừng việc xuất dữ liệu
            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName;
            bool isCsv = filePath.ToLower().EndsWith(".csv");

            // Trường hợp người dùng chủ động chọn lưu dưới định dạng CSV
            if (isCsv)
            {
                ExportToCsv(grid, title, filePath);
                MessageBox.Show("Xuất CSV thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Kiểm tra xem hệ thống đã cài đặt Microsoft Excel hay chưa qua COM Interop
            Type excelType = Type.GetTypeFromProgID("Excel.Application");
            if (excelType == null)
            {
                // Fallback: Tự động chuyển hướng xuất sang CSV nếu máy khách không có Microsoft Excel
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportToCsv(grid, title, filePath);
                MessageBox.Show("Hệ thống chưa cài đặt Microsoft Excel. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic excel = null;
            try
            {
                // Khởi tạo ứng dụng Excel thông qua Late Binding (COM Interop)
                excel = Activator.CreateInstance(excelType);
                excel.Visible = false; // Chạy ẩn danh để tăng hiệu năng và không làm phiền người dùng
                dynamic workbooks = excel.Workbooks;
                dynamic workbook = workbooks.Add(1); // Tạo 1 workbook mới với 1 sheet mặc định
                dynamic worksheet = workbook.Sheets[1];
                worksheet.Name = "BaoCao";

                // Tạo dòng tiêu đề lớn nằm ở trên cùng
                worksheet.Cells[1, 1] = title;
                dynamic titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, grid.Columns.Count]];
                titleRange.Merge(); // Gộp các ô lại thành một dòng lớn bằng số cột của lưới
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // Đặt màu xanh #1565C0 thương hiệu
                titleRange.HorizontalAlignment = -4108; // Căn giữa dòng tiêu đề (xlCenter = -4108)

                // Xuất tiêu đề cho từng cột dữ liệu
                int colIndex = 1;
                foreach (var col in grid.Columns)
                {
                    if (col.Header != null)
                    {
                        worksheet.Cells[3, colIndex] = col.Header.ToString();
                        colIndex++;
                    }
                }
                
                // Định dạng dòng tiêu đề cột: Chữ đậm, nền xanh dương đậm, chữ trắng, căn giữa
                dynamic headerRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3, grid.Columns.Count]];
                headerRange.Font.Bold = true;
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // Màu xanh #1565C0
                headerRange.HorizontalAlignment = -4108; // Căn giữa dòng tiêu đề cột

                // Xuất dữ liệu từng dòng từ DataGrid
                var itemsSource = grid.ItemsSource as System.Data.DataView;
                if (itemsSource != null)
                {
                    int rowIndex = 4; // Dữ liệu thực tế bắt đầu từ dòng thứ 4 trong file Excel
                    foreach (DataRowView rowView in itemsSource)
                    {
                        int colIdx = 1;
                        foreach (var col in grid.Columns)
                        {
                            string val = "";
                            // Trường hợp cột dạng chữ thông thường
                            if (col is DataGridTextColumn textCol)
                            {
                                string bindingPath = (textCol.Binding as System.Windows.Data.Binding)?.Path?.Path;
                                val = bindingPath != null ? rowView[bindingPath]?.ToString() ?? "" : "";
                            }
                            // Trường hợp cột dạng Template (ví dụ: các ô tùy biến hiển thị phức tạp)
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

                // Tự động căn chỉnh độ rộng cột theo độ dài nội dung (AutoFit)
                dynamic allCells = worksheet.Cells;
                allCells.EntireColumn.AutoFit();

                // Lưu file ở định dạng Excel chuẩn
                workbook.SaveAs(filePath, 51); // 51 = mã định dạng xlOpenXMLWorkbook (.xlsx)
                workbook.Close(true);
                MessageBox.Show("Xuất file Excel thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Fallback khi gặp lỗi bất kỳ trong quá trình làm việc với Excel qua COM Interop
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportToCsv(grid, title, filePath);
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Giải phóng tài nguyên COM Interop tránh treo tiến trình Excel trong Task Manager
                if (excel != null)
                {
                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }
            }
        }

        /// <summary>
        /// Xuất dữ liệu từ một DataTable ra file Excel hoặc CSV.
        /// </summary>
        /// <param name="dt">DataTable chứa dữ liệu cần xuất.</param>
        /// <param name="title">Tiêu đề lớn hiển thị ở dòng đầu tiên của file báo cáo.</param>
        /// <param name="defaultFileName">Tên file mặc định gợi ý khi lưu.</param>
        public static void ExportDataTable(DataTable dt, string title, string defaultFileName)
        {
            // Cấu hình hộp thoại lưu file (SaveFileDialog)
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                FileName = defaultFileName
            };

            // Nếu người dùng hủy hộp thoại, dừng việc xuất dữ liệu
            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName;
            bool isCsv = filePath.ToLower().EndsWith(".csv");

            // Trường hợp người dùng chủ động chọn lưu dưới định dạng CSV
            if (isCsv)
            {
                ExportDataTableToCsv(dt, title, filePath);
                MessageBox.Show("Xuất CSV thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Kiểm tra cài đặt Microsoft Excel trên máy tính client
            Type excelType = Type.GetTypeFromProgID("Excel.Application");
            if (excelType == null)
            {
                // Fallback sang CSV nếu không tìm thấy Excel
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportDataTableToCsv(dt, title, filePath);
                MessageBox.Show("Hệ thống chưa cài đặt Microsoft Excel. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic excel = null;
            try
            {
                // Khởi tạo ứng dụng Excel thông qua Late Binding (COM Interop)
                excel = Activator.CreateInstance(excelType);
                excel.Visible = false; // Chạy ẩn danh
                dynamic workbooks = excel.Workbooks;
                dynamic workbook = workbooks.Add(1);
                dynamic worksheet = workbook.Sheets[1];
                worksheet.Name = "BaoCao";

                // Tạo dòng tiêu đề lớn nằm ở trên cùng
                worksheet.Cells[1, 1] = title;
                dynamic titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, dt.Columns.Count]];
                titleRange.Merge();
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // Màu xanh #1565C0
                titleRange.HorizontalAlignment = -4108; // Căn giữa (xlCenter = -4108)

                // Xuất tiêu đề cho từng cột trong DataTable
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    worksheet.Cells[3, col + 1] = dt.Columns[col].ColumnName;
                }
                
                // Định dạng dòng tiêu đề cột
                dynamic headerRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3, dt.Columns.Count]];
                headerRange.Font.Bold = true;
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(21, 101, 192)); // Màu xanh #1565C0
                headerRange.HorizontalAlignment = -4108; // Căn giữa

                // Xuất dữ liệu từng dòng từ DataTable
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 4, col + 1] = dt.Rows[row][col]?.ToString() ?? "";
                    }
                }

                // Tự động căn chỉnh độ rộng cột theo độ dài nội dung (AutoFit)
                dynamic allCells = worksheet.Cells;
                allCells.EntireColumn.AutoFit();

                // Lưu file dưới dạng Excel tiêu chuẩn (.xlsx)
                workbook.SaveAs(filePath, 51); // 51 = xlOpenXMLWorkbook (.xlsx)
                workbook.Close(true);
                MessageBox.Show("Xuất file Excel thành công!", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Fallback khi gặp lỗi trong quá trình làm việc với Excel COM Interop
                filePath = Path.ChangeExtension(filePath, ".csv");
                ExportDataTableToCsv(dt, title, filePath);
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}. Đã tự động xuất ra file CSV thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Giải phóng tài nguyên COM Interop
                if (excel != null)
                {
                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }
            }
        }

        /// <summary>
        /// Hỗ trợ xuất dữ liệu DataGrid ra định dạng CSV (dùng dấu phẩy phân tách).
        /// </summary>
        private static void ExportToCsv(DataGrid grid, string title, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            
            // Thêm tiêu đề lớn ở dòng đầu
            sb.AppendLine(title);
            sb.AppendLine();

            // Lấy tiêu đề các cột
            List<string> headers = new List<string>();
            foreach (var col in grid.Columns)
            {
                if (col.Header != null)
                    headers.Add(col.Header.ToString());
            }
            sb.AppendLine(string.Join(",", headers));

            // Duyệt và lấy thông tin các dòng dữ liệu trong DataGrid
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

                        // Xử lý Escape ký tự đặc biệt đối với dữ liệu chứa dấu phẩy hoặc dấu nháy kép
                        if (val.Contains(",") || val.Contains("\""))
                        {
                            val = "\"" + val.Replace("\"", "\"\"") + "\"";
                        }
                        cells.Add(val);
                    }
                    sb.AppendLine(string.Join(",", cells));
                }
            }

            // Ghi dữ liệu ra file với UTF-8 và Byte Order Mark (BOM) để Excel nhận diện chuẩn tiếng Việt có dấu
            WriteBomCsv(filePath, sb.ToString());
        }

        /// <summary>
        /// Hỗ trợ xuất dữ liệu DataTable ra định dạng CSV (dùng dấu phẩy phân tách).
        /// </summary>
        private static void ExportDataTableToCsv(DataTable dt, string title, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            
            // Thêm tiêu đề lớn ở dòng đầu
            sb.AppendLine(title);
            sb.AppendLine();

            // Lấy danh sách tên cột
            List<string> headers = new List<string>();
            foreach (DataColumn col in dt.Columns)
                headers.Add(col.ColumnName);
            sb.AppendLine(string.Join(",", headers));

            // Duyệt các hàng trong DataTable
            foreach (DataRow row in dt.Rows)
            {
                List<string> cells = new List<string>();
                foreach (var item in row.ItemArray)
                {
                    string val = item?.ToString() ?? "";
                    
                    // Xử lý Escape ký tự đặc biệt đối với dữ liệu chứa dấu phẩy hoặc dấu nháy kép
                    if (val.Contains(",") || val.Contains("\""))
                        val = "\"" + val.Replace("\"", "\"\"") + "\"";
                    cells.Add(val);
                }
                sb.AppendLine(string.Join(",", cells));
            }

            // Ghi dữ liệu ra file với UTF-8 và Byte Order Mark (BOM)
            WriteBomCsv(filePath, sb.ToString());
        }

        /// <summary>
        /// Ghi file CSV mã hóa UTF-8 kèm theo BOM (Byte Order Mark) để đảm bảo không bị lỗi font khi mở bằng Microsoft Excel.
        /// </summary>
        /// <param name="filePath">Đường dẫn file đích cần ghi.</param>
        /// <param name="content">Chuỗi nội dung văn bản CSV.</param>
        private static void WriteBomCsv(string filePath, string content)
        {
            // UTF-8 BOM bytes: EF BB BF
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                // Ghi 3 bytes BOM lên đầu tiên
                fs.Write(bom, 0, bom.Length);
                // Sau đó ghi dữ liệu chuỗi được mã hóa dưới dạng UTF-8
                byte[] info = Encoding.UTF8.GetBytes(content);
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
