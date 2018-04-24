using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lirui.TagLibray.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lirui.TagLibray.Extension.Tests {
    [TestClass()]
    public class Mp3Tests {
        [TestMethod()]
        public void GetTagsTest() {
            try {
                var test = new Mp3(@"D:\CloudMusic\初音ミク\Re：Start\wowaka,初音ミク - アンノウン・マザーグース.mp3");
                var result = test.GetTags();
                var r = test.BeginGetTags().Result;
            } catch { Assert.Fail(); }

        }
    }
}