using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MyIDE_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml;

namespace MyIDE_WPF.Views
{
    /// <summary>
    /// Interaction logic for ProgramCodeViewModel.xaml
    /// </summary>
    public partial class ProgramCodeView : UserControl
    {
        public ProgramCodeView()
        {
            InitializeComponent();
        }

        private Stream GetResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string fullName = assembly.GetName().Name + "." + name;
            return assembly.GetManifestResourceStream(fullName);
        }

        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                ProgramCodeViewModel model = MyEditor.DataContext as ProgramCodeViewModel;

                using (Stream stream = GetResourceStream("python.xshd"))
                {
                    MyEditor.SyntaxHighlighting =
                        HighlightingLoader.Load(new XmlTextReader(stream),
                        HighlightingManager.Instance);
                }

                MyEditor.Text = model.Code;

                model.SyncRequested += Model_SyncRequested;

                model.PropertyChanged += Model_PropertyChanged;
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HighlightedLineNumber")
            {
                var model = sender as ProgramCodeViewModel;
                if (model != null)
                {
                    MyEditor.ScrollToLine(model.HighlightedLineNumber);
                    // TO DO: Highlight the line (somehow!)
                }
            }
        }

        private void Model_SyncRequested(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProgramCodeViewModel model = MyEditor.DataContext as ProgramCodeViewModel;

            if (e.PropertyName == nameof(model.Code))
            {
                model.Code = MyEditor.Text;
            }
        }
    }
}
