using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using Microsoft.DirectX.DirectInput;
using System.Threading;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication6
{



    public partial class Form1 : Form
    {
        int speed = 63;
        int length = 12;
        ushort strR = 0x20;
        ushort strL = 0x1e;
        
        ushort bind1 = 0;
        ushort bind2 = 1;
        int autostrafeY = 0;
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            uint uMsg;
            ushort wParamL;
            ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)] //*
            public MOUSEINPUT mi;
            [FieldOffset(4)] //*
            public KEYBDINPUT ki;
            [FieldOffset(4)] //*
            public HARDWAREINPUT hi;
        }
        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

        const int KEYEVENTF_DOWN = 0; //key UP
        const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        const int KEYEVENTF_KEYUP = 0x0002; //key UP
        const int KEYEVENTF_UNICODE = 0x0004;
        const int KEYEVENTF_SCANCODE = 0x0008; // scancode
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        const int MOUSEEVENTF_MOVE = 0x0001;


        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
        public static void GenerateKey(ushort vk, bool key_down)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = 1;

            KEYBDINPUT kb = new KEYBDINPUT(); //{0};
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - DateTime.Parse("1970-01-01T00:00:00").Ticks);
            UInt32 time_t = Convert.ToUInt32(ts.TotalSeconds);
            kb.wScan = vk;
            kb.time = time_t;
            if (key_down)
            {
                kb.dwFlags = (uint) ( KEYEVENTF_DOWN | KEYEVENTF_SCANCODE );
                kb.wVk = (ushort)0;
                kb.dwExtraInfo = GetMessageExtraInfo();
                inputs[0].ki = kb;
                SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
            }
            else
            {
                kb.dwFlags = (uint) ( KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE );
                kb.wVk = (ushort)0;
                kb.dwExtraInfo = GetMessageExtraInfo();
                inputs[0].ki = kb;
                SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
            }
             
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        static void move_mouse(int x, int y)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = 0;
            MOUSEINPUT mi = new MOUSEINPUT();
            mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
            mi.dx += x;
            mi.dy += y;
            inputs[0].mi = mi;
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }


       UInt16 Butn1;
       Thread t;
        Device keyboard, mouse, joystick;
        bool f = false;
        private void InitDevices() {
            keyboard = new Device(SystemGuid.Keyboard);
            if (keyboard == null) throw new Exception("No keyboard found.");

            //create mouse device.
            mouse = new Device(SystemGuid.Mouse);
            if (mouse == null)
            {
                throw new Exception("No mouse found.");
            }
            mouse.Acquire();
        
        }
        public void AutoStrafe() {
            f = true;
            while (radioButton2.Checked)
            {
                
                if (GetAsyncKeyState(Butn1) != 0) {
                    GenerateKey(strL, true); //D
                    for (int x = 0; x < speed; x++)
                    {
                        if (f)
                        {
                            move_mouse(-length / 2, autostrafeY);
                        }
                        else move_mouse(-length, autostrafeY);
                        Thread.Sleep(1);
                    }
                    f = false;
                    GenerateKey(strL, false);
                    if (GetAsyncKeyState(Butn1) == 0) break;
                    GenerateKey(strR, true);
                    for (int x = 0; x < speed; x++)
                    {
                        move_mouse(length, -autostrafeY);
                        Thread.Sleep(1);
                    }
                    GenerateKey(strR, false);
                
                }
                
            }
            t = new Thread(new ThreadStart(StrafeHelper));
            t.Start();
            
        }
        public void StrafeHelper() {
            while (radioButton1.Checked)
            {
                if (GetAsyncKeyState(Butn1) != 0)
                {
                    MouseState state = mouse.CurrentMouseState;
                    if (state.X > 0)
                    {
                        GenerateKey(strR, true);
                        f = true;
                    }
                    else if (state.X < 0)
                    {
                        GenerateKey(strR, false);
                    }
                    if (state.X < 0)
                    {
                        GenerateKey(strL, true);
                        f = true;
                    }
                    else if (state.X > 0)
                    {
                        GenerateKey(strL, false);
                    }
                    
                }
                if (GetAsyncKeyState(Butn1) == 0 && f == true) {
			        f = false;
			        GenerateKey(strR, false);
			        GenerateKey(strL, false);
		        }
                Thread.Sleep(1);
            }
            t = new Thread(new ThreadStart(AutoStrafe));
            t.Start();
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton3.Checked = true;
            InitDevices();
            textBox1.Text = Convert.ToString(63);
            textBox2.Text = Convert.ToString(12);
            this.MouseDown -= Form1_MouseDown;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (t != null)
            t.Suspend();
            this.KeyPreview = true;
            this.MouseDown += Form1_MouseDown;
            button1.MouseDown += button1_MouseDown;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            button1.Text = e.Button.ToString();
            string x = e.Button.ToString();
            //if (x == "Left") Butn1 = 0x01;
            if (x == "Right") Butn1 = 0x02;
            if (x == "Middle") Butn1 = 0x04;
            if (x == "XButton1") Butn1 = 0x05;
            if (x == "XButton2") Butn1 = 0x06;
            this.MouseDown -= Form1_MouseDown;
            button1.MouseDown -= button1_MouseDown;
            this.KeyPreview = false;
            if (radioButton1.Checked)
                t = new Thread(new ThreadStart(StrafeHelper));
            else t = new Thread(new ThreadStart(AutoStrafe));
            t.Start();
        }
        private void button1_MouseDown(object sender, MouseEventArgs e) {
            button1.Text = e.Button.ToString();
            string x = e.Button.ToString();
            //if (x == "Left") Butn1 = 0x01;
            if (x == "Right") Butn1 = 0x02;
            if (x == "Middle") Butn1 = 0x04;
            if (x == "XButton1") Butn1 = 0x05;
            if (x == "XButton2") Butn1 = 0x06;
            this.MouseDown -= Form1_MouseDown;
            button1.MouseDown -= button1_MouseDown;
            this.KeyPreview = false;
            if (radioButton1.Checked)
                t = new Thread(new ThreadStart(StrafeHelper));
            else t = new Thread(new ThreadStart(AutoStrafe));
            t.Start();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (t != null) t.Abort();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                textBox1.Text = "0";
            if (textBox2.Text == "")
                textBox2.Text = "0";
            speed = int.Parse(textBox1.Text);
            length = int.Parse(textBox2.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "") {
            length = int.Parse(textBox2.Text);
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                speed = int.Parse(textBox1.Text);
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                strL = 0x11;
                strR = 0x1F;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                strR = 0x20;
                strL = 0x1e;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                strR = 0x1e;
                strL = 0x20;
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                strL = 0x1F;
                strR = 0x11;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "") {
                autostrafeY = int.Parse(textBox3.Text);
            }
            
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            
        }


        private void button3_KeyDown(object sender, KeyEventArgs e)
        {
                
        }
    }
}
