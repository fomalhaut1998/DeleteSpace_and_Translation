using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteSpace {
    public partial class Form4 : Form {
        public Form4() {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e) {
            this.textBox1.Text = "本软件编写的初衷是针对一些PDF文献复制格式存在多余空格(换行等)的格式标准化"+ System.Environment.NewLine + "软件的编写全程为C#语言(基于.NET Framework 4.6.1)" + System.Environment.NewLine + "主要功能如下：" + System.Environment.NewLine + "1.文本格式标准化:根据输入自动识别中英文，中文去掉部分多余空格+换行；英文只保留一个空格+去换行" + System.Environment.NewLine + "2.文本翻译功能，目前集成了谷歌、百度、小牛、有道4个翻译引擎" + System.Environment.NewLine + "3.一键清空原文、翻译；一键复制原文、粘贴剪切板内容、复制译文" + System.Environment.NewLine + "4.保留转换的历史记录(保留最近10条)" + System.Environment.NewLine + "5.其他：换背景色、字符数统计等" + System.Environment.NewLine + "问题说明:百度和有道的翻译需要自己的API信息，技术是基于开放平台接口的调用+JSON数据包解析" + System.Environment.NewLine + "谷歌和小牛的翻译不需要API信息，技术是基于模拟浏览器发起GET请求 + JSON数据包解析" + System.Environment.NewLine + "小牛引擎还存在显示不完整的问题，日后修复" + System.Environment.NewLine + "历史记录保存在：~./ history.txt中；API信息保存在：~./ API.txt中" + System.Environment.NewLine + "##本软件仅仅为本人兴趣所写，不承担任何责任，有机会可以开源~~~##" + System.Environment.NewLine + "By:Fomalhaut" + System.Environment.NewLine + "Version: 1.2";
            // 默认全部不选中
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
            this.TopMost = Form1.Mainfrm.isTop;
        }
    }
}
