using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace EnumEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.LoadFrom(@"C:\Users\Arman\Documents\visual studio 2013\Projects\EnumEditor\SharedKernel.Enums\bin\Debug\SharedKernel.Enums.dll");

            var enums = GetEnumsNames(assembly);

            var props = GetEnumProps(assembly, enums.First().Key);

            props.Add("UserMode", 10);

            var enumstr = CreateEnum(props, enums.First().Key, "int");

            assembly = null;

            var source = enumstr;
            var references = new[] {"System.dll"};
            var result = CompileSourceCode(new[] { source }, @"C:\Users\Arman\Documents\visual studio 2013\Projects\EnumEditor\SharedKernel.Enums\bin\Debug\SharedKernel.Enums2.dll", references);

            if (result.Errors.Count == 0)
            {
                Console.WriteLine("No Errors");
            }
            else
            {
                foreach (CompilerError error in result.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
            }

            assembly = Assembly.LoadFrom(@"C:\Users\Arman\Documents\visual studio 2013\Projects\EnumEditor\SharedKernel.Enums\bin\Debug\SharedKernel.Enums2.dll");

            enums = GetEnumsNames(assembly);

            props = GetEnumProps(assembly, enums.First().Key);

            Console.ReadKey();
        }

        public static Dictionary<string, string> GetEnumsNames(Assembly assembly)
        {
            var namesDictionary = new Dictionary<string, string>();

            foreach (var type in assembly.GetExportedTypes())
                if (type.IsEnum)
                    namesDictionary.Add(type.Name, type.GetEnumUnderlyingType().Name);

            return namesDictionary;
        }

        public static Dictionary<string, int> GetEnumProps(Assembly assembly, string enumName)
        {
            var enumProps = new Dictionary<string, int>();

            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsEnum && type.Name == enumName)
                {
                    var y = type.GetEnumValues();

                    for (var i = 0; i < y.Length; i++)
                    {
                        var intValue = (int)y.GetValue(i);
                        var name = type.GetEnumName(intValue);
                        enumProps.Add(name, intValue);
                    }

                    break;
                }
            }

            return enumProps;
        }

        public static string CreateEnum(Dictionary<string, int> enumProps, string enumName, string baseType)
        {
            var enumStr = string.Format("public enum {0} : {1}", enumName, baseType) + Environment.NewLine + "{" + Environment.NewLine;

            foreach (var prop in enumProps)
            {
                enumStr += "\t" + string.Format("{0} = {1}", prop.Key, prop.Value) + "," + Environment.NewLine;
            }

            enumStr += "}";

            return enumStr;
        }

        public static CompilerResults CompileSourceCode(string[] sources, string output, params string[] references)
        {
            var parameters = new CompilerParameters(references, output)
            {
                GenerateExecutable = false,
                CompilerOptions = "/optimize",
                WarningLevel = 3,
                TreatWarningsAsErrors = false,
                GenerateInMemory = false,
                IncludeDebugInformation = true
            };

            using (var provider = new CSharpCodeProvider())
            {
                return provider.CompileAssemblyFromSource(parameters, sources);
            }
        }
    }
}
