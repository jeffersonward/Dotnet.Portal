using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dotnet.Portal
{
    /// <summary>
    /// Interaction logic for DraggableTabControl.xaml
    /// </summary>
    public partial class DraggableTabControl : UserControl
    {
        public DraggableTabControl()
        {
            InitializeComponent();
        }

        private TabItem _sticky;

        public TabItem Sticky
        {
            get => _sticky;
            set
            {
                if (_sticky != null)
                {
                    Items.Remove(_sticky);
                }

                _sticky = value;

                Items.Insert(0, _sticky);
            }
        }

        public ItemCollection Items => TabControl.Items;

        public int SelectedIndex
        {
            get => TabControl.SelectedIndex;
            set => TabControl.SelectedIndex = value;
        }

        public EventHandler TabItemDropped;

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.PrimaryDevice.LeftButton != MouseButtonState.Pressed) return;
            var tabItem = GetTabItem(e.Source);
            if (tabItem == null) return;
            DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
        }

        private TabItem GetTabItem(object source)
        {
            var tabItem = source as TabItem;
            if (tabItem == null && source is TabHeader tabHeader)
            {
                tabItem = tabHeader.Parent as TabItem;
            }

            return tabItem == null || tabItem.Parent != TabControl || tabItem == _sticky ? null : tabItem;
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            var tabItemTarget = e.Source is TabItem item ? item : (e.Source as TabHeader)?.Parent as TabItem;

            var tabItemSource = (TabItem)(e.Data.GetData(typeof(ProjectTabItem)) ?? e.Data.GetData(typeof(SolutionTabItem)));

            if (tabItemTarget == null || tabItemSource == null || tabItemTarget.Equals(tabItemSource)) return;

            var tabControlTarget = (TabControl)tabItemTarget.Parent;
            var tabControlSource = (TabControl)tabItemSource.Parent;

            if (tabControlSource != tabControlTarget) return;

            //var sourceIndex = tabControlTarget.Items.IndexOf(tabItemSource);
            var targetIndex = tabControlTarget.Items.IndexOf(tabItemTarget);

            tabControlTarget.Items.Remove(tabItemSource);
            tabControlTarget.Items.Insert(targetIndex, tabItemSource);

            tabControlTarget.SelectedIndex = targetIndex;

            TabItemDropped?.Invoke(this, EventArgs.Empty);
        }
    }
}