using System;
using System.Windows.Forms;

namespace CaroOnline
{
    internal static class Program
    {
        /// <summary>
        /// Điểm nòng cốt để ứng dụng bắt đầu chạy.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Lệnh này bảo C# hãy mở cái cửa sổ FormMain lên khi chạy phần mềm
            Application.Run(new FormConnect());
        }
    }
}