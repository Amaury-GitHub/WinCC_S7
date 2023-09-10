using S7.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCC_S7
{
    public partial class WinCC_S7 : Form
    {
        // 创建PLC的连接变量
        private Plc S7;
        // 创建WinCC的连接变量
        private CCHMIRUNTIME.HMIRuntime WinCC;
        public WinCC_S7()
        {
            InitializeComponent();
        }
        // UI加载事件
        private void Form_Load(object sender, EventArgs e)
        {
            // 获取当前正在运行的程序的路径
            string exeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // 构建ini的完整路径
            string filePath = Path.Combine(exeDirectory, "WinCC_S7.ini");
            // 提取ini中的参数
            IniParser parser = new IniParser();
            List<Parameter> parameters = parser.ParseIniFile(filePath);
            foreach (Parameter parameter in parameters)
            {
                string key = parameter.Key;
                string value = parameter.Value;

                if (key == "PalletNumber1")
                {
                    PalletNumber1Name.Text = value;
                }
                else if (key == "PalletNumber2")
                {
                    PalletNumber2Name.Text = value;
                }
                else if (key == "MLFB1")
                {
                    MLFB1Name.Text = value;
                }
                else if (key == "MLFB2")
                {
                    MLFB2Name.Text = value;
                }
                else if (key == "IP")
                {
                    S7_IP.Text = value;
                }
                else if (key == "AddValue")
                {
                    AddValue.Text = value;
                }
                else if (key == "StartAddress")
                {
                    StartAddress.Text = value;
                }
                else if (key == "Length")
                {
                    Length.Text = value;
                }
            }
            // 启动循环
            Loop();
        }
        // UI关闭事件
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 停止循环
            cancellationTokenSource?.Cancel();
            // 断开PLC
            S7?.Close();
            // 释放PLC资源
            if (S7 != null)
            {
                S7 = null;
            }
            // 释放WinCC资源
            if (WinCC != null)
            {
                WinCC = null;
            }
        }
        private CancellationTokenSource cancellationTokenSource;
        private async void Loop()
        {
            cancellationTokenSource = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    // 从WinCC读取托盘号码
                    Invoke((Action)(() =>
                    {
                        try
                        {
                            // 创建WinCC连接
                            WinCC = new CCHMIRUNTIME.HMIRuntime();
                            // 读取托盘1的号码
                            PalletNumber1Value.Text = WinCC.Tags[PalletNumber1Name.Text].Read().ToString();
                            // 更新连接状态
                            BeginInvoke((Action)(() => WinCC_Status.Checked = true));
                        }
                        catch (Exception)
                        {
                            // 更新连接状态
                            BeginInvoke((Action)(() => WinCC_Status.Checked = false));
                            PalletNumber1Value.Text = null;
                            PalletNumber2Value.Text = null;
                        }
                        try
                        {
                            // 读取托盘2的号码
                            PalletNumber2Value.Text = WinCC.Tags[PalletNumber2Name.Text].Read().ToString();
                        }
                        catch (Exception)
                        {

                        }
                    }));
                    // 尝试连接PLC
                    if (!PLC_Status.Checked)
                    {
                        try
                        {
                            // 创建PLC连接
                            S7 = new Plc(CpuType.S7300, S7_IP.Text, 0, 1);
                            // 尝试连接PLC
                            S7.Open();
                            // 更新连接状态
                            BeginInvoke((Action)(() => PLC_Status.Checked = true));
                        }
                        catch (Exception)
                        {
                            S7.Close();
                            S7 = null;
                            // 更新连接状态
                            BeginInvoke((Action)(() => PLC_Status.Checked = false));
                        }
                    }
                    // 从PLC读取MLFB
                    Invoke((Action)(() =>
                    {
                        if (PalletNumber1Value.Text != "0")
                        {
                            try
                            {
                                byte[] data1 = S7.ReadBytes(DataType.DataBlock, (int.Parse(PalletNumber1Value.Text) + int.Parse(AddValue.Text)), int.Parse(StartAddress.Text), int.Parse(Length.Text));
                                MLFB1Value.Text = Encoding.ASCII.GetString(data1);
                            }
                            catch (Exception)
                            {
                                MLFB1Value.Text = null;
                                S7?.Close();
                                S7 = null;
                                // 更新连接状态
                                BeginInvoke((Action)(() => PLC_Status.Checked = false));
                            }
                        }
                        else
                        {
                            MLFB1Value.Text = null;
                        }
                        if (PalletNumber2Value.Text != "0")
                        {
                            try
                            {
                                byte[] data2 = S7.ReadBytes(DataType.DataBlock, (int.Parse(PalletNumber2Value.Text) + int.Parse(AddValue.Text)), int.Parse(StartAddress.Text), int.Parse(Length.Text));
                                MLFB2Value.Text = Encoding.ASCII.GetString(data2);
                            }
                            catch (Exception)
                            {
                                MLFB2Value.Text = null;
                                S7?.Close();
                                S7 = null;
                                // 更新连接状态
                                BeginInvoke((Action)(() => PLC_Status.Checked = false));
                            }
                        }
                        else
                        {
                            MLFB2Value.Text = null;
                        }
                    }));
                    // 写入托盘对应的MLFB到WinCC
                    Invoke((Action)(() =>
                    {
                        try
                        {
                            // 写入托盘1对应的MLFB
                            WinCC.Tags[MLFB1Name.Text].Write(MLFB1Value.Text);
                        }
                        catch (Exception)
                        {

                        }
                        try
                        {
                            // 写入托盘2对应的MLFB
                            WinCC.Tags[MLFB2Name.Text].Write(MLFB2Value.Text);
                        }
                        catch (Exception)
                        {

                        }
                    }));

                    WinCC = null;

                    // 等待500ms后循环执行
                    await Task.Delay(500);
                }
            });
        }
        public class Parameter
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        public class IniParser
        {
            public List<Parameter> ParseIniFile(string filePath)
            {
                List<Parameter> parameters = new List<Parameter>();

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (string.IsNullOrEmpty(line))
                            continue;

                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            _ = line.Substring(1, line.Length - 2);
                            continue;
                        }

                        int separatorIndex = line.IndexOf('=');
                        if (separatorIndex > 0)
                        {
                            string key = line.Substring(0, separatorIndex).Trim();
                            string value = line.Substring(separatorIndex + 1).Trim();

                            Parameter parameter = new Parameter
                            {
                                Key = key,
                                Value = value
                            };

                            parameters.Add(parameter);
                        }
                    }
                }

                return parameters;
            }
        }

    }
}
