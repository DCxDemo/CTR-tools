﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using CTRFramework;
using CTRFramework.Shared;

namespace CTRtools
{
    public partial class Form1 : Form
    {
        string path;
        Scene scn;
        ColorDialog cd;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Form1()
        {
            InitializeComponent();
            cd = new ColorDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadLEV();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLEV();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (scn != null)
                if (scn.pickups.Count > 0)
                    propertyGrid1.SelectedObject = scn.pickups[trackBar1.Value];
        }


        private void LoadLEV()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CTR Scene file (*.lev)|*.lev";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                path = ofd.FileName;
                scn = new Scene(path, "obj");

                Text = String.Format("levTool - {0}", path);
                propertyGrid1.SelectedObject = scn.pickups[0];
                trackBar1.Maximum = scn.pickups.Count - 1;
            }
        }



        private void SaveLEV()
        {
            using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(path)))
            {
                /*
                bw.BaseStream.Position = ptrPickupHeaders;

                foreach (PickupHeader ph in headers)
                    ph.Write(bw);
                */

                bw.BaseStream.Position = scn.meshinfo.ptrvertarray + 4;

                foreach (Vertex v in scn.vert)
                    v.Write(bw);
               
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (scn != null)
            {
                foreach (PickupHeader ph in scn.pickups)
                {
                    ph.Position.Y += (short)numericUpDown1.Value;
                }

                //lmao
                propertyGrid1.SelectedObject = propertyGrid1.SelectedObject;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (scn != null)
            {
                foreach (PickupHeader ph in scn.pickups)
                {
                    ph.Position.Y -= (short)numericUpDown1.Value;
                }

                //lmao
                propertyGrid1.SelectedObject = propertyGrid1.SelectedObject;
            }
        }



        Color vertexcolor1 = Color.Gray;
        Color vertexcolor2 = Color.Gray;

        private void button2_Click(object sender, EventArgs e)
        {
            if (scn != null)
            {
                foreach (Vertex v in scn.vert)
                {
                    v.SetColor(Vcolor.Default, new Vector4b(vertexcolor1));
                    v.SetColor(Vcolor.Morph, new Vector4b(vertexcolor2));
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (cd.ShowDialog() == DialogResult.OK)
            {
                vertexcolor1 = cd.Color;
                button1.BackColor = vertexcolor1;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (cd.ShowDialog() == DialogResult.OK)
            {
                vertexcolor2 = cd.Color;
                button5.BackColor = vertexcolor2;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (scn != null)
            {
                foreach (Vertex v in scn.vert)
                {
                    v.color.Scale(0.12f, 0.21f, 0.32f, 1f);
                    v.color_morph.Scale(0.12f, 0.21f, 0.32f, 1f);
                }
            }
        }

        private void showConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllocConsole();
        }


        private void button7_Click_1(object sender, EventArgs e)
        {
            if (scn != null)
            {
                foreach (Vertex v in scn.vert)
                {
                    v.color_morph = v.color;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (scn != null)
            {
                float x = Single.Parse(maskedTextBox1.Text);

                foreach (Vertex v in scn.vert)
                {
                    v.coord.Scale(x);
                }
            }
        }
    }
}
