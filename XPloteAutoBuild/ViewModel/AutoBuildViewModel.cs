using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPlote.Framework.WPF;
using XPlote.Expand;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CppSharp.Generators;
using CppSharp.Parser;
using System.IO;
using System.Windows.Shapes;

namespace XPloteAutoBuild
{
    /// <summary>
    /// 主要的操作,计算逻辑在这里.
    /// </summary>
    public class AutoBuildViewModel : XBindingBase
    {
#if true
        public AutoBuildViewModel(AutoBuildModel autoModel)
        {
            gAutoModel = autoModel;
            initSouce();
        }
        private AutoBuildModel gAutoModel { get; set; }

#else

    public AutoBuildViewModel()
        {
            initSouce();
        }
        private AutoBuildModel gAutoModel => IocHelper.gDefaultIoc.gModel;


#endif

        #region 操作命令封装.
        public ICommand ISelectedHeadFiles { get; set; }
        public ICommand IClearnHeadFiles { get; set; }
        public ICommand IClearnSelectedHeadFile { get; set; }

        public ICommand ISelectedLibFiles { get; set; }
        public ICommand IClearnLibFiles { get; set; }
        public ICommand IClearnSelectedLibFile { get; set; }

        public ICommand IBuildNet6Proj { get; set; }

        public ICommand IBuildNet6ProjExe { get; set; }
        public ICommand IBuildCsharpLib { get; set; }

        /// <summary>
        /// 打开输出目录.
        /// </summary>
        public ICommand IOpenOutDir { get; set; }
        #endregion

        private void initSouce()
        {
            initModelSource();

            IOpenOutDir = new RelayCommand(() => {

                System.Diagnostics.Process.Start("explorer.exe", XPloteConfig.baseDir);
            });

            ISelectedHeadFiles = new RelayCommand(() =>
            {

                GetError(() =>
                {

                    var files = XPlote.Expand.FileHelper.OpenFiles("*.h|*.h");
                    foreach (var item in files)
                    {
                        gAutoModel.gIncludeLists.Add(item);
                    }

                });

            });
            IClearnSelectedHeadFile = new RelayCommand(() =>
            {

                GetError(() =>
                {
                    var selectedFile = gAutoModel.gSelectedIncludeFile;
                    gAutoModel.gIncludeLists.Remove(selectedFile);
                });

            });
            IClearnHeadFiles = new RelayCommand(() =>
            {
                gAutoModel?.gIncludeLists.Clear();
            });

            ISelectedLibFiles     = new RelayCommand(() =>
            {

                GetError(() =>
                {

                    var files = XPlote.Expand.FileHelper.OpenFiles("*.lib|*.lib");
                    foreach (var item in files)
                    {
                        gAutoModel.gLibLists.Add(item);
                    }
                    //这里把库名称设置为第一个添加的lib.
                    if (files.Count>0)
                    {
                        var libname = new FileInfo(files[0]).Name;
                        libname = libname.Substring(0, libname.Length-".lib".Length);
                        gAutoModel.gLibName =libname;
                    }
                    PrintLog($"库文件导出完成..{gAutoModel.gLibName}");
                });

            });
            IClearnLibFiles   = new RelayCommand(() =>
            {

                GetError(() =>
                {
                    gAutoModel.gLibLists.Clear();

                });

            });
            IClearnSelectedLibFile= new RelayCommand(() =>
            {

                GetError(() =>
                {
                    var file = gAutoModel.gSelectedLibFile;
                    gAutoModel.gLibLists.Remove(file);

                });

            });

            IBuildCsharpLib = new RelayCommand(() =>
            {

                GetError(() =>
                {
                    CppSharpBuild.Build();
                    PrintLog($"{gAutoModel.gSelectedLanguage} 库生成完成...");

                });
            });

            IBuildNet6Proj  = new RelayCommand(() =>
            {

                GetError(() =>
                {
                    AutoBuildNet6ProjHelper autoBuildNet6 = new AutoBuildNet6ProjHelper(gAutoModel);
                    autoBuildNet6.BuildNet6ProjLib();
                    PrintLog($"项目配置文件生成完成(lib)");

                });
            });
            IBuildNet6ProjExe  = new RelayCommand(() =>
            {

                GetError(() =>
                {
                    AutoBuildNet6ProjHelper autoBuildNet6 = new AutoBuildNet6ProjHelper(gAutoModel);
                    autoBuildNet6.BuildNet6ProjExe();
                    PrintLog($"项目配置文件生成完成(exe)");
                });
            });


        }

        private void initModelSource()
        {
            //语言列表
#if false
            gAutoModel?.gLanguageLists.Clear();
            var languagelists = Enum.GetValues(typeof(GeneratorKind));
            foreach (var languagelist in languagelists)
            {
                gAutoModel?.gLanguageLists.Add(languagelist.ToString());
            }

            //cpp版本>
            gAutoModel?.gCppVersionLists.Clear();
            var cpplists = Enum.GetValues(typeof(LanguageVersion));
            foreach (var cppitem in cpplists)
            {
                gAutoModel?.gCppVersionLists.Add(cppitem.ToString());
            } 

#else
            // --- 语言列表 (GeneratorKind) ---
            gAutoModel?.gLanguageLists.Clear();

            // 直接遍历 GeneratorKind 类中的 Registered 静态集合
            foreach (var generatorKind in GeneratorKind.Registered)
            {
                // 您可以选择添加 ID (如 "CSharp") 或 Name (如 "C#") 到列表中
                // 这里以添加更友好的 Name 为例
                gAutoModel?.gLanguageLists.Add(generatorKind.Name);

                // 如果您的UI需要对应的值是ID，可以这样做：
                // gAutoModel?.gLanguageLists.Add(generatorKind.ID);
            }

            // --- C++版本列表 (LanguageVersion) ---
            // 假设 LanguageVersion 仍然是一个枚举，这段代码保持不变
            gAutoModel?.gCppVersionLists.Clear();
            var cpplists = Enum.GetValues(typeof(LanguageVersion));
            foreach (var cppitem in cpplists)
            {
                gAutoModel?.gCppVersionLists.Add(cppitem.ToString());
            }

#endif
        }
        #region 辅助类.
        private void GetError(Action action)
        {
            XPlote.Expand.ActionHelper.GetErrorInfo(action);
        }


        private void PrintLog(string str)
        {
            XPlote.Expand.WindowLog.Default.Log(str);
        }
        #endregion

    }
}
