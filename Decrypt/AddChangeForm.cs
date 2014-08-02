using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Resources;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading;
using WzLib;

namespace Decrypt
{
    public partial class AddChangeForm : Form
    {
        private string name;
        private object data;
        private TreeNode tree;
        private int method;

        public AddChangeForm()
        {
            InitializeComponent();
        }

        public void getData(TreeNode recvtree, int recvmeth)
        {
            name = recvtree.Text;
            if (recvtree.Tag is WzFile)
            {
                data = ((WzFile)recvtree.Tag).WzDirectory;
            }
            else
            {
                data = recvtree.Tag;
            }
            tree = recvtree;
            method = recvmeth;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (method == 0)
            {
                switch (data.GetType().Name)
                {
                    case "WzDirectory":
                        ((WzDirectory)data).Name = textBox1.Text;
                        break;
                    case "WzImage":
                        ((WzImage)data).Name = textBox1.Text;
                        ((WzImage)data).changed = true;
                        tree.Text = textBox1.Text;
                        Close();
                        return;
                    case "WzSubProperty":
                        ((WzSubProperty)data).Name = textBox1.Text;
                        break;
                    case "WzCompressedIntProperty":
                        ((WzCompressedIntProperty)data).Name = textBox1.Text;
                        ((WzCompressedIntProperty)data).Value = Convert.ToInt32(textBox2.Text);
                        break;
                    case "WzCanvasProperty":
                        if (((WzCanvasProperty)data).PngProperty == null)
                        {
                            ((WzCanvasProperty)data).PngProperty = new WzPngProperty();
                        }
                        ((WzCanvasProperty)data).PngProperty.Name = textBox1.Text;
                        ((WzCanvasProperty)data).PngProperty.Height = pictureBox1.Image.Height;
                        ((WzCanvasProperty)data).PngProperty.Width = pictureBox1.Image.Width;
                        ((WzCanvasProperty)data).PngProperty.PNG = (Bitmap)pictureBox1.Image;
                        break;
                    case "WzVectorProperty":
                        ((WzVectorProperty)data).Name = textBox1.Text;
                        ((WzVectorProperty)data).X.Value = Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[0]);
                        ((WzVectorProperty)data).Y.Value = Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[1]);
                        break;
                    case "WzStringProperty":
                        ((WzStringProperty)data).Name = textBox1.Text;
                        ((WzStringProperty)data).Value = textBox2.Text;
                        break;
                    case "WzUOLProperty":
                        ((WzUOLProperty)data).Name = textBox1.Text;
                        ((WzUOLProperty)data).Value = textBox2.Text;
                        break;
                    case "WzDoubleProperty":
                        ((WzDoubleProperty)data).Name = textBox1.Text;
                        ((WzDoubleProperty)data).Value = Convert.ToDouble(textBox2.Text);
                        break;
                    case "WzByteFloatProperty":
                        ((WzByteFloatProperty)data).Name = textBox1.Text;
                        ((WzByteFloatProperty)data).Value = Convert.ToSingle(textBox2.Text);
                        break;
                    case "WzSoundProperty":
                        ((WzSoundProperty)data).Name = textBox1.Text;
                        FileStream readmp3 = File.OpenRead(textBox2.Text);
                        byte[] mpd = new byte[readmp3.Length];
                        readmp3.Read(mpd,0,(int)readmp3.Length);
                        ((WzSoundProperty)data).SoundData = mpd;
                        readmp3.Close();
                        break;
                    case "WzConvexProperty":
                        ((WzConvexProperty)data).Name = textBox1.Text;
                        break;
                    case "WzUnsignedShortProperty":
                        ((WzUnsignedShortProperty)data).Name = textBox1.Text;
                        ((WzUnsignedShortProperty)data).Value = Convert.ToUInt16(textBox2.Text);
                        break;
                    default:
                        break;
                }
                tree.Text = textBox1.Text;
                ((IWzImageProperty)data).ParentImage.changed = true;
                Close();
            }
            else if (method == 1)
            {
                switch (data.GetType().Name)
                {
                    case "WzDirectory":
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                WzDirectory dir = new WzDirectory(textBox1.Text);
                                ((WzDirectory)data).AddDirectory(dir);
                                tree.Nodes.Add(textBox1.Text).Tag = dir;
                                break;
                            case 1:
                                WzImage img = new WzImage(textBox1.Text);
                                ((WzDirectory)data).AddImage(img);
                                tree.Nodes.Add(textBox1.Text).Tag = img;
                                img.changed = true;
                                break;
                        }
                        Close();
                        return;
                        break;
                    case "WzImage":
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                WzCompressedIntProperty integer = new WzCompressedIntProperty(textBox1.Text, Convert.ToInt32(textBox2.Text));
                                ((WzImage)data).AddProperty(integer);
                                tree.Nodes.Add(textBox1.Text).Tag = integer;
                                break;
                            case 1:
                                WzCanvasProperty png = new WzCanvasProperty(textBox1.Text);
                                (png.PngProperty = new WzPngProperty()).PNG = (Bitmap)pictureBox1.Image;
                                png.PngProperty.Height = pictureBox1.Image.Height;
                                png.PngProperty.Width = pictureBox1.Image.Width;
                                ((WzImage)data).AddProperty(png);
                                tree.Nodes.Add(textBox1.Text).Tag = png;
                                break;
                            case 2:
                                WzVectorProperty vector = new WzVectorProperty(textBox1.Text, new WzCompressedIntProperty("X", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[0])), new WzCompressedIntProperty("Y", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[1])));
                                ((WzImage)data).AddProperty(vector);
                                tree.Nodes.Add(textBox1.Text).Tag = vector;
                                break;
                            case 3:
                                WzStringProperty str = new WzStringProperty(textBox1.Text, textBox2.Text);
                                ((WzImage)data).AddProperty(str);
                                tree.Nodes.Add(textBox1.Text).Tag = str;
                                break;
                            case 4:
                                WzUOLProperty uol = new WzUOLProperty(textBox1.Text, textBox2.Text);
                                ((WzImage)data).AddProperty(uol);
                                tree.Nodes.Add(textBox1.Text).Tag = uol;
                                break;
                            case 5:
                                WzDoubleProperty dou = new WzDoubleProperty(textBox1.Text, Convert.ToDouble(textBox2.Text));
                                ((WzImage)data).AddProperty(dou);
                                tree.Nodes.Add(textBox1.Text).Tag = dou;
                                break;
                            case 6:
                                WzByteFloatProperty flo = new WzByteFloatProperty(textBox1.Text, Convert.ToSingle(textBox2.Text));
                                ((WzImage)data).AddProperty(flo);
                                tree.Nodes.Add(textBox1.Text).Tag = flo;
                                break;
                            case 7:
                                WzSubProperty sub = new WzSubProperty(textBox1.Text);
                                ((WzImage)data).AddProperty(sub);
                                tree.Nodes.Add(textBox1.Text).Tag = sub;
                                break;
                            case 8:
                                WzSoundProperty mps = new WzSoundProperty(textBox1.Text);
                                FileStream readsound = File.OpenRead(textBox2.Text);
                                byte[] mpdata = new byte[readsound.Length];
                                readsound.Read(mpdata, 0, (int)readsound.Length);
                                mps.SoundData = mpdata;
                                ((WzImage)data).AddProperty(mps);
                                tree.Nodes.Add(textBox1.Text).Tag = mps;
                                readsound.Close();
                                break;
                            case 9:
                                WzConvexProperty convex = new WzConvexProperty(textBox1.Text);
                                ((WzImage)data).AddProperty(convex);
                                tree.Nodes.Add(textBox1.Text).Tag = convex;
                                break;
                            case 10:
                                WzUnsignedShortProperty us = new WzUnsignedShortProperty(textBox1.Text, Convert.ToUInt16(textBox2.Text));
                                ((WzImage)data).AddProperty(us);
                                tree.Nodes.Add(textBox1.Text).Tag = us;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "WzSubProperty":
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                WzCompressedIntProperty integer = new WzCompressedIntProperty(textBox1.Text, Convert.ToInt32(textBox2.Text));
                                ((WzSubProperty)data).AddProperty(integer);
                                tree.Nodes.Add(textBox1.Text).Tag = integer;
                                break;
                            case 1:
                                WzCanvasProperty png = new WzCanvasProperty(textBox1.Text);
                                (png.PngProperty = new WzPngProperty()).PNG = (Bitmap)pictureBox1.Image;
                                png.PngProperty.Height = pictureBox1.Image.Height;
                                png.PngProperty.Width = pictureBox1.Image.Width;
                                ((WzSubProperty)data).AddProperty(png);
                                tree.Nodes.Add(textBox1.Text).Tag = png;
                                break;
                            case 2:
                                WzVectorProperty vector = new WzVectorProperty(textBox1.Text, new WzCompressedIntProperty("X", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[0])), new WzCompressedIntProperty("Y", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[1])));
                                ((WzSubProperty)data).AddProperty(vector);
                                tree.Nodes.Add(textBox1.Text).Tag = vector;
                                break;
                            case 3:
                                WzStringProperty str = new WzStringProperty(textBox1.Text, textBox2.Text);
                                ((WzSubProperty)data).AddProperty(str);
                                tree.Nodes.Add(textBox1.Text).Tag = str;
                                break;
                            case 4:
                                WzUOLProperty uol = new WzUOLProperty(textBox1.Text, textBox2.Text);
                                ((WzSubProperty)data).AddProperty(uol);
                                tree.Nodes.Add(textBox1.Text).Tag = uol;
                                break;
                            case 5:
                                WzDoubleProperty dou = new WzDoubleProperty(textBox1.Text, Convert.ToDouble(textBox2.Text));
                                ((WzSubProperty)data).AddProperty(dou);
                                tree.Nodes.Add(textBox1.Text).Tag = dou;
                                break;
                            case 6:
                                WzByteFloatProperty flo = new WzByteFloatProperty(textBox1.Text, Convert.ToSingle(textBox2.Text));
                                ((WzSubProperty)data).AddProperty(flo);
                                tree.Nodes.Add(textBox1.Text).Tag = flo;
                                break;
                            case 7:
                                WzSubProperty sub = new WzSubProperty(textBox1.Text);
                                ((WzSubProperty)data).AddProperty(sub);
                                tree.Nodes.Add(textBox1.Text).Tag = sub;
                                break;
                            case 8:
                                WzSoundProperty mps = new WzSoundProperty(textBox1.Text);
                                FileStream readsound = File.OpenRead(textBox2.Text);
                                byte[] mpdata = new byte[readsound.Length];
                                readsound.Read(mpdata, 0, (int)readsound.Length);
                                mps.SoundData = mpdata;
                                ((WzSubProperty)data).AddProperty(mps);
                                tree.Nodes.Add(textBox1.Text).Tag = mps;
                                readsound.Close();
                                break;
                            case 9:
                                WzConvexProperty convex = new WzConvexProperty(textBox1.Text);
                                ((WzSubProperty)data).AddProperty(convex);
                                tree.Nodes.Add(textBox1.Text).Tag = convex;
                                break;
                            case 10:
                                WzUnsignedShortProperty us = new WzUnsignedShortProperty(textBox1.Text, Convert.ToUInt16(textBox2.Text));
                                ((WzSubProperty)data).AddProperty(us);
                                tree.Nodes.Add(textBox1.Text).Tag = us;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "WzCanvasProperty":
                        WzCanvasProperty canvas = (WzCanvasProperty)data;
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                WzCompressedIntProperty integer = new WzCompressedIntProperty(textBox1.Text, Convert.ToInt32(textBox2.Text));
                                canvas.AddProperty(integer);
                                tree.Nodes.Add(textBox1.Text).Tag = integer;
                                break;
                            case 1:
                                WzCanvasProperty png = new WzCanvasProperty(textBox1.Text);
                                png.PngProperty.PNG = (Bitmap)pictureBox1.Image;
                                png.PngProperty.Height = pictureBox1.Image.Height;
                                png.PngProperty.Width = pictureBox1.Image.Width;
                                canvas.AddProperty(png);
                                tree.Nodes.Add(textBox1.Text).Tag = png;
                                break;
                            case 2:
                                WzVectorProperty vector = new WzVectorProperty(textBox1.Text, new WzCompressedIntProperty("X", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[0])), new WzCompressedIntProperty("Y", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[1])));
                                canvas.AddProperty(vector);
                                tree.Nodes.Add(textBox1.Text).Tag = vector;
                                break;
                            case 3:
                                WzStringProperty str = new WzStringProperty(textBox1.Text, textBox2.Text);
                                canvas.AddProperty(str);
                                tree.Nodes.Add(textBox1.Text).Tag = str;
                                break;
                            case 4:
                                WzUOLProperty uol = new WzUOLProperty(textBox1.Text, textBox2.Text);
                                canvas.AddProperty(uol);
                                tree.Nodes.Add(textBox1.Text).Tag = uol;
                                break;
                            case 5:
                                WzDoubleProperty dou = new WzDoubleProperty(textBox1.Text, Convert.ToDouble(textBox2.Text));
                                canvas.AddProperty(dou);
                                tree.Nodes.Add(textBox1.Text).Tag = dou;
                                break;
                            case 6:
                                WzByteFloatProperty flo = new WzByteFloatProperty(textBox1.Text, Convert.ToSingle(textBox2.Text));
                                canvas.AddProperty(flo);
                                tree.Nodes.Add(textBox1.Text).Tag = flo;
                                break;
                            case 7:
                                WzSubProperty sub = new WzSubProperty(textBox1.Text);
                                canvas.AddProperty(sub);
                                tree.Nodes.Add(textBox1.Text).Tag = sub;
                                break;
                            case 8:
                                WzSoundProperty mps = new WzSoundProperty(textBox1.Text);
                                FileStream readsound = File.OpenRead(textBox2.Text);
                                byte[] mpdata = new byte[readsound.Length];
                                readsound.Read(mpdata, 0, (int)readsound.Length);
                                mps.SoundData = mpdata;
                                canvas.AddProperty(mps);
                                tree.Nodes.Add(textBox1.Text).Tag = mps;
                                readsound.Close();
                                break;
                            case 9:
                                WzConvexProperty convex = new WzConvexProperty(textBox1.Text);
                                canvas.AddProperty(convex);
                                tree.Nodes.Add(textBox1.Text).Tag = convex;
                                break;
                            case 10:
                                WzUnsignedShortProperty us = new WzUnsignedShortProperty(textBox1.Text, Convert.ToUInt16(textBox2.Text));
                                canvas.AddProperty(us);
                                tree.Nodes.Add(textBox1.Text).Tag = us;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "WzConvexProperty":
                        WzExtendedProperty ext = new WzExtendedProperty(textBox1.Text);
                        switch (comboBox1.SelectedIndex)
                        {
                            case 0:
                                WzCompressedIntProperty integer = new WzCompressedIntProperty(textBox1.Text, Convert.ToInt32(textBox2.Text));
                                ext.ExtendedProperty = integer;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = integer;
                                break;
                            case 1:
                                WzCanvasProperty png = new WzCanvasProperty(textBox1.Text);
                                (png.PngProperty = new WzPngProperty()).PNG = (Bitmap)pictureBox1.Image;
                                png.PngProperty.Height = pictureBox1.Image.Height;
                                png.PngProperty.Width = pictureBox1.Image.Width;
                                ext.ExtendedProperty = png;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = png;
                                break;
                            case 2:
                                WzVectorProperty vector = new WzVectorProperty(textBox1.Text, new WzCompressedIntProperty("X", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[0])), new WzCompressedIntProperty("Y", Convert.ToInt32(textBox2.Text.Split(Convert.ToChar(","))[1])));
                                ext.ExtendedProperty = vector;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = vector;
                                break;
                            case 3:
                                WzStringProperty str = new WzStringProperty(textBox1.Text, textBox2.Text);
                                ext.ExtendedProperty = str;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = str;
                                break;
                            case 4:
                                WzUOLProperty uol = new WzUOLProperty(textBox1.Text, textBox2.Text);
                                ext.ExtendedProperty = uol;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = uol;
                                break;
                            case 5:
                                WzDoubleProperty dou = new WzDoubleProperty(textBox1.Text, Convert.ToDouble(textBox2.Text));
                                ext.ExtendedProperty = dou;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = dou;
                                break;
                            case 6:
                                WzByteFloatProperty flo = new WzByteFloatProperty(textBox1.Text, Convert.ToSingle(textBox2.Text));
                                ext.ExtendedProperty = flo;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = flo;
                                break;
                            case 7:
                                WzSubProperty sub = new WzSubProperty(textBox1.Text);
                                ext.ExtendedProperty = sub;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = sub;
                                break;
                            case 8:
                                WzSoundProperty mps = new WzSoundProperty(textBox1.Text);
                                FileStream readsound = File.OpenRead(textBox2.Text);
                                byte[] mpdata = new byte[readsound.Length];
                                readsound.Read(mpdata, 0, (int)readsound.Length);
                                mps.SoundData = mpdata;
                                ext.ExtendedProperty = mps;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = mps;
                                readsound.Close();
                                break;
                            case 9:
                                WzConvexProperty convex = new WzConvexProperty(textBox1.Text);
                                ext.ExtendedProperty = convex;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = convex;
                                break;
                            case 10:
                                WzUnsignedShortProperty us = new WzUnsignedShortProperty(textBox1.Text, Convert.ToUInt16(textBox2.Text));
                                ext.ExtendedProperty = us;
                                ((WzConvexProperty)data).AddProperty(ext);
                                tree.Nodes.Add(textBox1.Text).Tag = us;
                                break;
                            default:
                                break;
                        }
                        break;
                }
                if (data is WzImage)
                    ((WzImage)data).changed = true;
                else
                    ((IWzImageProperty)data).ParentImage.changed = true;
                Close();
            }
        }

        private void AddChangeForm_Load(object sender, EventArgs e)
        {
            button2.Visible = false;
                if (data.GetType().Name == "WzDirectory")
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(new object[] { "WzDirectory", "WzImage" });
                }
            if (method == 0)
            {
                textBox1.Text = name;
                comboBox1.Enabled = false;
                switch (data.GetType().Name)
                {
                    case "WzDirectory":
                        comboBox1.SelectedIndex = 0;
                        textBox2.Enabled = false;
                        break;
                    case "WzImage":
                        comboBox1.Items.Add("WzImage");
                        comboBox1.SelectedIndex = 11;
                        textBox2.Enabled = false;
                        break;
                    case "WzSubProperty":
                        comboBox1.SelectedIndex = 7;
                        textBox2.Enabled = false;
                        break;
                    case "WzCompressedIntProperty":
                        textBox2.Text = Convert.ToString(((WzCompressedIntProperty)data).Value);
                        comboBox1.SelectedIndex = 0;
                        break;
                    case "WzCanvasProperty":
                        button2.Visible = true;
                        try
                        {
                            pictureBox1.Image = ((WzCanvasProperty)data).PngProperty.PNG;
                        }
                        catch
                        {
                            pictureBox1.Image = null;
                        }
                        comboBox1.SelectedIndex = 1;
                        break;
                    case "WzVectorProperty":
                        textBox2.Text = Convert.ToString(((WzVectorProperty)data).X.Value) + "," + Convert.ToString(((WzVectorProperty)data).Y.Value);
                        comboBox1.SelectedIndex = 2;
                        break;
                    case "WzStringProperty":
                        textBox2.Text = ((WzStringProperty)data).Value;
                        comboBox1.SelectedIndex = 3;
                        break;
                    case "WzUOLProperty":
                        textBox2.Text = ((WzUOLProperty)data).Value;
                        comboBox1.SelectedIndex = 4;
                        break;
                    case "WzDoubleProperty":
                        textBox2.Text = Convert.ToString(((WzDoubleProperty)data).Value);
                        comboBox1.SelectedIndex = 5;
                        break;
                    case "WzByteFloatProperty":
                        textBox2.Text = Convert.ToString(((WzByteFloatProperty)data).Value);
                        comboBox1.SelectedIndex = 6;
                        break;
                    case "WzSoundProperty":
                        comboBox1.SelectedIndex = 8;
                        break;
                    case "WzConvexProperty":
                        comboBox1.SelectedIndex = 9;
                        textBox2.Enabled = false;
                        break;
                    case "WzUnsignedShortProperty":
                        comboBox1.SelectedIndex = 10;
                        break;
                    default:
                        break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog pngopen = new OpenFileDialog();
            pngopen.Title = "Select the png file...";
            pngopen.Filter = "PNG file|*.png";
            if (pngopen.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            pictureBox1.Image = new Bitmap(pngopen.FileName);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            button2.Visible = false;
            textBox2.Enabled = true;
            if ((string)comboBox1.Items[comboBox1.SelectedIndex] == "WzCanvasProperty")
            {
                button2.Visible = true;
            }
            else if ((string)comboBox1.Items[comboBox1.SelectedIndex] == "WzSubProperty" || (string)comboBox1.Items[comboBox1.SelectedIndex] == "WzImage" || (string)comboBox1.Items[comboBox1.SelectedIndex] == "WzDirectory")
            {
                textBox2.Enabled = false;
            }
        }

        private void AddChangeForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }
    }
}
