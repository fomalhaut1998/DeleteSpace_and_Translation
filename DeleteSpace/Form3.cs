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
    public partial class Form3 : Form {
        public Form3() {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e) {
            textBox1.Text = Form1.Mainfrm.APPID_Baidu;
            textBox2.Text = Form1.Mainfrm.SecretKey_Baidu;
            textBox3.Text = Form1.Mainfrm.APPID_Youdao;
            textBox4.Text = Form1.Mainfrm.SecretKey_Youdao;
            // 默认全部不选中
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
            textBox2.SelectionStart = 0;
            textBox2.SelectionLength = 0;
            textBox3.SelectionStart = 0;
            textBox3.SelectionLength = 0;
            textBox4.SelectionStart = 0;
            textBox4.SelectionLength = 0;
            this.TopMost = Form1.Mainfrm.isTop;
        }

        // 保存API信息
        private void button1_Click(object sender, EventArgs e) {
            Form1.Mainfrm.APPID_Baidu = textBox1.Text;
            Form1.Mainfrm.SecretKey_Baidu = textBox2.Text;
            Form1.Mainfrm.APPID_Youdao = textBox3.Text;
            Form1.Mainfrm.SecretKey_Youdao = textBox4.Text;
            MessageBox.Show("保存API信息成功!");
            // 先删除文件再用StreamWriter流创建
            System.IO.File.Delete("API.txt");
            // 再将APPID与秘钥写入API.txt文件中
            StreamWriter sw = new StreamWriter("API.txt", true);
            if(this.textBox1.Text == "") {
                sw.WriteLine("请输入百度APPID!");
            } else {
                sw.WriteLine(this.textBox1.Text);
            }
            if (this.textBox2.Text == "") {
                sw.WriteLine("请输入百度秘钥!");
            } else {
                sw.WriteLine(this.textBox2.Text);
            }
            if (this.textBox3.Text == "") {
                sw.WriteLine("请输入有道APPID!");
            } else {
                sw.WriteLine(this.textBox3.Text);
            }
            if (this.textBox4.Text == "") {
                sw.WriteLine("请输入有道秘钥!");
            } else {
                sw.WriteLine(this.textBox4.Text);
            }
            // 释放资源
            sw.Flush();
            sw.Close();
            // 关闭窗口
            this.Close();
        }
    }
}
