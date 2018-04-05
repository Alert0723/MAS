using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace Adaptable_Studio
{
    class Dynamic_code
    {
        /// <summary> 编译动态代码 </summary>
        /// <param name="method">代码文本字符串</param>
        /// <returns></returns>
        public static string Compile(string method)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("namespace DynamicCodeGenerate");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("      public class DynamicClass");
            sb.Append(Environment.NewLine);
            sb.Append("      {");
            sb.Append(Environment.NewLine);

            sb.Append(method);

            sb.Append(Environment.NewLine);
            sb.Append("      }");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            //将被编译的代码
            string code = sb.ToString();

            // 1.CSharpCodePrivoder
            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();

            // 2.ICodeComplier
            ICodeCompiler objICodeCompiler = objCSharpCodePrivoder.CreateCompiler();

            // 3.CompilerParameters
            CompilerParameters objCompilerParameters = new CompilerParameters();
            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;

            // 4.CompilerResults
            CompilerResults cr = objICodeCompiler.CompileAssemblyFromSource(objCompilerParameters, code);

            if (cr.Errors.HasErrors)
            {
                string error = "编译错误：\r\n";

                foreach (CompilerError err in cr.Errors) error += err.ErrorText + "\r\n";
                return error;
            }
            else
            {
                // 通过反射，调用类的实例
                Assembly objAssembly = cr.CompiledAssembly;
                object objClass = objAssembly.CreateInstance("DynamicCodeGenerate.DynamicClass");
                MethodInfo objMI = objClass.GetType().GetMethod("Main");

                //Console.WriteLine(objMI.Invoke(objClass, null));
                string a = objMI.Invoke(objClass, null).ToString();
                return a;
            }
        }
    }
}