using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;

namespace Sdb.Ui
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      static App()
      {
         var storage = IsolatedStorageFile.GetUserStoreForAssembly();
         if (!storage.DirectoryExists("SavedQueries"))
         {
            storage.CreateDirectory("SavedQueries");
         }
      }
   }
}
