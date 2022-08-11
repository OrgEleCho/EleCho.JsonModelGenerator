using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EleCho.JsonModelGenerator.ViewModel
{
    internal class MainWindowModel : INotifyPropertyChanged
    {
        public bool ShowOptions { get; set; }
        public bool ShowLog { get; set; }

        public bool UsePascal { get; set; } = true;
        public bool UseCamel { get; set; }
        public bool UseSnake { get; set; }
        public bool UseOriginName { get; set; }

        public bool UseNestedClasses { get; set; } = true;
        public bool UseTopmostClasses { get; set; }
        public bool UseTopmostClassesAndBetterNames { get; set; }

        public bool UseArrayForCollection { get; set; } = true;
        public bool UseListForCollection { get; set; }
        public bool UseIEnumerableForCollection { get; set; }

        public bool UseAutomaticallyTranslateForForCollectionItemClass { get; set; } = true;
        public bool UsePropertyNameForCollectionItemClass { get; set; }
        public bool UsePropertyNameAndItemForCollectionItemClass { get; set; }

        public bool UseNoAttribute { get; set; } = true;
        public bool UseSystemTextJsonAttribute { get; set; }
        public bool UseNewtonsoftJsonAttribute { get; set; }



        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
