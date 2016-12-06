using System.Windows;

namespace Sdb.Ui.Views
{
   public interface IDialogService
   {
      bool Show(string message="");
   }

   public class SaveBeforeCloseDialog : IDialogService
   {
      private readonly Window _owner;
      public SaveBeforeCloseDialog(Window owner)
      {
         _owner = owner;
      }

      public bool Show(string name)
      {
         var message = string.Format("Save {0} before closing?", name);
         return MessageBox.Show(_owner, message, string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
      }
   }

   public class QueryErrorDialog : IDialogService
   {
      private readonly Window _owner;
      public QueryErrorDialog(Window owner)
      {
         _owner = owner;
      }

      public bool Show(string message)
      {
         MessageBox.Show(_owner, message, "Query Error", MessageBoxButton.OK, MessageBoxImage.Error);
         return true;
      }
   }

   public class DeleteQueryConfirmationDialog : IDialogService
   {
      private readonly Window _owner;
      public DeleteQueryConfirmationDialog(Window owner)
      {
         _owner = owner;
      }

      public bool Show(string queryName)
      {
         var msg = string.Format("Are you sure you want to delete the query: {0}?", queryName);
         return MessageBox.Show(_owner, msg, string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes;
      }
   }

   public class DeleteDomainsConfirmationDialog : IDialogService
   {
      private readonly Window _owner;
      public DeleteDomainsConfirmationDialog(Window owner)
      {
         _owner = owner;
      }

      public bool Show(string domains)
      {
         var msg = "Are you sure you want to delete the following domain(s)?\n" + domains;
         return MessageBox.Show(_owner, msg, string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes;
      }
   }

   public interface IInputDialogService
   {
      string Show();
   }

   public class EnterNameDialog : IInputDialogService
   {
      private readonly Window _owner;
      public EnterNameDialog(Window owner)
      {
         _owner = owner;
      }

      public string Show()
      {
         var dialog = new BasicInputBox(_owner, "Name:");
         dialog.ShowDialog();
         var name = dialog.TextBox.Text;

         while (string.IsNullOrWhiteSpace(name))
         {
            MessageBox.Show(_owner, "You must enter a name.");
            dialog = new BasicInputBox(_owner, "Name:");
            dialog.ShowDialog();
            name = dialog.TextBox.Text;
         }

         return name;
      }
   }
}
