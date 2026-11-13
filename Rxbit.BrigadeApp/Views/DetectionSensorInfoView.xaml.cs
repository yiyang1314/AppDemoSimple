using Rxbit.BrigadeApp.Controls;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Rxbit.BrigadeApp.Views
{
    public partial class DetectionSensorInfoView : WindowBase
    {
        public DetectionSensorInfoView()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // Handle cell selection
        }
    }



    public class ColumnSelectBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += DataGrid_PreviewMouseDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= DataGrid_PreviewMouseDown;
            base.OnDetaching();
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;
            // 获取点击的单元格
            var cell = e.OriginalSource as DataGridCell;
            if (cell == null) return;

            int targetColumnIndex = cell.Column.DisplayIndex;
            dataGrid.SelectedCells.Clear();
            // 遍历所有行，选中同列全部单元格
            foreach (DataGridRow row in dataGrid.Items)
            {
                if (row.IsNewItem) continue;
                var cellInfo = new DataGridCellInfo(row, dataGrid.Columns[targetColumnIndex]);
                dataGrid.SelectedCells.Add(cellInfo);
            }
            // 高亮列顶边框，匹配截图青蓝高亮
            //dataGrid.CurrentCell = cellInfo;
        }
    }


}
