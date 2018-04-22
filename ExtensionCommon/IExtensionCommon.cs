using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lirui.TagLibrary.ExtensionCommon
{
    public interface IExtensionCommon
    {
        /// <summary>
        /// 异步通过文件元数据获取Tag数组
        /// </summary>
        /// <param name="filename">文件名（绝对路径）</param>
        /// <returns></returns>
       Task<KeyValuePair<string,string>[]> BeginGetTags();

        /// <summary>
        /// 同步通过文件元数据获取Tag数组
        /// </summary>
        /// <param name="filename">文件名（绝对路径）</param>
        /// <returns></returns>
        KeyValuePair<string, string>[] GetTags();
    }
}
