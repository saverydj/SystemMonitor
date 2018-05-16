using System.Windows.Input;
using STARS.Applications.Interfaces;
using System.ComponentModel.Composition;
using STARS.Applications.VETS.Interfaces;
using Caliburn.PresentationFramework.Views;
using STARS.Applications.Interfaces.Constants;
using STARS.Applications.Interfaces.ViewModels;
using STARS.Applications.VETS.Interfaces.Constants;
using STARS.Applications.VETS.Interfaces.ViewModels.Attributes;
using STARS.Applications.VETS.UI.Views.Commands.CommandBaseViews;
using System.Threading;
using STARS.Applications.UI.Common;
using STARS.Applications.VETS.Interfaces.ResourceData;
using Stars.DataDistribution;
using STARS.Applications.VETS.Interfaces.Devices;
using STARS.Applications.Interfaces.Alarms;
using System.Reflection;
using System.Linq;
using Stars.ApplicationManager;
using STARS.Applications.VETS.Interfaces.Logging;
using STARS.Applications.Interfaces.Dialogs;

namespace STARS.Applications.VETS.Plugins.SystemMonitor.ViewModels.Home
{
    [View(typeof(Explorer), Context = "Explorer")]
    [Command(CommandCategories.Utilities, "SystemMonitor", Priority = Priorities.Last), PartCreationPolicy(CreationPolicy.Shared)]
    class HomeViewCommandViewModel //: ICommandViewModel
    {

        [ImportingConstructor]
        public HomeViewCommandViewModel
        (
            IImageManager imageManager, 
            OnlineResources onlineResources,        
            ISystemLogManager logger,
            IDialogService dialogService,
            IDeviceManager deviceManager
        )
        {

            if (Config.ShowForm)
            {
                Form1 form1 = new Form1();
                form1.Show();
            }

            SystemLogService.Init(logger, dialogService);
            Main.Init(onlineResources, deviceManager);

            Thread timerTickThread = new Thread(_ => Main.MonitorTimerTick());
            Thread timerResetThread = new Thread(_ => Main.MonitorTimerReset());

            timerTickThread.IsBackground = true;
            timerResetThread.IsBackground = true;

            timerTickThread.Start();
            timerResetThread.Start();

            //DisplayName = Properties.Resources.DisplayName;
            //DisplayInfo = new ExplorerDisplayInfo
            //{
            //    Description = Properties.Resources.DisplayName,
            //    Image16 = imageManager.GetImage16Path("OK"),
            //    ExplorerImage16 = imageManager.GetImage16Path("OK")
            //};

            //Command = new RelayCommand(_ => HelloWorld());
        }

        //public void HelloWorld()
        //{
        //}

        //public ICommand Command { get; }
        //public string DisplayName { get; }
        //public DisplayInfo DisplayInfo { get; }
    }
}
