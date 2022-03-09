# DeleteSpace_and_Translation
##本软件编写的初衷是针对一些PDF文献复制格式存在多余空格(换行等)的格式标准化

软件的编写全程为C#语言(基于.NET Framework 4.6.1)

主要功能如下：

1.文本格式标准化:根据输入自动识别中英文，中文去掉部分多余空格+换行；英文只保留一个空格+去换行

2.文本翻译功能，目前集成了谷歌、百度、小牛、有道4个翻译引擎

3.一键清空原文、翻译；一键复制原文、粘贴剪切板内容、复制译文

4.保留转换的历史记录(保留最近10条)

5.其他：换背景色、字符数统计等
问题说明:百度和有道的翻译需要自己的API信息，技术是基于开放平台接口的调用+JSON数据包解析
谷歌和小牛的翻译不需要API信息，技术是基于模拟浏览器发起GET请求 + JSON数据包解析
小牛引擎还存在显示不完整的问题，日后修复
历史记录保存在：./ history.txt中；API信息保存在：./ API.txt中
##本软件仅仅为本人兴趣所写，不承担任何责任~~~
By:Fomalhaut
Version: 1.2
![示意图](https://user-images.githubusercontent.com/53938635/157469334-5efa64b6-d42d-4f69-abdf-a9027bdd881f.jpg)
