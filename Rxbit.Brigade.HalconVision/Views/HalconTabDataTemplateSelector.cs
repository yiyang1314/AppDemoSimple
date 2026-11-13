using System.Windows;
using System.Windows.Controls;
using Rxbit.Brigade.HalconVision.ViewModels;

namespace Rxbit.Brigade.HalconVision.Views
{
    public class HalconTabDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GraphicsTemplate { get; set; }
        public DataTemplate ProgramTemplate { get; set; }
        public DataTemplate VariablesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is GraphicsWindowViewModel) return GraphicsTemplate;
            if (item is ProgramWindowViewModel) return ProgramTemplate;
            if (item is VariablesWindowViewModel) return VariablesTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}