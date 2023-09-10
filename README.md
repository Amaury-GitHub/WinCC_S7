# WinCC_S7
## 背景描述
本程序基于WinCC自带的DLL以及S7.Net库开发</br>
基于工作的需求,现场有一个PLC不具备修改条件,拥有100多个DB用于存放序列号,同时使用一个托盘号变量进行指向,HMI可以配置动态变量连接</br>
WinCC无法实现此种变量连接,序列号还是以byte的形式存储,每个都连接再处理会导致外部变量的数量爆表</br>
所以自己开发一个app来实现WinCC与PLC的交互,减少变量占用</br>
## 功能介绍
使用ini文件存储相关参数</br>
公司没有开发环境,记不得变量的具体参数,索性搞成配置式的吧</br>
包括托盘号的WinCC变量名称,WinCC中的序列号名称,PLC的IP,托盘号与DB号的对应关系,序列号在DB中的起始地址,序列号的长度</br>
程序启动时会读取相关参数,然后从WinCC中读取托盘号,处理后从PLC中读取序列号,处理后写入到WinCC中</br>
![image](https://github.com/Amaury-GitHub/WinCC_S7/blob/main/README_IMG/IMG1.png)<br>
## 截图
![image](https://github.com/Amaury-GitHub/WinCC_S7/blob/main/README_IMG/IMG2.png)<br>
![image](https://github.com/Amaury-GitHub/WinCC_S7/blob/main/README_IMG/IMG3.png)<br>
![image](https://github.com/Amaury-GitHub/WinCC_S7/blob/main/README_IMG/IMG4.png)<br>
