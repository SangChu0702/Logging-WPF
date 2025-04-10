using Prism.Ioc;
using UARTLogging.Views;
using System.Windows;
using UARTLogging.ViewModels;
using Prism.Regions;

namespace UARTLogging
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MainViewModel>();
        }   
        protected override void OnInitialized()
        {
            base.OnInitialized();
            var regionManager = Prism.Ioc.ContainerLocator.Container.Resolve<Prism.Regions.IRegionManager>();
            regionManager.RegisterViewWithRegion<SerialPortView>("SPView");
            regionManager.RegisterViewWithRegion<LogView>("LogView");
            regionManager.RegisterViewWithRegion<LogMonitorView>("LogMonitorView");
        }
    }
}
