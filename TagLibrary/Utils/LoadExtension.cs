using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Lirui.TagLibrary.ExtensionCommon;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Text;

namespace Lirui.TagLibrary.Utils {
    class ExtensionUtil {
        public static Dictionary<Assembly, List<string>> Assemblies { get; private set; }
        public static List<KeyValuePair<string, Type>> ExtensionType { get; private set; }

        static ExtensionUtil() {
            ExtensionType = new List<KeyValuePair<string, Type>>();
            LoadExtension();
        }

        private static void LoadExtension() {

            Func<byte[], bool> publicKeyTokenTest = (buffer) => {
                var str = new StringBuilder();
                foreach (var b in buffer) {
                    str.Append($"{b:x2}");
                }
                if (str.ToString().Equals("0ff26afdf715c0d1")) return true;
                return false;
            };

            var dir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\Extensions");
            Assemblies =
                dir.EnumerateFiles()
                .Where(item => item.Extension.ToUpper() == ".DLL")
                .Select(item => item.FullName)
                .Select(item => { try { return Assembly.LoadFile(item); } catch { return null; } })
                .Where(item => item != null)
                .Where(item => publicKeyTokenTest(item.GetName().GetPublicKeyToken()))
                //.Where(item => Regex.IsMatch(item.FullName, "PublicKeyToken=0ff26afdf715c0d1", RegexOptions.IgnoreCase))
                .ToDictionary(item => item, item => new List<string>());

            Assemblies
                .SelectMany(item => item.Key.ExportedTypes, (key, value) => new { assembly = key.Key, type = value })
                .ToList()
                .ForEach(item => {
                    //如果不是继承自AbstractExtension，则不进行操作
                    if (!item.type.IsSubclassOf(typeof(AbstractExtension))) return;
                    var customAttributes =
                        item.type.CustomAttributes
                        ?.Where(x => x.AttributeType == typeof(ExtensionInfoAttribute))
                        ?.SingleOrDefault();
                    ///如果没有ExtensionInfoAttribute属性，则不进行操作
                    if (customAttributes == null) return;
                    foreach (var i in customAttributes.ConstructorArguments) {
                        if (i.Value is ReadOnlyCollection<CustomAttributeTypedArgument>) {
                            //MessageBox.Show($"Array of '{i.ArgumentType}':");
                            foreach (var cataElement in
                                i.Value as ReadOnlyCollection<CustomAttributeTypedArgument>) {
                                //MessageBox.Show($"Type: '{cataElement.ArgumentType}'Value: '{cataElement.Value}'");
                                ExtensionType.Add(new KeyValuePair<string, Type>(cataElement.Value as string, item.type));
                                Assemblies[item.assembly].Add(cataElement.Value as string);
                            }
                        } else {
                            //MessageBox.Show($"Type: '{i.ArgumentType}'  Value: '{i.Value}'");
                        }
                    }
                });
            //foreach (var assembly in Assemblies) {
            //    foreach (var module in assembly.GetModules()) {
            //        foreach (var type in module.GetTypes()) {

            //        }
            //    }
            //}
        }
    }
}
