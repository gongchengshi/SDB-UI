using System.Windows;

namespace Sdb.Ui.Views
{
   public partial class BasicInputBox : Window
   {
      public BasicInputBox(Window owner, string text, string buttonText)
      {
         Owner = owner;
         InitializeComponent();
         Label.Content = text;
         Button.Content = buttonText;
      }

      public BasicInputBox(Window owner, string text) : this(owner, text, "OK")
      {}

      private void Button_OnClick(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
