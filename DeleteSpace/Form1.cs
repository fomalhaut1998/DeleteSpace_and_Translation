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
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeleteSpace {
    public partial class Form1 : Form {
        // 用于APPID和SecretKey跨窗口访问
        public static Form1 Mainfrm = null;
        // 标记当前模式
        int mode = 0;
        // 标记相同文本框重复被点击的次数，只有修改了才会写入历史记录防止保存重复记录
        int modifyCount = 0;
        // 百度翻译APPID 
        public string APPID_Baidu = "";
        // 百度翻译秘钥
        public string SecretKey_Baidu = "";
        // 有道翻译APPID 
        public string APPID_Youdao = "";
        // 有道翻译秘钥
        public string SecretKey_Youdao = "";
        // 翻译引擎
        public int engine = 0;
        // 置顶标志
        public bool isTop = false;
        public Form1() {
            Mainfrm = this;
            InitializeComponent();
            // 初始化窗口大小
            this.Height = 620;
            this.Width = 690;
        }

        // 将文字格式化
        private void button1_Click(object sender, EventArgs e) {
            // 模式0：中文->去除空格
            if (this.textBox1.Text != "" && mode == 0) {
                string s = this.textBox1.Text;
                s = s.Trim().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                this.textBox1.Text = s;
            }

            // 模式1：英文->去除多余空格换行
            if (this.textBox1.Text != "" && mode == 1) {
                string s = this.textBox1.Text;
                // 先去除换行，以空格取代
                s = s.Trim().Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                string[] words = s.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                s = String.Join(" ", words);
                this.textBox1.Text = s;
            }
            // 每转化一次被修改的次数+1
            modifyCount++;
            if(modifyCount > 1) {
                return;
            }

            // 没有文件则创建
            StreamWriter sw = new StreamWriter("history.txt", true);
            sw.Write("");
            // 释放资源
            sw.Flush();
            sw.Close();

            // 加载历史记录到historyList以便于从头部插入
            List<string> historyList = new List<string>();
            System.IO.StreamReader sr = new System.IO.StreamReader("history.txt");
            try {
                string line;
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

            // 插入该条历史数据
            historyList.Insert(0, this.textBox1.Text);
            historyList.Insert(0, DateTime.Now.ToString());
            
            // 设置只保存10条历史记录,多余的删除(最后面的是时间距离最远的)
            if (historyList.Count > 20) {
                historyList.RemoveAt(historyList.Count - 1);
                historyList.RemoveAt(historyList.Count - 1);
            }

            // 写入history.txt
            // 先删除文件再用StreamWriter流创建
            System.IO.File.Delete("history.txt");
            sw = new StreamWriter("history.txt", true);
            // 将插入的数据写入history.txt
            for (int i = 0; i < historyList.Count; i++) {
                sw.WriteLine(historyList[i]);
            }
            sw.Flush();
            sw.Close();

            // 格式化同时进行翻译
            // 根据翻译引擎进行翻译
            switch (engine) {
                case 0:
                    // 调用谷歌翻译
                    translateByGoogle();
                    break;
                case 1:
                    // 调用百度翻译
                    translateByBaidu();
                    break;
                case 2:
                    // 调用小牛翻译
                    translateByBiu();
                    break;
                case 3:
                    // 调用有道翻译
                    //translateByYoudao();
                    translateByYoudao2();
                    break;
            }
        }

        // 复制到剪切板
        private void button2_Click(object sender, EventArgs e) {
            if (this.textBox1.Text != "") {
                Clipboard.SetDataObject(this.textBox1.Text);
                MessageBox.Show("已复制原文！");
            }
        }

        // 帮助
        private void button3_Click(object sender, EventArgs e) {
            Form4 form4 = new Form4();
            form4.ShowDialog();
        }

        // 文本框改变事件
        private void textBox1_TextChanged(object sender, EventArgs e) {
            // 自动检测是否有中文从而判断当前语言
            if(Regex.IsMatch(textBox1.Text, "[\u4e00-\u9fbb]")) {
                mode = 0;
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf("中文");
            } else {
                mode = 1;
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf("英文");
            }
            this.label2.Text = "字符数：" + this.textBox1.Text.Length;
            modifyCount = 0 ;
        }

        // 清除文本
        private void button4_Click(object sender, EventArgs e) {
            this.textBox1.Text = "";
        }
            
        // 窗口加载事件
        private void Form1_Load(object sender, EventArgs e) {
            My_Conbobox1();
            My_Conbobox2();
            // 默认为中文模式
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("中文");
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf("谷歌");
            this.label7.Text = "翻译结果:";
            // 从API.txt读入API信息
            // 文件不存在则创建
            if (!File.Exists("API.txt")) {
                StreamWriter sw = new StreamWriter("API.txt", true);
                sw.Write("");
                sw.Flush();
                sw.Close();
            }
            StreamReader sr = new StreamReader("API.txt");
            try {
                this.APPID_Baidu = sr.ReadLine();
                this.SecretKey_Baidu = sr.ReadLine();
                this.APPID_Youdao = sr.ReadLine();
                this.SecretKey_Youdao = sr.ReadLine();
            } catch (Exception ex) {
                // 向用户显示出错消息
                MessageBox.Show("读取API数据出错！\n" + ex.Message);
            } finally {
                // 释放资源
                sr.Close();
            }
        }

        private void My_Conbobox2() {
            comboBox2.Items.Add("谷歌");//选择项1：谷歌
            comboBox2.Items.Add("百度");//选择项2：百度
            comboBox2.Items.Add("小牛");//选择项3：小牛
            comboBox2.Items.Add("有道");//选择项4：有道
        }

        private void My_Conbobox1() {
            comboBox1.Items.Add("中文");//选择项1：中文
            comboBox1.Items.Add("英文");//选择项2：英文
        }

        private void button5_Click(object sender, EventArgs e) {
            // 声明一个IDataObject来保存从剪贴板返回的数据。
            // 从剪贴板中检索数据。
            IDataObject iData = Clipboard.GetDataObject();

            // 确定数据是否为您可以使用的格式。
            if (iData.GetDataPresent(DataFormats.Text)) {
                // 是的，所以在文本框中显示。
                textBox1.Text = (String)iData.GetData(DataFormats.Text);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            //获取选择的内容
            switch (comboBox1.SelectedItem.ToString()) {
                case "中文":
                    this.mode = 0;
                    break;
                case "英文":
                    this.mode = 1;
                    break;
            }
        }

        // 查看历史记录
        private void button6_Click(object sender, EventArgs e) {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        // 清空历史记录
        private void button7_Click(object sender, EventArgs e) {
            // 先删除文件再用StreamWriter流创建
            System.IO.File.Delete("history.txt");
            StreamWriter sw = new StreamWriter("history.txt",true);
            sw.Write("");
            sw.Flush();
            sw.Close();
            MessageBox.Show("已清除历史记录！");
        }

        // 有道翻译(需要API)
        public void translateByYoudao2() {
            Dictionary<String, String> dic = new Dictionary<String, String>();
            string url = "https://openapi.youdao.com/api";
            string q = this.textBox1.Text;
            string appKey = this.APPID_Youdao;
            string appSecret = this.SecretKey_Youdao;
            string salt = DateTime.Now.Millisecond.ToString();
            dic.Add("from", mode == 1 ? "en" : "zh");
            dic.Add("to", mode == 0 ? "en" : "zh");
            dic.Add("signType", "v3");
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            string curtime = Convert.ToString(millis / 1000);
            dic.Add("curtime", curtime);
            string signStr = appKey + Truncate(q) + salt + curtime + appSecret; ;
            string sign = ComputeHash(signStr, new SHA256CryptoServiceProvider());
            dic.Add("q", System.Web.HttpUtility.UrlEncode(q));
            dic.Add("appKey", appKey);
            dic.Add("salt", salt);
            dic.Add("sign", sign);
            dic.Add("vocabId", "您的用户词表ID");
            Post(url, dic);
        }

        // 有道翻译(需要API)
        public string ComputeHash(string input, HashAlgorithm algorithm) {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        // 有道翻译(需要API)
        public void Post(string url, Dictionary<String, String> dic) {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic) {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream()) {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            if (resp.ContentType.ToLower().Equals("audio/mp3")) {
                SaveBinaryFile(resp, "合成的音频存储路径");
            } else {
                Stream stream = resp.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                    result = reader.ReadToEnd();
                }
                if(!result.Contains("\"errorCode\":\"0\"")) {
                    this.textBox2.Text = "获取翻译信息失败，请检查APPID和秘钥！";
                    this.label7.Text = "翻译结果:fail!";
                    return;
                }
                int a = result.IndexOf("\"translation\"");
                int b = result.IndexOf("\"errorCode\"");
                // 注意这里第二个参数是长度
                result = result.Substring(a + 16, b - a - 19);
                this.textBox2.Text = result;
                this.label7.Text = "翻译结果:success!";
            }
        }

        // 有道翻译(需要API)
        public string Truncate(string q) {
            if (q == null) {
                return null;
            }
            int len = q.Length;
            return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
        }

        // 有道翻译(需要API)
        public bool SaveBinaryFile(WebResponse response, string FileName) {
            string FilePath = FileName + DateTime.Now.Millisecond.ToString() + ".mp3";
            bool Value = true;
            byte[] buffer = new byte[1024];

            try {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                Stream outStream = System.IO.File.Create(FilePath);
                Stream inStream = response.GetResponseStream();

                int l;
                do {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            } catch {
                Value = false;
            }
            return Value;
        }


        // 有道翻译(不用API)
        public void translateByYoudao() {
            YouDao(textBox1.Text, mode == 1 ? "en" : "zh", mode == 0 ? "en" : "zh");
        }

        // 有道翻译主体(不用API)
        public void YouDao(string q, string from, string to) {
            string result = "";
            string url = "http://fanyi.youdao.com/translate_o?smartresult=dict&smartresult=rule/";
            string u = "fanyideskweb";
            string c = "Y2FYu%TNSbMCxc3t2u^XT";
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            string curtime = Convert.ToString(millis);
            Random rd = new Random();
            string f = curtime + rd.Next(0, 9);
            string signStr = u + q + f + c;
            string sign = GetMd5Str_32(signStr);
            Dictionary<String, String> dic = new Dictionary<String, String>();
            dic.Add("i", q);
            dic.Add("from", from);
            dic.Add("to", to);
            dic.Add("smartresult", "dict");
            dic.Add("client", "fanyideskweb");
            dic.Add("salt", f);
            dic.Add("sign", sign);
            dic.Add("lts", curtime);
            dic.Add("bv", GetMd5Str_32("5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36"));
            dic.Add("doctype", "json");
            dic.Add("version", "2.1");
            dic.Add("keyfrom", "fanyi.web");
            dic.Add("action", "FY_BY_REALTlME");
            //dic.Add("typoResult", "false");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Referer = "http://fanyi.youdao.com/";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36";
            req.Headers.Add("Cookie", "OUTFOX_SEARCH_USER_ID=-2030520936@111.204.187.35; OUTFOX_SEARCH_USER_ID_NCOO=798307585.9506682; UM_distinctid=17c2157768a25e-087647b7cf38e8-581e311d-1fa400-17c2157768b8ac; P_INFO=15711476666|1632647789|1|youdao_zhiyun2018|00&99|null&null&null#bej&null#10#0|&0||15711476666; JSESSIONID=aaafZvxuue5Qk5_d9fLWx; ___rl__test__cookies=" + curtime);
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic) {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream()) {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                JObject jo = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd());
                if (jo.Value<string>("errorCode").Equals("0")) {
                    var tgtarray = jo.SelectToken("translateResult").First().Values<string>("tgt").ToArray();
                    result = string.Join("", tgtarray);
                }
            }
            this.textBox2.Text = result;
            this.label7.Text = "翻译结果:success!";
        }

        public string GetMd5Str_32(string encryptString) {
            byte[] result = Encoding.UTF8.GetBytes(encryptString);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string encryptResult = BitConverter.ToString(output).Replace("-", "");
            return encryptResult;
        }

        // 小牛翻译
        private void translateByBiu() {
            Niutrans(textBox1.Text, mode == 1 ? "en" : "zh", mode == 0 ? "en" : "zh");
        }

        // 小牛翻译主程序
        private void Niutrans(string q, string from, string to) {
            string result = string.Empty;
            string url = string.Format("https://test.niutrans.com/NiuTransServer/testaligntrans?from={0}&to={1}&src_text={2}&source=text&dictNo=&memoryNo=&isUseDict=0&isUseMemory=0", from, to, q);
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36");
            byte[] responseData = client.DownloadData(url);
            string content = Encoding.GetEncoding("utf-8").GetString(responseData);
            JObject jo = (JObject)JsonConvert.DeserializeObject(content);
            result = jo.Value<string>("tgt_text"); ;
            this.textBox2.Text = result;
            this.label7.Text = "翻译结果:success!";
        }

        // 谷歌翻译
        private void translateByGoogle() {
            string url = "https://translate.google.cn/_/TranslateWebserverUi/data/batchexecute?rpcids=MkEWBc&bl=boq_translate-webserver_20210927.13_p0&soc-app=1&soc-platform=1&soc-device=1&rt=c";
            string q = this.textBox1.Text;
            string from = mode == 1 ? "en" : "zh";
            string to = mode == 0 ? "en" : "zh";
            var from_data = "f.req=" + System.Web.HttpUtility.UrlEncode(
                string.Format("[[[\"MkEWBc\",\"[[\\\"{0}\\\",\\\"{1}\\\",\\\"{2}\\\",true],[null]]\", null, \"generic\"]]]",
                ReplaceString(q), from, to), Encoding.UTF8).Replace("+", "%20");
            byte[] postData = Encoding.UTF8.GetBytes(from_data);
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            client.Headers.Add("ContentLength", postData.Length.ToString());
            client.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36");
            byte[] responseData = client.UploadData(url, "POST", postData);
            string content = Encoding.UTF8.GetString(responseData);
            content = MatchResult(content);
            // 由于解包原因，部分翻译结果与原文前置部分相同需要去掉
            int index = 0;
            string text1 = this.textBox1.Text;
            while(index < text1.Length && index < content.Length && text1[index] == content[index]) {
                index++;
            }
            content = content.Substring(index);
            if(content.Length > 0 && content[0] == ' ') {
                content = content.Substring(1);
            }
            this.textBox2.Text = content;
            this.label7.Text = "翻译结果:success!";
        }

        // 匹配翻译结果(Google)
        public string MatchResult(string content) {
            string result = "";
            string patttern = @",\[\[\\\""(.*?)\\\"",";
            Regex regex = new Regex(patttern);
            MatchCollection matchcollection = regex.Matches(content);
            if (matchcollection.Count > 0) {
                List<string> list = new List<string>();
                foreach (Match match in matchcollection) {
                    list.Add(match.Groups[1].Value);
                }
                result = string.Join(" ", list.Distinct());
                if (result.LastIndexOf(@"\""]]]],\""") > 0) {
                    result = result.Substring(0, result.LastIndexOf(@"\""]]]],"));
                }
            }
            return result;
        }

        // 替换部分字符串(Google)
        public string ReplaceString(string JsonString) {
            if (JsonString == null) { return JsonString; }
            if (JsonString.Contains("\\")) {
                JsonString = JsonString.Replace("\\", "\\\\");
            }
            if (JsonString.Contains("\'")) {
                JsonString = JsonString.Replace("\'", "\\\'");
            }
            if (JsonString.Contains("\"")) {
                JsonString = JsonString.Replace("\"", "\\\\\\\"");
            }
            //去掉字符串的回车换行符
            JsonString = Regex.Replace(JsonString, @"[\n\r]", "");
            JsonString = JsonString.Trim();
            return JsonString;
        }

        // 百度翻译
        public void translateByBaidu() {
            // 原文
            string q = this.textBox1.Text;
            // 源语言
            string from = mode == 1 ? "en" : "zh";
            // 目标语言
            string to = mode == 0 ? "en" : "zh";
            // APP ID
            string appId = this.APPID_Baidu;
            Random rd = new Random();
            string salt = rd.Next(100000).ToString();
            // 密钥
            string secretKey = this.SecretKey_Baidu;
            string sign = EncryptString(appId + q + salt + secretKey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            int i = retString.IndexOf("dst");
            int j = retString.IndexOf("}");
            retString = retString.Substring(i + 6, j - i - 7);
            retString = Regex.Unescape(retString);
            if (retString.Contains("error_msg")) {
                this.textBox2.Text = "获取翻译信息失败，请检查APPID和秘钥！";
                this.label7.Text = "翻译结果:fail!";
                return;
            }
            this.textBox2.Text = retString;
            this.label7.Text = "翻译结果:success!";
        }

        // 计算MD5值(百度翻译)
        public static string EncryptString(string str) {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew) {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
        
        // 复制译文到剪切板
        private void button10_Click(object sender, EventArgs e) {
            if (this.textBox2.Text != "") {
                Clipboard.SetDataObject(this.textBox2.Text);
                MessageBox.Show("已复制翻译！");
            }
        }

        // 清空翻译
        private void button11_Click(object sender, EventArgs e) {
            this.textBox2.Text = "";
        }

        // API信息
        private void button9_Click(object sender, EventArgs e) {
            Form3 form3 = new Form3();
            form3.ShowDialog();
        }

        private void textBox2_TextChanged(object sender, EventArgs e) {
            this.label5.Text = "字符数：" + this.textBox2.Text.Length;
        }

        // 翻译引擎改变时
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) {
            //获取选择的内容
            switch (comboBox2.SelectedItem.ToString()) {
                case "谷歌":
                    this.engine = 0;
                    break;
                case "百度":
                    this.engine = 1;
                    break;
                case "小牛":
                    this.engine = 2;
                    break;
                case "有道":
                    this.engine = 3;
                    break;
            }
            // 根据翻译引擎进行翻译
            switch (engine) {
                case 0:
                    // 调用谷歌翻译
                    translateByGoogle();
                    break;
                case 1:
                    // 调用百度翻译
                    translateByBaidu();
                    break;
                case 2:
                    // 调用小牛翻译
                    translateByBiu();
                    break;
                case 3:
                    // 调用有道翻译
                    translateByYoudao2();
                    break;
            }
        }

        // 设置背景色
        private void button8_Click(object sender, EventArgs e) {
            colorDialog1.ShowDialog();
            Color c = colorDialog1.Color;
            this.BackColor = c;
        }

        // 关闭确认
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            DialogResult dr = MessageBox.Show("确定关闭？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK) {
                // 关闭窗口
                e.Cancel = false;
            } else {
                // 不关闭
                e.Cancel = true;
            }
        }

        private void button12_Click(object sender, EventArgs e) {
            if(this.TopMost == false) {
                this.TopMost = true;
                this.button12.Text = "置顶:开";
                this.isTop = true;
            }else {
                this.TopMost = false;
                this.button12.Text = "置顶:关";
                this.isTop = false;
            }
        }
    }
}
