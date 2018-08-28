using CommandStructureCreater;
using fNbt;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Windows;

namespace Adaptable_Studio
{
    /// <summary> OutPut.xaml 的交互逻辑 </summary>
    public partial class OutPut : MetroWindow
    {
        public OutPut()
        {
            InitializeComponent();
        }

        /// <summary> 复制代码 </summary>
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(Print.Text);
        }

        /// <summary> 生成NBT文件 </summary>
        private void ToNBT_Click(object sender, RoutedEventArgs e)
        {
            //另存为参数初始化
            SaveFileDialog StructureFile = new SaveFileDialog()
            {
                Title = "Save file",
                Filter = "NBT File(*.nbt)|*.nbt",
                FileName = "NbtFile"
            };

            //保存输出
            if ((bool)StructureFile.ShowDialog())
            {
                //文件名检测
                string NBTname = StructureFile.FileName;
                if (NBTname == null) NBTname = "NbtFile";

                //数据导出
                CreateNBT.CreateStructure(MainWindow.k, MainWindow.Max_length, MainWindow.Portrait, MainWindow.commands, MainWindow.Flat, "Minecraft Adaptable Studio", ref MainWindow.StructureNbt);
                var NBTFile = new NbtFile(MainWindow.StructureNbt);
                try { NBTFile.SaveToFile(NBTname, NbtCompression.GZip); }
                catch (Exception) { MessageBox.Show("被覆盖的文件正在占用进程", "Error"); }
            }
        }
    }
}