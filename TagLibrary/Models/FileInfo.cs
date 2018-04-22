using SqlSugar;

namespace Lirui.TagLibrary.Models {
    public class FileInfo {

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int? Id { get; set; }

        [SugarColumn(IsNullable = false)]
        public string UUID { get; set; }

        [SugarColumn]
        public string OriginalPath { get; set; }

        [SugarColumn(IsNullable = false)]
        public string Name { get; set; }

        [SugarColumn(IsNullable = false)]
        public string Format { get; set; }

        [SugarColumn(IsNullable = false)]
        public long Size { get; set; }
        
    }
}
