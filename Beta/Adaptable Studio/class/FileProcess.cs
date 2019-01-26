using System;
using System.IO;
using System.Reflection;

namespace FileProcess
{
    public class DLLPrecess
    {
        /// <summary> 执行动态链接库内指定函数 </summary>
        /// <param name="FilePath">dll文件路径</param>
        /// <param name="ClassName">函数所在类名</param>
        /// <param name="FuncName">函数名称</param>
        /// <param name="Parameters">函数所需参数集合</param>
        public static void ExecuteDLL(string FilePath, string ClassName, string FuncName, object[] Parameters = null)
        {
            try
            {
                //获取dll列表
                foreach (FileInfo file in new DirectoryInfo(FilePath).GetFiles("*.dll"))
                {
                    string DllPath = file.FullName;
                    Assembly assem = Assembly.LoadFile(DllPath);
                    Type[] tys = assem.GetTypes();
                    //得到所有的类型名，然后遍历，通过类型名字来区别

                    foreach (Type ty in tys)//获取类名
                    {
                        if (ty.Name == ClassName)
                        {
                            //获取不带参数的构造函数
                            ConstructorInfo Constructor = ty.GetConstructor(Type.EmptyTypes);
                            object ClassObject = Constructor.Invoke(new object[] { });//获取类的实例

                            //执行类class的方法
                            MethodInfo mi = ty.GetMethod(FuncName);
                            mi.Invoke(ClassObject, Parameters);
                        }
                    }
                }
            }
            catch (Exception e) { throw e; }
        }
    }
}
