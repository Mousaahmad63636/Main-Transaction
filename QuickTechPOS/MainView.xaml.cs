using QuickTechPOS.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the main view
        /// </summary>
        /// <param name="viewModel">The view model for this view</param>
        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// Sets the main content of the view
        /// </summary>
        /// <param name="content">The content to display</param>
        public void SetContent(UIElement content)
        {
            MainContent.Content = content;
        }
    }
}