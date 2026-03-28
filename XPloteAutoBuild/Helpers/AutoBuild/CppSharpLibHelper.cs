using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace XPloteAutoBuild
{
    public class CppSharpLibHelper : ILibrary
    {
        private string _includePath;
        private string _outputPath;
        private string _libPath;

        /// <summary>
        /// 输入路径和输出路径
        /// 注意:在生成第三方DLL的实话,要引入install 这个文件夹.
        /// </summary>
        /// <param name="includePath"></param>
        /// <param name="outputPath"></param>
        //public CppSharpLibHelper(string includePath, string outputPath, string libPath)
        //{
        //    _includePath = includePath;
        //    _outputPath = outputPath;
        //    _libPath=libPath;
        //}
        //private AutoBuildModel buildData => IocHelper.gDefaultIoc.gModel;
        private AutoBuildModel buildData { get; set; }
        public CppSharpLibHelper(AutoBuildModel autoModel)
        {
            buildData = autoModel;
        }

      

        /// <summary>
        /// 后处理.在生成成功后,比如复制一些文件或者资源
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Postprocess(Driver driver, ASTContext ctx)
        {
            foreach (var module in ctx.TranslationUnits)
            {
                InternalPostprocess(driver, module);
            }
            //
        }

        /// <summary>
        /// 递归循环内部的模块.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="module"></param>
        private void InternalPostprocess(Driver driver, DeclarationContext module)
        {
            foreach (var ns in module.Namespaces)
            {
                InternalPostprocess(driver, ns);
            }
            foreach (var c in module.Classes)
            {
                c.Name = c.OriginalName;
                foreach (var f in c.Functions)
                {
                    f.Name = f.OriginalName;
                }
                foreach (var f in c.Methods)
                {
                    f.Name = f.OriginalName;
                    f.ConvertToProperty = false;
                }
            }
            foreach (var f in module.Functions)
            {
                f.Name = f.OriginalName;
            }
            foreach (var f in module.Enums)
            {
                f.Name = f.OriginalName;
            }
        }


        /// <summary>
        /// 预处理....在处理前,将需要的文件移植过来.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Preprocess(Driver driver, ASTContext ctx)
        {

        }

        /// <summary>
        /// 这里,需要写入包含的.h头文件. 以及 lib文件.
        /// </summary>
        /// <param name="driver"></param>
        /*   public void Setup(Driver driver)
           {
               driver.Options.GeneratorKind = GeneratorKind.CSharp;
               driver.Options.OutputDir = _outputPath;

               ///一个模块 StandardLib 就是一个类, 指定一个命名空间. 对照着添加接口.h
               var module = driver.Options.AddModule("StandardLib"); //这个只会生成 调用类的文件名以及类名

               ///添加命名空间
               module.OutputNamespace = "CsharpApi"; //命名空间,没有可以不添加.

               ///添加静态库
               module.IncludeDirs.Add(_includePath);   //包含的includ目录
               module.Headers.Add("StandardLib.h");
               module.Headers.Add("TestB1.h");


               ///添加静态库.
               module.LibraryDirs.Add(_libPath);
               module.Libraries.Add("StandardLib.lib");

           }

           */

        public void Setup(Driver driver)
        {
            // 将以下代码：
            // GeneratorKind gk = Enum.Parse<GeneratorKind>(buildData.gSelectedLanguage.Trim());

            // 替换为：
            GeneratorKind gk = GeneratorKind.FindGeneratorKindByID(buildData.gSelectedLanguage.Trim());
            driver.Options.GeneratorKind =gk;
            driver.Options.OutputDir = buildData.gOutCsharpLibPath;

            ///一个模块 StandardLib 就是一个类, 指定一个命名空间. 对照着添加接口.h
            var data = buildData;
            var module = driver.Options.AddModule(data.gLibName); //这个只会生成 调用类的文件名以及类名

            ///添加命名空间
            module.OutputNamespace = data.gNamespaceName; //命名空间,没有可以不添加.

            ///添加头文件.
            //module.IncludeDirs.Add(_includePath);   //包含的includ目录
            //module.Headers.Add("StandardLib.h");
            //module.Headers.Add("TestB1.h");
            var includeLists = data.gIncludeLists;

            if (includeLists.Count>0)
            {
                var first = includeLists[0];
                var dirResult = getDirAndFileName(first);
                module.IncludeDirs.Add(dirResult.Item1);//设置include头文件所在目录.
                module.Headers.Add(dirResult.Item2);
                //添加提取到的文件.
                for (int i = 1; i < includeLists.Count; i++)
                {
                    var headfile= getFileName(includeLists[i]);
                    module.Headers.Add(headfile);
                }
            }



            ///添加静态库.
            //module.LibraryDirs.Add(_libPath);
            //module.Libraries.Add("StandardLib.lib");
            var libLists = data.gLibLists;
            if(libLists.Count>0)
            {
                var firstlib = libLists[0];
                var libDirInfo = getDirAndFileName(firstlib);
                module.LibraryDirs.Add(libDirInfo.Item1);
                module.Libraries.Add(libDirInfo.Item2);
                for (int i = 1; i < libLists.Count; i++)
                {
                    module.Libraries.Add(getFileName(libLists[i]));
                }
            }

        }

        public void SetupPasses(Driver driver)
        {
            ///设置CPP11 版本.
           // driver.Generator.Context.ParserOptions.LanguageVersion = CppSharp.Parser.LanguageVersion.CPP11;
            driver.Generator.Context.ParserOptions.LanguageVersion = Enum.Parse<LanguageVersion>(buildData.gSelectedVersion);
        }

        ///将传入的路径,提取文件名和输入文件夹.
        private Tuple<string,string> getDirAndFileName(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            string dir = file.Directory.FullName;
            string fileName = file.Name;
            return new Tuple<string, string>(dir,fileName);
        }
        private string getFileName(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            return file.Name;
        }
    }


}
