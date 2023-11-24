using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;

namespace Expedicao
{
    public class GridSelectionControllerExt : GridSelectionController
    {
        public GridSelectionControllerExt(SfDataGrid dataGrid) : base(dataGrid)
        {
        }

        protected override void ProcessKeyDown(KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                KeyEventArgs arguments = new KeyEventArgs(args.KeyboardDevice, args.InputSource, args.Timestamp, Key.Tab) { RoutedEvent = args.RoutedEvent };
                base.ProcessKeyDown(arguments);
                //assigning the state of Tab key Event handling to Enter key
                args.Handled = arguments.Handled;
                return;
            }
            base.ProcessKeyDown(args);
        }
    }
}
