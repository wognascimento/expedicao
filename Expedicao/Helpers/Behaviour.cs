using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expedicao
{
    public class Behaviour : Behavior<UserControl>
    {
        SfDataGrid? dataGrid;
        SearchControl? searchControl;
        protected override void OnAttached()
        {
            var window = AssociatedObject;
            dataGrid = window.FindName("dataGrid") as SfDataGrid;
            dataGrid.KeyDown += OnDataGridKeyDown;
            searchControl = window.FindName("searchControl") as SearchControl;
        }
        /// <summary>
        /// Invokes thid Event to show the AdonerDecorator in the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataGridKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.None && e.Key == Key.F)
                searchControl?.UpdateSearchControlVisiblity(true);
            else
                searchControl?.UpdateSearchControlVisiblity(false);
        }
        protected override void OnDetaching()
        {
            dataGrid.KeyDown -= OnDataGridKeyDown;
        }
    }
}
