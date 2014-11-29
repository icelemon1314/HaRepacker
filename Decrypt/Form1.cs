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
using System.Xml;
using WzLib;

namespace Decrypt
{
    public partial class Form1 : Form
    {
        public WzListFile wzfl;
        public string propname;
        public object propdata;
        public delegate void delPassData(TreeNode tree, int method);
        public WzImage copypasta;
        public int searchidx = 0;
        public int currentidx = 0;
        public List<WzFile> WzFiles = new List<WzFile>();
        public bool finished = false;
        public bool usebasepng = false;
        public bool combineimgs = false;

        public Form1()
        {
            InitializeComponent();
        }

        // 载入wz文件
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog wzopen = new OpenFileDialog();
            wzopen.Title = "Select the wz file...";
            wzopen.Filter = "MapleStory wz file|*.wz";
            // 打开失败
            if (!(wzopen.ShowDialog() == DialogResult.OK) || !(Path.GetExtension(wzopen.FileName) == ".wz"))
            {
                return;
            }
            if (Path.GetFileName(wzopen.FileName) == "List.wz")
            {
                // 特殊处理List.wz文件
                wzfl = new WzListFile(wzopen.FileName);
                wzfl.ParseWzFile();
                TreeNode newnode = treeView1.Nodes.Add("List.wz");
                foreach (string entry in wzfl.WzListEntries)
                {
                    newnode.Nodes.Add(entry);
                }
            }
            else
            {
                load_file(wzopen.FileName);
            }
        }

        private void RecursiveTV(WzDirectory dir, TreeNode lastnode)
        {
            foreach (WzDirectory subdir in dir.WzDirectories)
            {
                TreeNode newnode = lastnode.Nodes.Add(subdir.Name);
                newnode.Tag = subdir;
                RecursiveTV(subdir, newnode);
            }
            foreach (WzImage subimg in dir.WzImages)
            {
                TreeNode newnode = lastnode.Nodes.Add(subimg.Name);
                newnode.Tag = subimg;
            }
        }

        private TreeNode GetNode(string name)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            return null;
        }

        private void ExtractSubProperty(IWzImageProperty[] subprop, TreeNode parent)
        {
            foreach (IWzImageProperty prop in subprop)
            {
                TreeNode newnode;
                switch (prop.GetType().Name)
                {
                    case "WzExtendedProperty":
                        WzExtendedProperty extprop = (WzExtendedProperty)prop;
                        newnode = parent.Nodes.Add(extprop.Name);
                        switch (extprop.ExtendedProperty.GetType().Name)
                        {
                            case "WzSubProperty":
                                newnode.Tag = (WzSubProperty)extprop.ExtendedProperty;
                                if (((WzSubProperty)extprop.ExtendedProperty).WzProperties.Length != 0)
                                {
                                    ExtractSubProperty(((WzSubProperty)extprop.ExtendedProperty).WzProperties, newnode);
                                }
                                break;
                            case "WzCanvasProperty":
                                WzCanvasProperty canvas = (WzCanvasProperty)extprop.ExtendedProperty;
                                newnode.Tag = canvas;//.PngProperty;
                                if (canvas.WzProperties.Length != 0)
                                {
                                    ExtractSubProperty(canvas.WzProperties, newnode);
                                }
                                break;
                            case "WzVectorProperty":
                                newnode.Tag = (WzVectorProperty)extprop.ExtendedProperty;
                                break;
                            case "WzUOLProperty":
                                newnode.Tag = (WzUOLProperty)extprop.ExtendedProperty;
                                break;
                            case "WzConvexProperty":
                                newnode.Tag = (WzConvexProperty)extprop.ExtendedProperty;
                                ExtractSubProperty(((WzConvexProperty)extprop.ExtendedProperty).WzProperties, newnode);
                                break;
                            case "WzSoundProperty":
                                newnode.Tag = (WzSoundProperty)extprop.ExtendedProperty;
                                break;
                            default:
                                string asdf = extprop.ExtendedProperty.GetType().Name;
                                break;
                        }
                        break;
                    case "WzCompressedIntProperty":
                        newnode = parent.Nodes.Add(prop.Name);
                        newnode.Tag = (WzCompressedIntProperty)prop;
                        break;
                    case "WzStringProperty":
                        newnode = parent.Nodes.Add(prop.Name);
                        newnode.Tag = (WzStringProperty)prop;
                        break;
                    case "WzNullProperty":
                        newnode = parent.Nodes.Add(prop.Name);
                        break;
                    case "WzDoubleProperty":
                        newnode = parent.Nodes.Add(prop.Name);
                        newnode.Tag = (WzDoubleProperty)prop;
                        break;
                    case "WzByteFloatProperty":
                        newnode = parent.Nodes.Add(prop.Name);
                        newnode.Tag = (WzByteFloatProperty)prop;
                        break;
                    case "WzUnsignedShortProperty":
                        newnode = parent.Nodes.Add(prop.Name);
                        newnode.Tag = (WzUnsignedShortProperty)prop;
                        break;
                    default:
                        string asfd = prop.GetType().Name;
                        break;
                }
            }
        }

        private void ExtractImg(WzImage img, TreeNode parent)
        {
            foreach (IWzImageProperty prop in img.WzProperties)
            {
                WzExtendedProperty extprop = (WzExtendedProperty)prop;
                TreeNode newnode = parent.Nodes.Add(extprop.Name);
                WzSubProperty subprop = (WzSubProperty)extprop.ExtendedProperty;
                if (subprop.WzProperties.Length != 0)
                {
                    newnode.Tag = subprop;
                    ExtractSubProperty(subprop.WzProperties, newnode);
                }
            }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Nodes.Count != 0)
                return;
            object data = treeView1.SelectedNode.Tag;
            if (data == null)
                return;
            switch (data.GetType().Name)
            {
                case "WzImage":
                    if (!((WzImage)data).Parsed)
                    {
                        try
                        {
                            ((WzImage)data).ParseImage();
                        }
                        catch
                        {
                        }
                    }
                    ExtractSubProperty(((WzImage)data).WzProperties, treeView1.SelectedNode);
                    treeView1.SelectedNode.Expand();
                    Sort();
                    break;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            button2.Visible = false;
            button4.Visible = false;
            pictureBox1.Image = null;
            object data = treeView1.SelectedNode.Tag;
            if (data == null)
            {
                textBox1.Visible = false;
                return;
            }
            switch (data.GetType().Name)
            {
                case "WzFile":
                    textBox1.Visible = false;
                    toolStripStatusLabel1.Text = "Selection Type: WzFile";
                    break;
                case "WzImage":
                    toolStripStatusLabel1.Text = "Selection Type: WzImage";
                    textBox1.Visible = false;
                    break;
                case "WzDirectory":
                    toolStripStatusLabel1.Text = "Selection Type: WzDirectory";
                    textBox1.Visible = false;
                    break;
                case "WzCompressedIntProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzCompressedIntProperty";
                    textBox1.Text = Convert.ToString(((WzCompressedIntProperty)data).Value);
                    textBox1.Visible = true;
                    break;
                case "WzCanvasProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzCanvasProperty";
                    button2.Visible = true;
                    textBox1.Visible = false;
                    try
                    {
                        pictureBox1.Image = ((WzCanvasProperty)data).PngProperty.PNG;
                    }
                    catch
                    {
                        pictureBox1.Image = null;
                    }
                    break;
                case "WzVectorProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzVectorProperty";
                    textBox1.Visible = true;
                    textBox1.Text = "X: " + Convert.ToString(((WzVectorProperty)data).X.Value) + "\r\n" + "Y: " + Convert.ToString(((WzVectorProperty)data).Y.Value);
                    break;
                case "WzStringProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzStringProperty";
                    textBox1.Visible = true;
                    textBox1.Text = ((WzStringProperty)data).Value;
                    break;
                case "WzUOLProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzUOLProperty";
                    textBox1.Text = "Link (UOL) to " + ((WzUOLProperty)data).Value;
                    textBox1.Visible = true;
                    break;
                case "WzSubProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzSubProperty";
                    break;
                case "WzConvexProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzConvexProperty";
                    textBox1.Visible = false;
                    break;
                case "WzSoundProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzSoundProperty";
                    button4.Visible = true;
                    break;
                case "WzDoubleProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzDoubleProperty";
                    textBox1.Visible = true;
                    textBox1.Text = Convert.ToString(((WzDoubleProperty)data).Value);
                    break;
                case "WzByteFloatProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzByteFloatProperty";
                    textBox1.Visible = true;
                    textBox1.Text = Convert.ToString(((WzByteFloatProperty)data).Value);
                    break;
                case "WzUnsignedShortProperty":
                    toolStripStatusLabel1.Text = "Selection Type: WzUnsignedShortProperty";
                    textBox1.Visible = true;
                    textBox1.Text = Convert.ToString(((WzUnsignedShortProperty)data).Value);
                    break;
                default:
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog pngsave = new SaveFileDialog();
            pngsave.Title = "Select where to save...";
            pngsave.Filter = "PNG file|*.png";
            if (!(pngsave.ShowDialog() == DialogResult.OK) || !(Path.GetExtension(pngsave.FileName) == ".png"))
            {
                return;
            }
            pictureBox1.Image.Save(pngsave.FileName);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is WzImage)
            {
                removeImgToolStripMenuItem_Click(null, null);
                return;
            }
            else if (treeView1.SelectedNode.Tag is WzDirectory)
            {
                removeDirectoryToolStripMenuItem_Click(null, null);
                return;
            }
            try
            {
                string toremove = treeView1.SelectedNode.Text;
                switch (treeView1.SelectedNode.Parent.Tag.GetType().Name)
                {
                    case "WzImage":
                        ((WzImage)treeView1.SelectedNode.Parent.Tag).RemoveProperty(toremove);
                        ((WzImage)treeView1.SelectedNode.Parent.Tag).changed = true;
                        treeView1.SelectedNode.Remove();
                        break;
                    case "WzSubProperty":
                        ((WzSubProperty)treeView1.SelectedNode.Parent.Tag).RemoveProperty(toremove);
                        ((WzSubProperty)treeView1.SelectedNode.Parent.Tag).ParentImage.changed = true;
                        treeView1.SelectedNode.Remove();
                        break;
                    case "WzCanvasProperty":
                        ((WzCanvasProperty)treeView1.SelectedNode.Parent.Tag).RemoveProperty(toremove);
                        ((WzCanvasProperty)treeView1.SelectedNode.Parent.Tag).ParentImage.changed = true;
                        treeView1.SelectedNode.Remove();
                        break;
                    case "WzConvexProperty":
                        ((WzConvexProperty)treeView1.SelectedNode.Parent.Tag).RemoveProperty(toremove);
                        ((WzConvexProperty)treeView1.SelectedNode.Parent.Tag).ParentImage.changed = true;
                        treeView1.SelectedNode.Remove();
                        break;
                    default:
                        MessageBox.Show("the selected item is not an inside-img property");
                        break;
                }
            }
            catch
            {
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog mp3save = new SaveFileDialog();
            mp3save.Title = "Select where to save...";
            mp3save.Filter = "MP3 file|*.mp3";
            if (!(mp3save.ShowDialog() == DialogResult.OK) || !(Path.GetExtension(mp3save.FileName) == ".mp3"))
            {
                return;
            }
            byte[] data = ((WzSoundProperty)treeView1.SelectedNode.Tag).SoundData;
            FileStream mp3 = File.OpenWrite(mp3save.FileName);
            mp3.Write(data, 0, data.Length);
            mp3.Close();
        }

        private void Refresh(object obj, FormClosedEventArgs e)
        {
            treeView1.Refresh();
            treeView1_AfterSelect(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                AddChangeForm form2 = new AddChangeForm();
                delPassData del = new delPassData(form2.getData);
                del(treeView1.SelectedNode, 0);
                form2.Show();
                form2.FormClosed += new FormClosedEventHandler(Refresh);
            }
            catch
            {
            }
        }

        // 加载wz文件
        public WzFile load_file(string path)
        {
            try
            {
                WzMapleVersion ver;
                bool jmsmob = false;
                if (comboBox1.SelectedIndex == 3)
                {
                    ver = WzMapleVersion.BMS;
                    if (Path.GetFileName(path).ToLower() == "mob.wz")
                        jmsmob = true;
                }
                else
                {
                    ver = (WzMapleVersion)comboBox1.SelectedIndex;
                }
                WzFile wzf = new WzFile(path, ver);
                wzf.ParseWzFile();
                TreeNode newnode = treeView1.Nodes.Add(wzf.WzDirectory.Name);
                newnode.Tag = wzf;
                RecursiveTV(wzf.WzDirectory, newnode);
                WzFiles.Add(wzf);
                Sort();
                if (jmsmob)
                {
                    WzTools.CreateWzKey(WzMapleVersion.EMS);
                }
                return wzf;
            }
            catch
            {
                MessageBox.Show("The wz file is either in use, or you dont have .NET 3.5");
                return null;
            }
        }

        private void Sort()
        {
            if (checkBox1.Checked)
            {
                treeView1.Sort();
            }
        }

        public void SafeSave(WzFile wzf, string savepath)
        {
            WzFile dispose = null;
            foreach (WzFile wz in WzFiles)
            {
                if (wz.Path == savepath)
                {
                    dispose = wz;
                }
            }
            if (dispose != null && savepath == dispose.Path)
            {
                if (MessageBox.Show("do you want to overwrite the current wz file?", "overwrite", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
                WzFiles.Remove(dispose);
                wzf.SaveToDisk(savepath + ".temp");
                dispose.Dispose();
                File.Delete(savepath);
                File.Move(savepath + ".temp", savepath);
                treeView1.Nodes.Clear();
                foreach (WzFile file in WzFiles)
                {
                    file.Dispose();
                }
                WzFiles = new List<WzFile>();
                load_file(savepath);
            }
            else
            {
                wzf.SaveToDisk(savepath);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog wzsave = new SaveFileDialog();
                wzsave.Title = "Select where to save...";
                wzsave.Filter = "Wz file|*.wz";
                if (!(wzsave.ShowDialog() == DialogResult.OK) || !(Path.GetExtension(wzsave.FileName) == ".wz"))
                {
                    return;
                }
                if (treeView1.SelectedNode.Tag is WzFile)
                {
                    SafeSave(((WzFile)treeView1.SelectedNode.Tag), wzsave.FileName);
                }
                else if (WzFiles.Count == 1)
                {
                    SafeSave(WzFiles[0], wzsave.FileName);
                }
                else
                {
                    MessageBox.Show("There are more than 1 Wz File opened. please select the one you want to save.");
                }
            }
            catch
            {
                MessageBox.Show("Saving the file failed.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag == null)
                return;
            string type = treeView1.SelectedNode.Tag.GetType().Name;
            if (type != "WzImage" && type != "WzSubProperty" && type != "WzCanvasProperty" && type != "WzDirectory" && type != "WzFile" && type != "WzConvexProperty")
            {
                MessageBox.Show("The selected property is not a directory and cannot have childs");
                return;
            }
            AddChangeForm form2 = new AddChangeForm();
            delPassData del = new delPassData(form2.getData);
            del(treeView1.SelectedNode, 1);
            form2.FormClosed += new FormClosedEventHandler(Refresh);
            form2.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void removeImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode.Tag.GetType().Name != "WzImage")
                    return;
                if (treeView1.SelectedNode.Parent.Tag.GetType().Name == "WzDirectory")
                    ((WzDirectory)treeView1.SelectedNode.Parent.Tag).RemoveImage(treeView1.SelectedNode.Text);
                else if (treeView1.SelectedNode.Parent.Tag.GetType().Name == "WzFile")
                    ((WzFile)treeView1.SelectedNode.Parent.Tag).WzDirectory.RemoveImage(treeView1.SelectedNode.Text);
                treeView1.SelectedNode.Remove();
            }
            catch
            {
            }
        }

        private void removeDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode.Tag.GetType().Name != "WzDirectory")
                    return;
                if (treeView1.SelectedNode.Parent.Tag.GetType().Name == "WzDirectory")
                    ((WzDirectory)treeView1.SelectedNode.Parent.Tag).RemoveDirectory(treeView1.SelectedNode.Text);
                else if (treeView1.SelectedNode.Parent.Tag.GetType().Name == "WzFile")
                    ((WzFile)treeView1.SelectedNode.Parent.Tag).WzDirectory.RemoveDirectory(treeView1.SelectedNode.Text);
                treeView1.SelectedNode.Remove();
            }
            catch
            {
            }
        }

        /**
         * 导出xml格式文件
         * 
         */
        public void DumpXML(ref TextWriter tw, string depth, IWzImageProperty[] props)
        {
            foreach (IWzImageProperty property in props)
            {
                if (property != null)
                {
                    IWzImageProperty extendedProperty = property;
                    if (property is WzExtendedProperty)
                    {
                        extendedProperty = ((WzExtendedProperty)property).ExtendedProperty;
                    }
                    if (extendedProperty is WzCanvasProperty)
                    {
                        WzCanvasProperty property3 = (WzCanvasProperty)extendedProperty;
                        if (usebasepng)
                        {
                            MemoryStream stream = new MemoryStream();
                            property3.PngProperty.PNG.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] pngbytes = stream.ToArray();
                            stream.Close();
                            tw.WriteLine(string.Concat(new object[] { depth, "<canvas name=\"", property3.Name, "\" width=\"", property3.PngProperty.Width, "\" height=\"", property3.PngProperty.Height, "\" basedata=\"", Convert.ToBase64String(pngbytes), "\">" }));
                        }
                        else
                        {
                            tw.WriteLine(string.Concat(new object[] { depth, "<canvas name=\"", property3.Name, "\" width=\"", property3.PngProperty.Width, "\" height=\"", property3.PngProperty.Height, "\">" }));
                        }
                        this.DumpXML(ref tw, depth + "    ", property3.WzProperties);
                        tw.WriteLine(depth + "</canvas>");
                        
                    }
                    else if (extendedProperty is WzCompressedIntProperty)
                    {
                        WzCompressedIntProperty property4 = (WzCompressedIntProperty)extendedProperty;
                        tw.WriteLine(string.Concat(new object[] { depth, "<int name=\"", property4.Name, "\" value=\"", property4.Value, "\"/>" }));
                    }
                    else if (extendedProperty is WzDoubleProperty)
                    {
                        WzDoubleProperty property5 = (WzDoubleProperty)extendedProperty;
                        tw.WriteLine(string.Concat(new object[] { depth, "<double name=\"", property5.Name, "\" value=\"", property5.Value, "\"/>" }));
                    }
                    else if (extendedProperty is WzNullProperty)
                    {
                        WzNullProperty property6 = (WzNullProperty)extendedProperty;
                        tw.WriteLine(depth + "<null name=\"" + property6.Name + "\"/>");
                    }
                    else if (extendedProperty is WzSoundProperty)
                    {
                        WzSoundProperty property7 = (WzSoundProperty)extendedProperty;
                        tw.WriteLine(depth + "<sound name=\"" + property7.Name + "\"/>");
                    }
                    else if (extendedProperty is WzStringProperty)
                    {
                        WzStringProperty property8 = (WzStringProperty)extendedProperty;
                        string str = property8.Value.Replace("<", "&lt;").Replace("&", "&amp;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");
                        tw.WriteLine(depth + "<string name=\"" + property8.Name + "\" value=\"" + str + "\"/>");
                    }
                    else if (extendedProperty is WzSubProperty)
                    {
                        WzSubProperty property9 = (WzSubProperty)extendedProperty;
                        tw.WriteLine(depth + "<imgdir name=\"" + property9.Name + "\">");
                        this.DumpXML(ref tw, depth + "    ", property9.WzProperties);
                        tw.WriteLine(depth + "</imgdir>");
                    }
                    else if (extendedProperty is WzUnsignedShortProperty)
                    {
                        WzUnsignedShortProperty property10 = (WzUnsignedShortProperty)extendedProperty;
                        tw.WriteLine(string.Concat(new object[] { depth, "<short name=\"", property10.Name, "\" value=\"", property10.Value, "\"/>" }));
                    }
                    else if (extendedProperty is WzUOLProperty)
                    {
                        WzUOLProperty property11 = (WzUOLProperty)extendedProperty;
                        tw.WriteLine(depth + "<uol name=\"" + property11.Name + "\" value=\"" + property11.Value + "\"/>");
                    }
                    else if (extendedProperty is WzVectorProperty)
                    {
                        WzVectorProperty property12 = (WzVectorProperty)extendedProperty;
                        tw.WriteLine(string.Concat(new object[] { depth, "<vector name=\"", property12.Name, "\" x=\"", property12.X.Value, "\" y=\"", property12.Y.Value, "\"/>" }));
                    }
                    else if (extendedProperty is WzByteFloatProperty)
                    {
                        WzByteFloatProperty property13 = (WzByteFloatProperty)extendedProperty;
                        string str2 = property13.Value.ToString();
                        if (!str2.Contains("."))
                        {
                            str2 = str2 + ".0";
                        }
                        tw.WriteLine(depth + "<float name=\"" + property13.Name + "\" value=\"" + str2 + "\"/>");
                    }
                    else if (extendedProperty is WzConvexProperty)
                    {
                        tw.WriteLine(depth + "<extended name=\"" + extendedProperty.Name + "\">");
                        DumpXML(ref tw, depth + "    ", ((WzConvexProperty)extendedProperty).WzProperties);
                        tw.WriteLine(depth + "</extended>");
                    }
                }
            }
        }

        public void DumpDir(WzDirectory dir, string directory)
        {
            if (dir != null)
            {
                if (!combineimgs)
                {
                    foreach (WzDirectory directory2 in dir.WzDirectories) {
                        Directory.CreateDirectory(directory + "/" + directory2.Name);
                        this.DumpDir(directory2, directory + "/" + directory2.Name);
                    }
                    foreach (WzImage image in dir.WzImages) {
                        TextWriter tw = new StreamWriter(directory + "/" + image.Name + ".xml");
                        tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                        tw.WriteLine("<imgdir name=\"" + image.Name + "\">");
                        this.DumpXML(ref tw, "    ", image.WzProperties);
                        tw.WriteLine("</imgdir>");
                        tw.Close();
                        image.Dispose();
                        GC.Collect();
                    }
                }
                else
                {
                    TextWriter tw = new StreamWriter(directory + ".xml");
                    tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                    tw.WriteLine("<xmldump>");
                    foreach (WzDirectory directory2 in dir.WzDirectories)
                    {
                        this.DumpDirRecursive(tw, directory2, "    ");
                    }
                    foreach (WzImage image in dir.WzImages)
                    {
                        tw.WriteLine("    " + "<wzimg name=\"" + image.Name + "\">");
                        this.DumpXML(ref tw, "    " + "    ", image.WzProperties);
                        tw.WriteLine("    " + "</wzimg>");
                    }
                    tw.WriteLine("</xmldump>");
                    tw.Close();
                }
            }
        }

        private void DumpDirRecursive(TextWriter tw, WzDirectory dir, string depth)
        {
            tw.WriteLine(depth + "<wzdir name=\"" + dir.Name + "\">");
            foreach (WzDirectory subdir in dir.WzDirectories)
            {
                DumpDirRecursive(tw, subdir, depth + "    ");
            }
            foreach (WzImage image in dir.WzImages)
            {
                tw.WriteLine(depth + "    " + "<wzimg name=\"" + image.Name + "\">");
                this.DumpXML(ref tw, depth + "    " + "    ", image.WzProperties);
                tw.WriteLine(depth + "    " + "</wzimg>");
            }
            tw.WriteLine(depth + "</wzdir>");
        }

        /**
         * dump xml 文件
         */
        private void dumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            usebasepng = false;
            combineimgs = false;
            if (checkBox2.Checked)
            {
                usebasepng = true;
            }
            if (checkBox3.Checked)
            {
                combineimgs = true;
            }
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag == null)
            {
                MessageBox.Show("no img selected");
                return;
            }
            if (treeView1.SelectedNode.Tag is WzImage) {
                WzImage current = (WzImage)treeView1.SelectedNode.Tag;
                TextWriter tw = new StreamWriter(current.Name + ".xml");
                tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                tw.WriteLine("<imgdir name=\"" + current.Name + "\">");
                DumpXML(ref tw, "    ", current.WzProperties);
                tw.WriteLine("</imgdir>");
                tw.Close();
            } else if (treeView1.SelectedNode.Tag is WzDirectory || treeView1.SelectedNode.Tag is WzFile) {
                WzDirectory dir;
                if (treeView1.SelectedNode.Tag is WzFile){
                    dir = ((WzFile)treeView1.SelectedNode.Tag).WzDirectory;
                }else{
                    dir = (WzDirectory)treeView1.SelectedNode.Tag;
                }
                string name = dir.Name;
                if (!combineimgs){
                    try{
                        Directory.CreateDirectory(name);
                    }catch{
                        name = name + "_";
                        Directory.CreateDirectory(name);
                    }
                }
                DumpDir(dir, name);
            } else {
                MessageBox.Show("Please choose a .img or directory");
            }
        }

        public void ParseXML(XmlElement element, IWzObject wo)
        {
            foreach (XmlNode node in element)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                XmlElement element2 = (XmlElement)node;
                if (element2.Name == "imgdir")
                {
                    WzSubProperty prop = new WzSubProperty(element2.GetAttribute("name"));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(prop);
                        this.ParseXML(element2, prop);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(prop);
                        this.ParseXML(element2, prop);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(prop);
                        this.ParseXML(element2, prop);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(prop.Name);
                        extended.ExtendedProperty = prop;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, prop);
                    }
                    continue;
                }
                if (element2.Name == "canvas")
                {
                    WzCanvasProperty property2 = new WzCanvasProperty(element2.GetAttribute("name"));
                    if (usebasepng)
                    {
                        property2.PngProperty = new WzPngProperty();
                        MemoryStream pngstream = new MemoryStream(Convert.FromBase64String(element2.GetAttribute("basedata")));
                        property2.PngProperty.PNG = (Bitmap)Image.FromStream(pngstream, true, true);
                    }
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property2);
                        this.ParseXML(element2, property2);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property2);
                        this.ParseXML(element2, property2);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property2);
                        this.ParseXML(element2, property2);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property2.Name);
                        extended.ExtendedProperty = property2;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property2);
                    }
                    continue;
                }
                if (element2.Name == "int")
                {
                    WzCompressedIntProperty property3 = new WzCompressedIntProperty(element2.GetAttribute("name"), Convert.ToInt32(element2.GetAttribute("value")));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property3);
                        this.ParseXML(element2, property3);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property3);
                        this.ParseXML(element2, property3);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property3);
                        this.ParseXML(element2, property3);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property3.Name);
                        extended.ExtendedProperty = property3;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property3);
                    }
                    continue;
                }
                if (element2.Name == "double")
                {
                    WzDoubleProperty property4 = new WzDoubleProperty(element2.GetAttribute("name"), Convert.ToDouble(element2.GetAttribute("value")));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property4);
                        this.ParseXML(element2, property4);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property4);
                        this.ParseXML(element2, property4);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property4);
                        this.ParseXML(element2, property4);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property4.Name);
                        extended.ExtendedProperty = property4;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property4);
                    }
                    continue;
                }
                if (element2.Name == "null")
                {
                    WzNullProperty property5 = new WzNullProperty(element2.GetAttribute("name"));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property5);
                        this.ParseXML(element2, property5);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property5);
                        this.ParseXML(element2, property5);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property5);
                        this.ParseXML(element2, property5);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property5.Name);
                        extended.ExtendedProperty = property5;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property5);
                    }
                    continue;
                }
                if (element2.Name == "sound")
                {
                    WzSoundProperty property6 = new WzSoundProperty(element2.GetAttribute("name"));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property6);
                        this.ParseXML(element2, property6);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property6);
                        this.ParseXML(element2, property6);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property6);
                        this.ParseXML(element2, property6);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property6.Name);
                        extended.ExtendedProperty = property6;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property6);
                    }
                    continue;
                }
                if (element2.Name == "string")
                {
                    string str = element2.GetAttribute("value").Replace("&lt;", "<").Replace("&amp;", "&").Replace("&gt;", ">").Replace("&apos;", "'").Replace("&quot;", "\"");
                    WzStringProperty property7 = new WzStringProperty(element2.GetAttribute("name"), str);
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property7);
                        this.ParseXML(element2, property7);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property7);
                        this.ParseXML(element2, property7);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property7);
                        this.ParseXML(element2, property7);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property7.Name);
                        extended.ExtendedProperty = property7;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property7);
                    }
                    continue;
                }
                if (element2.Name == "short")
                {
                    WzUnsignedShortProperty property8 = new WzUnsignedShortProperty(element2.GetAttribute("name"), Convert.ToUInt16(element2.GetAttribute("value")));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property8);
                        this.ParseXML(element2, property8);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property8);
                        this.ParseXML(element2, property8);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property8);
                        this.ParseXML(element2, property8);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property8.Name);
                        extended.ExtendedProperty = property8;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property8);
                    }
                    continue;
                }
                if (element2.Name == "uol")
                {
                    WzUOLProperty property9 = new WzUOLProperty(element2.GetAttribute("name"), element2.GetAttribute("value"));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property9);
                        this.ParseXML(element2, property9);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property9);
                        this.ParseXML(element2, property9);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property9);
                        this.ParseXML(element2, property9);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property9.Name);
                        extended.ExtendedProperty = property9;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property9);
                    }
                    continue;
                }
                if (element2.Name == "vector")
                {
                    WzVectorProperty property10 = new WzVectorProperty(element2.GetAttribute("name"), new WzCompressedIntProperty("x", Convert.ToInt32(element2.GetAttribute("x"))), new WzCompressedIntProperty("y", Convert.ToInt32(element2.GetAttribute("y"))));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property10);
                        this.ParseXML(element2, property10);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property10);
                        this.ParseXML(element2, property10);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property10);
                        this.ParseXML(element2, property10);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property10.Name);
                        extended.ExtendedProperty = property10;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property10);
                    }
                    continue;
                }
                if (element2.Name == "float")
                {
                    WzByteFloatProperty property11 = new WzByteFloatProperty(element2.GetAttribute("name"), Convert.ToSingle(element2.GetAttribute("value")));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(property11);
                        this.ParseXML(element2, property11);
                        continue;
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(property11);
                        this.ParseXML(element2, property11);
                        continue;
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(property11);
                        this.ParseXML(element2, property11);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(property11.Name);
                        extended.ExtendedProperty = property11;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, property11);
                    }
                }
                if (element2.Name == "extended")
                {
                    WzConvexProperty convex = new WzConvexProperty(element2.GetAttribute("name"));
                    if (wo is WzImage)
                    {
                        ((WzImage)wo).AddProperty(convex);
                        this.ParseXML(element2, convex);
                    }
                    else if (wo is WzSubProperty)
                    {
                        ((WzSubProperty)wo).AddProperty(convex);
                        this.ParseXML(element2, convex);
                    }
                    else if (wo is WzCanvasProperty)
                    {
                        ((WzCanvasProperty)wo).AddProperty(convex);
                        this.ParseXML(element2, convex);
                    }
                    else if (wo is WzConvexProperty)
                    {
                        WzExtendedProperty extended = new WzExtendedProperty(convex.Name);
                        extended.ExtendedProperty = convex;
                        ((WzConvexProperty)wo).AddProperty(extended);
                        ParseXML(element2, convex);
                    }
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            usebasepng = false;
            combineimgs = false;
            if (checkBox2.Checked)
            {
                usebasepng = true;
            }
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag == null || (treeView1.SelectedNode.Tag.GetType().Name != "WzDirectory" && treeView1.SelectedNode.Tag.GetType().Name != "WzFile"))
            {
                MessageBox.Show("no directory selected");
                return;
            }
            OpenFileDialog xmlopen = new OpenFileDialog();
            xmlopen.Title = "Open a XML";
            xmlopen.Filter = "XML File|*.xml";
            if (xmlopen.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            XmlDocument document = new XmlDocument();
            document.Load(xmlopen.FileName);
            foreach (XmlNode node in document)
            {
                if (node is XmlElement)
                {
                    if (node.Name == "xmldump")
                        combineimgs = true;
                    else
                        combineimgs = false;
                    break;
                }
            }
            if (!combineimgs)
            {
                WzImage wo = new WzImage();
                foreach (XmlNode node in document)
                {
                    if (node is XmlElement)
                    {
                        XmlElement element = (XmlElement)node;
                        wo = new WzImage(element.GetAttribute("name"));
                        ParseXML(element, wo);
                    }
                }
                wo.changed = true;
                if (treeView1.SelectedNode.Tag is WzDirectory)
                {
                    ((WzDirectory)treeView1.SelectedNode.Tag).AddImage(wo);
                }
                else if (treeView1.SelectedNode.Tag is WzFile)
                {
                    ((WzFile)treeView1.SelectedNode.Tag).WzDirectory.AddImage(wo);
                }
                treeView1.SelectedNode.Nodes.Add(wo.Name).Tag = wo;
            }
            else
            {
                foreach (XmlNode node in document)
                {
                    if (!(node is XmlElement))
                    {
                        continue;
                    }
                    XmlElement element = (XmlElement)node;
                    if (element.Name == "xmldump")
                    {
                        foreach (XmlElement subelement in element)
                        {
                            switch (subelement.Name)
                            {
                                case "wzdir":
                                    WzDirectory wo = new WzDirectory(subelement.GetAttribute("name"));
                                    TreeNode current = treeView1.SelectedNode.Nodes.Add(wo.Name);
                                    current.Tag = wo;
                                    ParseXMLDump(subelement, wo, current);
                                    if (treeView1.SelectedNode.Tag is WzDirectory)
                                    {
                                        ((WzDirectory)treeView1.SelectedNode.Tag).AddDirectory(wo);
                                    }
                                    else if (treeView1.SelectedNode.Tag is WzFile)
                                    {
                                        ((WzFile)treeView1.SelectedNode.Tag).WzDirectory.AddDirectory(wo);
                                    }
                                    break;
                                case "wzimg":
                                    WzImage wi = new WzImage(subelement.GetAttribute("name"));
                                    wi.changed = true;
                                    treeView1.SelectedNode.Nodes.Add(wi.Name).Tag = wi;
                                    ParseXML(subelement, wi);
                                    if (treeView1.SelectedNode.Tag is WzDirectory)
                                    {
                                        ((WzDirectory)treeView1.SelectedNode.Tag).AddImage(wi);
                                    }
                                    else if (treeView1.SelectedNode.Tag is WzFile)
                                    {
                                        ((WzFile)treeView1.SelectedNode.Tag).WzDirectory.AddImage(wi);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                        throw new Exception("xml is invalid");
                }
            }
        }

        private void ParseXMLDump(XmlElement element, WzDirectory dir, TreeNode node)
        {
            foreach (XmlElement subelement in element)
            {
                switch (subelement.Name)
                {
                    case "wzdir":
                        WzDirectory wo = new WzDirectory(subelement.GetAttribute("name"));
                        TreeNode current = node.Nodes.Add(wo.Name);
                        current.Tag = wo;
                        ParseXMLDump(subelement, wo, current);
                        dir.AddDirectory(wo);
                        break;
                    case "wzimg":
                        WzImage wi = new WzImage(subelement.GetAttribute("name"));
                        node.Nodes.Add(wi.Name).Tag = wi;
                        ParseXML(subelement, wi);
                        dir.AddImage(wi);
                        break;
                    default:
                        break;
                }
            }
        }

        private void copyImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag == null || treeView1.SelectedNode.Tag.GetType().Name != "WzImage")
            {
                return;
            }
            copypasta = ((WzImage)treeView1.SelectedNode.Tag).Clone();
        }

        private void pastaImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag == null)
            {
                return;
            }
            WzDirectory dir;
            if (treeView1.SelectedNode.Tag.GetType().Name == "WzFile")
            {
                dir = ((WzFile)treeView1.SelectedNode.Tag).WzDirectory;
            }
            else if (treeView1.SelectedNode.Tag.GetType().Name == "WzDirectory")
            {
                dir = (WzDirectory)treeView1.SelectedNode.Tag;
            }
            else
            {
                return;
            }
            WzImage newimg = copypasta.Clone();
            newimg.changed = true;
            dir.AddImage(newimg);
            treeView1.SelectedNode.Nodes.Add(copypasta.Name).Tag = newimg;
        }

        private void toolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            searchidx = 0;
        }

        private void SearchTV(TreeNode parent)
        {
            foreach (TreeNode node in parent.Nodes)
            {
                if (node.Text.Contains(toolStripTextBox2.Text))
                {
                    if (currentidx == searchidx)
                    {
                        treeView1.SelectedNode = node;
                        node.EnsureVisible();
                        treeView1.Focus();
                        finished = true;
                        searchidx++;
                        return;
                    }
                    else
                    {
                        currentidx++;
                    }
                }
                if (node.Nodes.Count != 0)
                {
                    SearchTV(node);
                    if (finished)
                        return;
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
        tryagain:
            finished = false;
            currentidx = 0;
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Text.Contains(toolStripTextBox2.Text))
                {
                    if (currentidx == searchidx)
                    {
                        treeView1.SelectedNode = node;
                        node.EnsureVisible();
                        treeView1.Focus();
                        finished = true;
                        searchidx++;
                        return;
                    }
                    else
                    {
                        currentidx++;
                    }
                }
                if (node.Nodes.Count != 0)
                {
                    SearchTV(node);
                    if (finished)
                        return;
                }
            }
            if (searchidx != 0)
            {
                searchidx = 0;
                goto tryagain;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            searchidx = 0;
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = true;
            toolStripTextBox2.Focus();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = false;
        }

        private void toolStripTextBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripButton1_Click(null, null);
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> removed = new List<string>();
            foreach (WzFile f in WzFiles)
            {
                removed.Add(f.Path);
                f.Dispose();
            }
            WzFiles = new List<WzFile>();
            treeView1.Nodes.Clear();
            foreach (string path in removed)
            {
                load_file(path);
            }
        }

        private void unloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (WzFile f in WzFiles)
            {
                f.Dispose();
            }
            WzFiles = new List<WzFile>();
            treeView1.Nodes.Clear();
        }
    }
}
