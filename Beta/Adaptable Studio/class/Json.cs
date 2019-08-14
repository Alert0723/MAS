using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Adaptable_Studio
{
    class Json
    {
        public List<string> CN = null;
        public List<string> EN = null;

        /// <summary> Json反序列化 </summary>
        /// <param name="ReaderPath">文件路径</param>
        /// <param name="json">Json类型变量</param>
        public static void Deserialize(string ReaderPath, ref Json json)
        {
            using (StreamReader line = new StreamReader(ReaderPath))
            {
                try
                {
                    string JSONcontent = line.ReadToEnd();
                    json = JsonConvert.DeserializeObject<Json>(JSONcontent);
                }
                catch { MainWindow.Log_Write(MainWindow.LogPath, "json文件反序列化失败"); }
            }
        }
    }
}
