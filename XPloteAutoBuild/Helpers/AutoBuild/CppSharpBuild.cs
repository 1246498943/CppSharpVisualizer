using CppSharp;
namespace XPloteAutoBuild
{
    public class CppSharpBuild
    {
        public static void Build()
        {
            var autoModle = IocHelper.gDefaultIoc.gModel;
            
            ConsoleDriver.Run(new CppSharpLibHelper(autoModle));

        }

    }


}
