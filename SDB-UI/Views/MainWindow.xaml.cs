using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Sdb.Ui.Messages;
using Sdb.Ui.ViewModel;

namespace Sdb.Ui.Views
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
         Messenger.Default.Register<ShowConfig>(this, msg => new ConfigView(this).ShowDialog());

         MainViewModel.QueryErrorDialog = new QueryErrorDialog(this);
         MainViewModel.DeleteQueryConfirmationDialog = new DeleteQueryConfirmationDialog(this);
         MainViewModel.DeleteDomainsConfirmationDialog = new DeleteDomainsConfirmationDialog(this);
         MainViewModel.SaveBeforeCloseDialog = new SaveBeforeCloseDialog(this);
         MainViewModel.EnterNameDialog = new EnterNameDialog(this);
      }
   }
}
