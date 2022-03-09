using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteSpace {
    public partial class Form2 : Form {
        public Form2() {
            InitializeComponent();
        }

        // 窗口加载事件
        private void Form2_Load(object sender, EventArgs e) {
            // 文件不存在则创建
            if (!File.Exists("history.txt")) {
                StreamWriter sw2 = new StreamWriter("history.txt", true);
                sw2.Write("");
                sw2.Flush();
                sw2.Close();
            }
            // 存储转化的历史记录
            List<string> historyList = new List<string>();
            // 加载历史记录到historyRecords以便于回显数据
            System.IO.StreamReader sr = new System.IO.StreamReader("history.txt");
            try {
                string line;
                // 创建一个 StreamReader 的实例来读取文件 ,using 语句也能关闭 StreamReader
                // 从文件读取并显示行，直到文件的末尾
                while ((line = sr.ReadLine()) != null) {
                    // 每行作为一条历史记录存储
                    historyList.Add(line);
                    line = "";
                }
            } catch (Exception ex) {
                // 向用户显示出错消息
                MessageBox.Show("读取历史数据出错！\n" + ex.Message);
            } finally {
                // 释放资源
                sr.Close();
            }

            // 回写文件历史数据到文本框中
            this.textBox1.Text += "当前历史记录有" + historyList.Count / 2 + "条" + System.Environment.NewLine + System.Environment.NewLine; 
            // 历史记录倒序查看(时间近的先展示)
            for (int i = 0; i < historyList.Count; i++) {
                int idx =  i / 2 + 1;
                // 时间栏
                if(i % 2 == 0) {
                    this.textBox1.Text += "<" + idx + ">" + historyList[i] + System.Environment.NewLine;
                }
                // 内容栏
                if(i % 2 == 1) {
                    this.textBox1.Text += historyList[i] + System.Environment.NewLine;
                }
                // 每2个为一条历史记录
                if (i > 0 && i % 2 == 1) {
                    this.textBox1.Text += System.Environment.NewLine;
                }
            }
            // 默认全部不选中
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
            this.TopMost = Form1.Mainfrm.isTop;
        }
    }
}
