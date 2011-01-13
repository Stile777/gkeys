using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GKeys;
using LuaInterface;

namespace GSXtended
{
    public partial class frmMain : Form
    {
        private delegate void ModeInvoker(Mode mode);

        private RadioButton[] m_rbtn_gKey;
        private string[,] m_scriptPath;

        private RadioButton[] m_rbtn_mode;

        private TextBoxStreamWriter txtStream;

        private RadioButton m_currentRadioButton;

        int m_currentMode;

        private Lua luaVm;

        private GKeyHandler gKeyHandler;

        public frmMain()
        {
            InitializeComponent();
        }



        private void frmMain_Load(object sender, EventArgs e)
        {
#if DEBUG == false
            dbgBox.Visible = false;
#endif
            txtStream = new TextBoxStreamWriter(dbgBox);
            Console.SetOut(txtStream);
            Console.WriteLine("Successfully set up debug box");
            LoadVariables();
            UpdateControls();
        }

        private void LoadVariables()
        {
            gKeyHandler = new GKeyHandler(100);

            gKeyHandler.OnGKeyDown += new OnGKeyDownEventHandler(onGKeyDown);
            gKeyHandler.OnGKeyUp += new OnGKeyUpEventHandler(onGKeyUp);
            gKeyHandler.OnModeChange += new OnModeChangeEventHandler(onModeChange);

            luaVm = new Lua();
            luaVm.RegisterFunction("GetMode", gKeyHandler, gKeyHandler.GetType().GetMethod("GetMode"));
            luaVm.RegisterFunction("IsKeyDown", gKeyHandler, gKeyHandler.GetType().GetMethod("IsKeyDown"));
            luaVm["gKeyHandler"] = gKeyHandler;

            m_rbtn_mode = new RadioButton[3];
            m_rbtn_mode[0] = rbtn_m1;
            m_rbtn_mode[1] = rbtn_m2;
            m_rbtn_mode[2] = rbtn_m3;

            m_currentMode = gKeyHandler.GetMode();
            m_rbtn_mode[m_currentMode].Checked = true;

            m_rbtn_gKey = new RadioButton[18];
            m_rbtn_gKey[0] = rbtnG1;
            m_rbtn_gKey[1] = rbtnG2;
            m_rbtn_gKey[2] = rbtnG3;
            m_rbtn_gKey[3] = rbtnG4;
            m_rbtn_gKey[4] = rbtnG5;
            m_rbtn_gKey[5] = rbtnG6;
            m_rbtn_gKey[6] = rbtnG7;
            m_rbtn_gKey[7] = rbtnG8;
            m_rbtn_gKey[8] = rbtnG9;
            m_rbtn_gKey[9] = rbtnG10;
            m_rbtn_gKey[10] = rbtnG11;
            m_rbtn_gKey[11] = rbtnG12;
            m_rbtn_gKey[12] = rbtnG13;
            m_rbtn_gKey[13] = rbtnG14;
            m_rbtn_gKey[14] = rbtnG15;
            m_rbtn_gKey[15] = rbtnG16;
            m_rbtn_gKey[16] = rbtnG17;
            m_rbtn_gKey[17] = rbtnG18;

            m_currentRadioButton = m_rbtn_gKey[0];

            m_scriptPath = new string[3,18];
            for (int i = 0; i < 18; i++)
                for(int j = 0; j < 3; j++)
                    m_scriptPath[j,i] = "default.lua";
        }

        private void rbtn_CheckedChanged(object sender, EventArgs e)
        {
            m_currentRadioButton = (RadioButton)sender;
            UpdateControls();
        }

        private void UpdateControls()
        {
            int currentIndex = GetRadioButtonIndex(m_currentRadioButton);
            txtScriptPath_M1.Text = m_scriptPath[0, currentIndex];
            txtScriptPath_M2.Text = m_scriptPath[1, currentIndex];
            txtScriptPath_M3.Text = m_scriptPath[2, currentIndex];
            try
            {
                switch (m_currentMode)
                {
                    case 0:
                        luaVm.DoFile(txtScriptPath_M1.Text);
                        break;
                    case 1:
                        luaVm.DoFile(txtScriptPath_M2.Text);
                        break;
                    case 2:
                        luaVm.DoFile(txtScriptPath_M3.Text);
                        break;
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private int GetRadioButtonIndex(RadioButton r)
        {
            for (int i = 0; i < 18; i++)
            {
                if (m_rbtn_gKey[i] == r)
                    return i;
            }
            return -1;
        }

        private void onModeChange(Mode mode)
        {
            if (m_rbtn_mode[(int)mode].InvokeRequired)
            {
                m_rbtn_mode[(int)mode].Invoke(new ModeInvoker(onModeChange), mode);
                return;
            }
            m_currentMode = (int)mode;
            m_rbtn_mode[(int)mode].Checked = true;
            UpdateControls();

            try
            {
                LuaFunction func_onModeChanged = luaVm.GetFunction("onModeChanged");
                if (func_onModeChanged != null)
                {
                    func_onModeChanged.Call((int)mode);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private void onGKeyDown(GKey whichKey)
        {
            try
            {
                LuaFunction func_onGKeyDown = luaVm.GetFunction("onGKeyDown");
                if (func_onGKeyDown != null)
                {
                    func_onGKeyDown.Call((int)whichKey);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private void onGKeyUp(GKey whichKey)
        {
            try
            {
                LuaFunction func_onGKeyUp = luaVm.GetFunction("onGKeyUp");
                if (func_onGKeyUp != null)
                {
                    func_onGKeyUp.Call((int)whichKey);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private void dbgBox_TextChanged(object sender, EventArgs e)
        {
            dbgBox.Select(dbgBox.Text.Length, 0);
            dbgBox.ScrollToCaret();
        }        
       
        private void dialogOpenScript_M1_FileOk(object sender, CancelEventArgs e)
        {
            txtScriptPath_M1.Text = dialogOpenScript_M1.FileName;
        }

        private void dialogOpenScript_M2_FileOk(object sender, CancelEventArgs e)
        {
            txtScriptPath_M2.Text = dialogOpenScript_M2.FileName;
        }

        private void dialogOpenScript_M3_FileOk(object sender, CancelEventArgs e)
        {
            txtScriptPath_M3.Text = dialogOpenScript_M3.FileName;
        }        
        
        private void txtScriptPath_M1_TextChanged(object sender, EventArgs e)
        {
            int currentIndex = GetRadioButtonIndex(m_currentRadioButton);
            m_scriptPath[0,currentIndex] = txtScriptPath_M1.Text;
            try
            {
                luaVm.DoFile(m_scriptPath[m_currentMode,currentIndex]);
            }
            catch {  }
        }

        private void txtScriptPath_M2_TextChanged(object sender, EventArgs e)
        {
            int currentIndex = GetRadioButtonIndex(m_currentRadioButton);
            m_scriptPath[1, currentIndex] = txtScriptPath_M2.Text;
            try
            {
                luaVm.DoFile(m_scriptPath[m_currentMode, currentIndex]);
            }
            catch { }
        }

        private void txtScriptPath_M3_TextChanged(object sender, EventArgs e)
        {
            int currentIndex = GetRadioButtonIndex(m_currentRadioButton);
            m_scriptPath[2, currentIndex] = txtScriptPath_M3.Text;
            try
            {
                luaVm.DoFile(m_scriptPath[m_currentMode, currentIndex]);
            }
            catch { }
        }

        private void btnBrowseScript_M1_Click(object sender, EventArgs e)
        {
            dialogOpenScript_M1.ShowDialog();
        }

        private void btnBrowseScript_M2_Click(object sender, EventArgs e)
        {
            dialogOpenScript_M2.ShowDialog();
        }

        private void btnBrowseScript_M3_Click(object sender, EventArgs e)
        {
            dialogOpenScript_M3.ShowDialog();
        }
    }
}
