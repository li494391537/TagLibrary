using Lirui.TagLibrary.ExtensionCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lirui.TagLibray.Extension {
    [ExtensionInfo(new string[] { "png" }, Author = "lirui", Version = "1.0.0.0")]
    public class Mp4 : AbstractExtension {
        public Mp4(string filename) : base(filename) {
        }

        public override KeyValuePair<string, string>[] GetTags() {
            throw new NotImplementedException();
        }
    }
}
