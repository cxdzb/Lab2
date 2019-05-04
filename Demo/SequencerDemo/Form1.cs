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

        //*************************��ӵĴ���*************************
        private List<float> Width_Proportion = new List<float>();   //6��ProportionΪ6��������λ�á���С�����壩����ԭʼ�����������Ժ�ļ��㡣
        private List<float> Height_Proportion = new List<float>();
        private List<float> Left_Proportion = new List<float>();
        private List<float> Top_Proportion = new List<float>();
        private List<float> Font_Proportion1 = new List<float>();
        private List<float> Font_Proportion2 = new List<float>();
        private string play_method = "˳�򲥷�";    //��ʼ������˳��
        private string current_music = "";  //���嵱ǰ���ŵ�����
        private int current_index = 0;  //���嵱ǰ���ֵ�����
        //*************************��ӵĴ���*************************

        public Form1()
        {
            InitializeComponent();

            //*************************��ӵĴ���*************************
            CheckForIllegalCrossThreadCalls = false;    //�������߳���ʱ����ui��������invoke�����ܱ�invoke�ߣ��˴�����play����
            foreach (Control control in this.Controls)  //��ʼ�����пؼ�ԭʼ����
            {
                Width_Proportion.Add((float)control.Width / Width);
                Height_Proportion.Add((float)control.Height / Height);
                Left_Proportion.Add((float)control.Left / Width);
                Top_Proportion.Add((float)control.Top / Height);
                Font_Proportion1.Add((float)control.Font.Size / control.Width);
                Font_Proportion2.Add((float)control.Font.Size / control.Height);
            }
            //*************************��ӵĴ���*************************
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
            //*************************��ӵĴ���*************************
            current_music = fileName;   //ÿ������ʱ�޸ĵ�ǰ����
            if (listBox1.Items.IndexOf(fileName)<0) //�������ʱ�ų��ظ����֣����ظ������
                listBox1.Items.Add(fileName);
            //*************************��ӵĴ���*************************
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
            //*************************��ӵĴ���*************************
            play(this,e,play_method);   //�������ʱ����������
            //*************************��ӵĴ���*************************
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

        //*************************��ӵĴ���*************************
        private void Form1_Resize(object sender, EventArgs e)   //�ؼ���������Ӧ
        {
            if (Width_Proportion.Count == 0) return;
            int i = 0;
            foreach (Control control in this.Controls)  //���пؼ������尴�ձ����仯
            {
                control.Width = (int)(Width * Width_Proportion[i]);
                control.Height = (int)(Height * Height_Proportion[i]);
                control.Left = (int)(Width * Left_Proportion[i]);
                control.Top = (int)(Height * Top_Proportion[i]);
                float font_size1 = Font_Proportion1[i] * control.Width; //���尴�ճ��Ϳ��и�С�ı仯���ı�
                float font_size2 = Font_Proportion2[i] * control.Height;
                control.Font = new Font(control.Font.Name, font_size1 < font_size2 ? font_size1 : font_size2, control.Font.Style);
                i++;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)   //���ڵ�������б�����
        {
            string filename = listBox1.SelectedItem.ToString();
            Open(filename);
            while(startButton.Enabled == false) //������������ϣ�ģ����start��ť
            {
                Application.DoEvents(); //�ڴ�������ѭ���ڣ���Application.DoEvents���Է�ֹ����ֹͣ��Ӧ�����û�м��� DoEvents�Ļ�������ѭ��ʱ���ȽϾþͻ���ּ�����״̬
                Thread.Sleep(100);
            }
            startButton_Click(this, e);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)    //������ק�����ļ�
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
            else e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)    //������ק�����ļ�
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Open(path);
            playing = true;
            sequencer1.Start();
            timer1.Start();
        }

        private void radioButton_Click(object sender, EventArgs e)  //����ѡ�񲥷�ģʽ
        {
            if (radioButton1.Checked) play_method = radioButton1.Text;  //ģʽ�л�Ϊ�����ģʽ
            else if (radioButton2.Checked) play_method = radioButton2.Text;
            else play_method = radioButton3.Text;
        }

        private void play(object sender, EventArgs e,string flag)   //���Ž�βʱ�������¼���ѭ����
        {
            string filename = current_music;    //�������벥��
            if (flag== "˳�򲥷�")
            {
                int count = listBox1.Items.Count;   //�õ��б����������
                current_index=(current_index+1)%count;  //����������һ
                filename = listBox1.Items[current_index].ToString();    //�л�����һ��
            }
            else if (flag == "�������")
            {
                int count = listBox1.Items.Count;   //�õ��б����������
                Random rd = new Random();
                current_index=rd.Next(0,count); //����������������
                filename = listBox1.Items[current_index].ToString();
            }
            Open(filename); //����listBox1_SelectedIndexChanged
            while (startButton.Enabled == false)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            startButton_Click(this, e);
        }
        //*************************��ӵĴ���*************************
    }
}