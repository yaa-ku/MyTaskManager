﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;

namespace MyTaskManager
{
    public partial class Form1 : Form
    {

        private List<Process> processes = null;

        string[] p_win = {"winlogon","wininit","taskmgr","taskhost","System","spoolsv","svchost",
                        "smss","scrncap","sihost","services","SearchIndexer",
                        "SecurityHealthService","ntoskrnl","lsass","lsm","internat","irstrtsv",
                        "explorer","dwm","csrss","conhost","ApplicationFrameHost"};


        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();

            RefreshProcessesList();
        }

        private void GetProcesses()
        {
            processes.Clear();//очищаем список всех процессов

            processes = Process.GetProcesses().ToList<Process>(); //получаем список всех системных процессов
                                                                  //т.к. возвращаемый тип - массив
                                                                  //приводим к типу списков
        }

        private void RefreshProcessesList()
        {


            listView1.Items.Clear();//очищаем список процессов

            double memSize = 0;

            foreach (Process p in processes)
            {
                memSize = 0;

                PerformanceCounter pc = new PerformanceCounter();//perfomance counter представляет компонент счётчика производительности Windows NT

                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;

                memSize = (double)pc.NextValue() / (1000 * 1000); //память в мегобайтах

                string processType = "Фоновый процесс";
               
                for (int i = 0; i < p_win.Length; i++)
                {
                    if (p_win[i] == p.ProcessName.ToString())
                    {
                        processType = "Процесс Windows";
                    }
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        processType = "Приложение";
                    }
                }


                    string[] row = new string[] {   p.ProcessName.ToString(),
                                                Math.Round(memSize,1).ToString(),
                                                processType};

                listView1.Items.Add(new ListViewItem(row));

                pc.Close();
                pc.Dispose();
            }


            Text = "Запущено процессов: " + processes.Count().ToString();
        }

        private void RefreshProcessesList(List<Process> processes, string keyword) //перегрузка метода
        {

            try
            {

                listView1.Items.Clear();//очищаем список процессов

                double memSize = 0;

                foreach (Process p in processes)
                {
                    if (p != null)
                    {
                        memSize = 0;

                        PerformanceCounter pc = new PerformanceCounter();//perfomance counter представляет компонент счётчика производительности Windows NT

                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = p.ProcessName;

                        memSize = (double)pc.NextValue() / (1000 * 1000); //память в мегобайтах

                        string processType = "Фоновый процесс";
                        if (p.MainWindowHandle != IntPtr.Zero) processType = "Приложение"; //если процесс является «приложением», он должен иметь собственное имя окна, в противном случае это «фоновое приложение"

                        for (int i = 0; i < p_win.Length; i++)
                        {
                            if (p_win[i] == p.ProcessName.ToString())
                            {
                                processType = "Процесс Windows";
                            }
                            if (p.MainWindowHandle != IntPtr.Zero)
                            {
                                processType = "Приложение";
                            }
                        }

                        string[] row = new string[] {   p.ProcessName.ToString(),
                                                Math.Round(memSize,1).ToString(),
                                                processType};

                        listView1.Items.Add(new ListViewItem(row));

                        pc.Close();
                        pc.Dispose();
                    }
                }


                Text = $"Запущено процессов '{keyword}': " + processes.Count().ToString();
            }

            catch (Exception) { }

        }

        private void KillProcess(Process process)
        {
            process.Kill();

            process.WaitForExit();
        }

        private void KillProcessAndChildren(int pid)
        {
            if (pid == 0)
            {
                return;
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "Select * From Win32_Process Where ParentProcessID=" + pid);

            ManagementObjectCollection objectCollection = searcher.Get();

            foreach (ManagementObject obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }

            try
            {
                Process p = Process.GetProcessById(pid);

                p.Kill();

                p.WaitForExit();
            }

            catch (ArgumentException) { }
        }

        private int GetParentProcessId(Process p)
        {
            int parentID = 0;

            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + p.Id + "'");

                managementObject.Get();

                parentID = Convert.ToInt32(managementObject["ParentProcessID"]);
            }

            catch (Exception) { }

            return parentID;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();

            GetProcesses();

            RefreshProcessesList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);

                    GetProcesses();

                    RefreshProcessesList();
                }
            }

            catch (Exception) { }
        }

        private void завершитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);

                    GetProcesses();

                    RefreshProcessesList();
                }
            }

            catch (Exception) { }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();

            List<Process> filteredprocesses = processes.Where((x) =>
                                              x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();

            RefreshProcessesList(filteredprocesses, toolStripTextBox1.Text);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));

                    GetProcesses();

                    RefreshProcessesList();

                }
            }

            catch (Exception) { }
        }

        private void завершитьДеревоПроцессовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));

                    GetProcesses();

                    RefreshProcessesList();

                }
            }

            catch (Exception) { }
        }
    }
}
