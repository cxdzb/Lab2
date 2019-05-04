using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia.Midi.UI;
using System.Threading;

namespace SequencerDemo
{
    public partial class Form1 : Form
    {
        private bool scrolling = false;

        private bool playing = false;

        private bool closing = false;

        private OutputDevice outDevice;

        private int outDeviceID = 0;

        private OutputDeviceDialog outDialog = new OutputDeviceDialog();

        //*************************添加的代码*************************
        private List<float> Width_Proportion = new List<float>();   //6个Proportion为6个比例（位置、大小、字体），即原始比例，用于以后的计算。
        private List<float> Height_Proportion = new List<float>();
        private List<float> Left_Proportion = new List<float>();
        private List<float> Top_Proportion = new List<float>();
        private List<float> Font_Proportion1 = new List<float>();
        private List<float> Font_Proportion2 = new List<float>();
        private string play_method = "顺序播放";    //初始化播放顺序
        private string current_music = "";  //定义当前播放的音乐
        private int current_index = 0;  //定义当前音乐的索引
        //*************************添加的代码*************************

        public Form1()
        {
            InitializeComponent();

            //*************************添加的代码*************************
            CheckForIllegalCrossThreadCalls = false;    //容许子线呈随时更新ui，类似于invoke，性能比invoke高，此处用于play方法
            foreach (Control control in this.Controls)  //初始化所有控件原始比例
            {
                Width_Proportion.Add((float)control.Width / Width);
                Height_Proportion.Add((float)control.Height / Height);
                Left_Proportion.Add((float)control.Left / Width);
                Top_Proportion.Add((float)control.Top / Height);
                Font_Proportion1.Add((float)control.Font.Size / control.Width);
                Font_Proportion2.Add((float)control.Font.Size / control.Height);
            }
            //*************************添加的代码*************************
        }

        protected override void OnLoad(EventArgs e)
        {
            if(OutputDevice.DeviceCount == 0)
            {
                MessageBox.Show("No MIDI output devices available.", "Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);

                Close();
            }
            else
            {
                try
                {
                    outDevice = new OutputDevice(outDeviceID);

                    sequence1.LoadProgressChanged += HandleLoadProgressChanged;
                    sequence1.LoadCompleted += HandleLoadCompleted;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    Close();
                }
            }

            base.OnLoad(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            pianoControl1.PressPianoKey(e.KeyCode);

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            pianoControl1.ReleasePianoKey(e.KeyCode);

            base.OnKeyUp(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            closing = true;

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            sequence1.Dispose();

            if(outDevice != null)
            {
                outDevice.Dispose();
            }

            outDialog.Dispose();

            base.OnClosed(e);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openMidiFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openMidiFileDialog.FileName;
                Open(fileName);
                
            }
        }

        public void Open(string fileName)
        {
            try
            {
                sequencer1.Stop();
                playing = false;
                sequence1.LoadAsync(fileName);
                this.Cursor = Cursors.WaitCursor;
                startButton.Enabled = false;
                continueButton.Enabled = false;
                stopButton.Enabled = false;
                openToolStripMenuItem.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            //*************************添加的代码*************************
            current_music = fileName;   //每次载入时修改当前音乐
            if (listBox1.Items.IndexOf(fileName)<0) //添加音乐时排除重复音乐，无重复则添加
                listBox1.Items.Add(fileName);
            //*************************添加的代码*************************
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void outputDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog dlg = new AboutDialog();

            dlg.ShowDialog();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            try
            {
                playing = false;
                sequencer1.Stop();
                timer1.Stop();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            try
            {
                playing = true;
                sequencer1.Start();
                timer1.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            try
            {
                playing = true;
                sequencer1.Continue();
                timer1.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void positionHScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if(e.Type == ScrollEventType.EndScroll)
            {
                sequencer1.Position = e.NewValue;

                scrolling = false;
            }
            else
            {
                scrolling = true;
            }
        }

        private void HandleLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            startButton.Enabled = true;
            continueButton.Enabled = true;
            stopButton.Enabled = true;
            openToolStripMenuItem.Enabled = true;
            toolStripProgressBar1.Value = 0;

            if(e.Error == null)
            {
                positionHScrollBar.Value = 0;
                positionHScrollBar.Maximum = sequence1.GetLength();
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void HandleChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            if(closing)
            {
                return;
            }

            outDevice.Send(e.Message);
            pianoControl1.Send(e.Message);
        }

        private void HandleChased(object sender, ChasedEventArgs e)
        {
            foreach(ChannelMessage message in e.Messages)
            {
                outDevice.Send(message);
            }
        }

        private void HandleSysExMessagePlayed(object sender, SysExMessageEventArgs e)
        {
       //     outDevice.Send(e.Message); Sometimes causes an exception to be thrown because the output device is overloaded.
        }

        private void HandleStopped(object sender, StoppedEventArgs e)
        {
            foreach(ChannelMessage message in e.Messages)
            {
                outDevice.Send(message);
                pianoControl1.Send(message);
            }
        }

        private void HandlePlayingCompleted(object sender, EventArgs e)
        {
            timer1.Stop();
            //*************************添加的代码*************************
            play(this,e,play_method);   //音乐完结时，继续播放
            //*************************添加的代码*************************
        }

        private void pianoControl1_PianoKeyDown(object sender, PianoKeyEventArgs e)
        {
            #region Guard

            if(playing)
            {
                return;
            }

            #endregion

            outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, e.NoteID, 127));
        }

        private void pianoControl1_PianoKeyUp(object sender, PianoKeyEventArgs e)
        {
            #region Guard

            if(playing)
            {
                return;
            }

            #endregion

            outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, e.NoteID, 0));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!scrolling)
            {
                positionHScrollBar.Value = Math.Min(sequencer1.Position, positionHScrollBar.Maximum);
            }
        }

        //*************************添加的代码*************************
        private void Form1_Resize(object sender, EventArgs e)   //控件窗口自适应
        {
            if (Width_Proportion.Count == 0) return;
            int i = 0;
            foreach (Control control in this.Controls)  //所有控件和字体按照比例变化
            {
                control.Width = (int)(Width * Width_Proportion[i]);
                control.Height = (int)(Height * Height_Proportion[i]);
                control.Left = (int)(Width * Left_Proportion[i]);
                control.Top = (int)(Height * Top_Proportion[i]);
                float font_size1 = Font_Proportion1[i] * control.Width; //字体按照长和宽中更小的变化来改变
                float font_size2 = Font_Proportion2[i] * control.Height;
                control.Font = new Font(control.Font.Name, font_size1 < font_size2 ? font_size1 : font_size2, control.Font.Style);
                i++;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)   //用于点击播放列表音乐
        {
            string filename = listBox1.SelectedItem.ToString();
            Open(filename);
            while(startButton.Enabled == false) //当音乐载入完毕，模拟点击start按钮
            {
                Application.DoEvents(); //在大运算量循环内，加Application.DoEvents可以防止界面停止响应，如果没有加上 DoEvents的话，由于循环时间会比较久就会出现假死的状态
                Thread.Sleep(100);
            }
            startButton_Click(this, e);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)    //用于拖拽音乐文件
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
            else e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)    //用于拖拽音乐文件
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Open(path);
            playing = true;
            sequencer1.Start();
            timer1.Start();
        }

        private void radioButton_Click(object sender, EventArgs e)  //用于选择播放模式
        {
            if (radioButton1.Checked) play_method = radioButton1.Text;  //模式切换为点击的模式
            else if (radioButton2.Checked) play_method = radioButton2.Text;
            else play_method = radioButton3.Text;
        }

        private void play(object sender, EventArgs e,string flag)   //播放结尾时发生的事件（循环）
        {
            string filename = current_music;    //用于载入播放
            if (flag== "顺序播放")
            {
                int count = listBox1.Items.Count;   //得到列表的音乐数量
                current_index=(current_index+1)%count;  //音乐索引加一
                filename = listBox1.Items[current_index].ToString();    //切换到下一首
            }
            else if (flag == "随机播放")
            {
                int count = listBox1.Items.Count;   //得到列表的音乐数量
                Random rd = new Random();
                current_index=rd.Next(0,count); //利用随机数随机播放
                filename = listBox1.Items[current_index].ToString();
            }
            Open(filename); //类似listBox1_SelectedIndexChanged
            while (startButton.Enabled == false)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            startButton_Click(this, e);
        }
        //*************************添加的代码*************************
    }
}