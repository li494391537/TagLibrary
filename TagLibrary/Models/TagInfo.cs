using SqlSugar;
using System.ComponentModel;

namespace Lirui.TagLibrary.Models {
    public class TagInfo : INotifyPropertyChanged {

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int? Id { get; set; }

        private string name;
        [SugarColumn(IsNullable = false)]
        public string Name {
            get => name;
            set {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private string group;
        [SugarColumn(IsNullable = false)]
        public string Group {
            get => group;
            set {
                group = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Group"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
