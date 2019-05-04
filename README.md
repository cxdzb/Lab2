# 《Windows Form实现MIDI音乐文件的播放APP》实验报告
#### 学院：软件学院  班级：2017级软工1班  学号：3017218062   姓名：刘书裴
#### 日期：  2019  年 4 月 4 日
## 一、功能概述
###### 1、点击open，选择音乐进行播放。
###### 2、鼠标拖入midi文件进行播放。
###### 3、可使用按钮进行开始/暂停/继续操作，也可使用进度条改变播放进度。
###### 4、中央列表储存着播放过的音乐。
###### 5、可选择播放模式（单曲循环/顺序播放/随机播放）。

## 二、项目特色
###### 1、多种播放模式。
###### 2、可点击列表对应音乐继续播放。
###### 3、加入try-catch异常语句，防止意外错误的发生。
###### 4、实现了窗口自适应。

## 三、代码总量
无法计算

## 四、工作时间
无法计算

## 五、知识点总结图（Concept MAP）
![](https://github.com/cxdzb/Lab2/blob/master/Images/1.png?raw=true)

## 六、结论
#### 实验过程：

###### 1、实现控件和窗口的自适应
1)、定义多个容器，用于保存各个控件的原始比例。
2)、在Form1构造函数中，初始化各容器。
3)、在事件Form1Resize（窗口大小变化事件）中，遍历所有控件，将其大小、位置、字体按比例变化。
###### 2、实现文件拖拽读取
1)、DragEnter 事件在其他应用程序拖入的文件进入时判断当前拖动的对象类型，如果是文件类型，则设置拖动响应类型为Copy。
2)、DragDrop 事件在这里完成将其他应用程序拖入的文件拷贝到Winform应用当前的目录中。
###### 3、实现列表点击播放
1)、添加一个listBox。
2)、编辑listBox1_SelectedIndexChanged（选择项改变）事件，当选中音乐时，载入并播放此音乐。
3)、使用Application.DoEvents避免程序假死。
###### 4、实现播放模式选择
1)、添加3个radioButton。
2)、定义play_method属性用于储存播放模式。
3)、3个radioButton公用radioButton_Click事件，默认为顺序播放，当选择某个模式时，play_method随之改变。
###### 5、实现多种播放模式
1)、定义当前播放的音乐及其索引。
2)、在HandlePlayingCompleted事件中调用play。
3)、创建play函数，判断播放模式。对于循环播放，即播放列表中下一首；对于随机播放，使用随机数选择音乐；对于单曲循环，即重新播放当前音乐。
###### 6、修改Sequencer.cs避免越界
测试时发现，每切换一首歌，对应的enumerators.Count就会变化，而foreach是线程不安全的，当enumerators.Count变小，就会发生越界，所以将其替换为for + try-catch就能动态地改变i和count，避免越界的发生。
#### 实验结果：
###### 1、刚打开程序时
![](https://github.com/cxdzb/Lab2/blob/master/Images/2.png?raw=true)

###### 2、改变形状时
![](https://github.com/cxdzb/Lab2/blob/master/Images/3.png?raw=true)

###### 3、将音乐加入列表
![](https://github.com/cxdzb/Lab2/blob/master/Images/4.png?raw=true)

###### 4、拖拽音乐到列表
![](https://github.com/cxdzb/Lab2/blob/master/Images/5.png?raw=true)
![](https://github.com/cxdzb/Lab2/blob/master/Images/6.png?raw=true)
