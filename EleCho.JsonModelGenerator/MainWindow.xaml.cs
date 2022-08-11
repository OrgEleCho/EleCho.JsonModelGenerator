using EleCho.Json;
using EleCho.JsonModelGenerator.Core;
using EleCho.JsonModelGenerator.Utils;
using EleCho.JsonModelGenerator.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EleCho.JsonModelGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        JsonModelWriterOptions GetOptions()
        {
            MainWindowModel vm = (MainWindowModel)DataContext;
            return new JsonModelWriterOptions()
            {
                NamingStyle = (NamingStyle)EnumUtils.NumValue(
                    vm.UsePascal,
                    vm.UseCamel,
                    vm.UseSnake,
                    vm.UseOriginName),
                ClassNestStyle = (ClassNestStyle)EnumUtils.NumValue(
                    vm.UseNestedClasses,
                    vm.UseTopmostClasses,
                    vm.UseTopmostClassesAndBetterNames),
                CollectionTypeStyle = (CollectionTypeStyle)EnumUtils.NumValue(
                    vm.UseArrayForCollection,
                    vm.UseListForCollection,
                    vm.UseIEnumerableForCollection),
                CollectionItemNamingStyle = (CollectionItemNamingStyle)EnumUtils.NumValue(
                    vm.UseAutomaticallyTranslateForForCollectionItemClass,
                    vm.UsePropertyNameForCollectionItemClass,
                    vm.UsePropertyNameAndItemForCollectionItemClass),
                AttributeStyle = (AttributeStyle)EnumUtils.NumValue(
                    vm.UseNoAttribute,
                    vm.UseSystemTextJsonAttribute,
                    vm.UseNewtonsoftJsonAttribute),
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StringWriter sw = new StringWriter();
            JsonModelWriter mw = new JsonModelWriter(GetOptions(), sw);

            IJsonData? toWrite = null;

            try
            {
                toWrite = JsonReader.Read(input.Text);
            }
            catch
            {
                output.Text = "格式不正确";
            }

            if (toWrite is JsonObject jobj)
            {
                mw.WriteModel(jobj);
                output.Text = sw.ToString();
            }

        }
    }
}
