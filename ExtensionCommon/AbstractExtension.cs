using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lirui.TagLibrary.ExtensionCommon {
    public abstract class AbstractExtension : IExtensionCommon {

        public AbstractExtension(string filename) {
            Filename = filename;
        }
        protected string Filename { get; private set; }
        
        public async Task<KeyValuePair<string, string>[]> BeginGetTags() {
            Func<KeyValuePair<string, string>[]> func = () => {
                return GetTags();
            };
            var task = new Task<KeyValuePair<string, string>[]>(func);
            task.Start();
            return await task;
        }

        public abstract KeyValuePair<string, string>[] GetTags();
    }
}
