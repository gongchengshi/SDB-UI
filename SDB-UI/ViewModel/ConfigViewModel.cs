using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Sdb.Ui.ViewModel
{
   public class ConfigViewModel : ViewModelBase
   {
      public ConfigViewModel()
      {
#if DEBUG
         AwsAccessKey = "AKIAIEWGSYRFXIUJ7KTQ";
         AwsSecretKey = "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";
#endif

         SaveConfigCommand = new RelayCommand(() =>
         {
            Properties.Settings.Default.AwsAccessKey = AwsAccessKey;
            Properties.Settings.Default.AwsSecretKey = AwsSecretKey;
            AwsAccessKeysChanged();
            Properties.Settings.Default.Save();
         });
      }

      public event Action AwsAccessKeysChanged;

      public RelayCommand SaveConfigCommand { private set; get; }

      public string AwsAccessKey { get; set; }
      public string AwsSecretKey { get; set; }
   }
}
