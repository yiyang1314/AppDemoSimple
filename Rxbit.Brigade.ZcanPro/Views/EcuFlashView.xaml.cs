using System.Windows;
using System.Windows.Controls;

namespace Rxbit.Brigade.ZcanPro.Views
{
    public partial class EcuFlashView : UserControl
    {
        public EcuFlashEditorView Editor { get; }
        public EcuFlashExecutorView Executor { get; }

        public EcuFlashView()
        {
            InitializeComponent();

            TabEditor.Checked += (s, e) => ShowEditor();
            TabExecutor.Checked += (s, e) => ShowExecutor();

            Editor = new EcuFlashEditorView();
            Executor = new EcuFlashExecutorView();
            EditorHolder.Content = Editor;
            ExecutorHolder.Content = Executor;

            Loaded += (s, e) => ShowEditor();
        }

        private void ShowEditor()
        {
            EditorHolder.Visibility = Visibility.Visible;
            ExecutorHolder.Visibility = Visibility.Collapsed;
        }

        private void ShowExecutor()
        {
            EditorHolder.Visibility = Visibility.Collapsed;
            ExecutorHolder.Visibility = Visibility.Visible;
        }
    }
}
