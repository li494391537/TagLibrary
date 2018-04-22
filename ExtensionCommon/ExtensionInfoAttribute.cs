using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lirui.TagLibrary.ExtensionCommon {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExtensionInfoAttribute : Attribute {

        public ExtensionInfoAttribute(string[] fileExtension) {
            FileExtension = fileExtension;
        }

        public string[] FileExtension { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

    }
}
