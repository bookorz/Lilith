﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lilith
{
    static class Program
    {
        //每支程式以不同GUID當成Mutex名稱，可避免執行檔同名同姓的風險
        static string appGuid = "{B19DAFCB-729C-43A6-8232-F3C31BB4E404}";
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //如果要做到跨Session唯一，名稱可加入"Global\"前綴字
            //如此即使用多個帳號透過Terminal Service登入系統
            //整台機器也只能執行一份
            using (Mutex m = new Mutex(false, "Global\\" + appGuid))
            {
                //檢查是否同名Mutex已存在(表示另一份程式正在執行)
                if (!m.WaitOne(0, false))
                {
                    MessageBox.Show("偵測到程式已開啟!");
                    return;
                }

                #region 程式處理邏輯
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //啟動畫面
                SplashScreen.ShowSplashScreen();


                Application.Run(new FormMain());
                #endregion
            }
        }
    }
}
