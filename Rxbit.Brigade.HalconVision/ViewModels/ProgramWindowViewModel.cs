using System.Collections.ObjectModel;
using System.Windows.Input;
using Rxbit.Brigade.HalconVision.Mvvm;

namespace Rxbit.Brigade.HalconVision.ViewModels
{
    public class ProgramWindowViewModel : ObservableObject
    {
        private string _title;
        private string _codeText;
        private int _cursorLine;
        private int _cursorColumn;
        private bool _isModified;
        private string _filePath = "main()";
        private ObservableCollection<CodeLineItem> _codeLines;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string CodeText { get => _codeText; set { SetProperty(ref _codeText, value); UpdateCodeLines(); } }
        public int CursorLine { get => _cursorLine; set => SetProperty(ref _cursorLine, value); }
        public int CursorColumn { get => _cursorColumn; set => SetProperty(ref _cursorColumn, value); }
        public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public ObservableCollection<CodeLineItem> CodeLines { get => _codeLines; set => SetProperty(ref _codeLines, value); }

        public ObservableCollection<string> Keywords { get; } = new ObservableCollection<string>
        {
            "dev_read_image", "dev_close_window", "dev_open_window", "dev_set_part",
            "threshold", "edges_image", "regiongrowing", "connection", "select_shape",
            "count_objects", "dev_clear_window", "dev_display", "dev_set_color",
            "dev_set_draw", "dev_set_line_width", "dev_close_window", "read_image",
            "Threshold", "EdgesImage", "RegionGrowing", "Connection", "SelectShape"
        };

        public ICommand RunCommand { get; }
        public ICommand DebugCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public ProgramWindowViewModel()
        {
            _title = "程序窗口 - main()";
            _codeText = GetInitialCode();
            _codeLines = new ObservableCollection<CodeLineItem>();
            UpdateCodeLines();

            RunCommand = new RelayCommand(() => { });
            DebugCommand = new RelayCommand(() => { });
            SaveCommand = new RelayCommand(() => { IsModified = false; });
            UndoCommand = new RelayCommand(() => { });
            RedoCommand = new RelayCommand(() => { });
        }

        private void UpdateCodeLines()
        {
            _codeLines = new ObservableCollection<CodeLineItem>();
            var lines = CodeText?.Split('\n') ?? new string[0];
            for (int i = 0; i < lines.Length; i++)
            {
                _codeLines.Add(new CodeLineItem { LineNumber = i + 1, Text = lines[i] });
            }
            OnPropertyChanged(nameof(CodeLines));
        }

        private string GetInitialCode()
        {
            return @"// Display an XLD object.
read_image (Image, 'reut')
dev_close_window ()
dev_open_window (0, 0, 512, 512, 'black', WindowID)
set_display_font (WindowID, 14, 'mono', 'true', 'false')
edges_image (Image, ImaAmp, ImaDir, 'lanser2', 0.7, 'nms', 20, 40)
threshold (ImaAmp, Region, 1, 255)
connection (Region, Countours1)
select_shape (Countours1, Contours, 1, 'area', 'and', 150, 99999)
dev_set_part (0, 0, 512, 512)
dev_clear_window ()
dev_display (Contours, WindowID)
// Measure the runtime to display xlds
count_seconds (Seconds1)
disp_message (36420, 36420, 36420, 'window', 'false')
count_seconds (Seconds2)
ImaAmp2 := Seconds2 - Seconds1
dev_close_window ()
count_seconds (Seconds1)
disp_xld (Contours, WindowID)
count_seconds (Seconds2)
ImaAmp := Seconds2 - Seconds1";
        }
    }

    public class CodeLineItem : ObservableObject
    {
        private int _lineNumber;
        private string _text;
        private bool _isHighlighted;

        public int LineNumber { get => _lineNumber; set => SetProperty(ref _lineNumber, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public bool IsHighlighted { get => _isHighlighted; set => SetProperty(ref _isHighlighted, value); }
    }
}