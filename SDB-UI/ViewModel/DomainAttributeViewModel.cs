using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using Gongchengshi;

namespace Sdb.Ui.ViewModel
{
   public class DomainPrefixViewModel : ViewModelBase
   {
      public DomainPrefixViewModel(string name)
      {
         Name = name;
      }

      public string Name { get; private set; }
      public Property<bool> IsSelected { get { return _isSelected; } }
      private readonly Property<bool> _isSelected = new Property<bool>(false);
      public ObservableCollection<DomainPrefixViewModel> Children { get; set; }
   }

   public class DomainViewModel : DomainPrefixViewModel
   {
      public DomainViewModel(string name) : base(name)
      {
      }
   }

   public class DomainAttributeViewModel : ViewModelBase
   {
      public DomainAttributeViewModel(string name)
      {
         Name = name;
      }

      public string Name { get; private set; }
      public Property<bool> IsSelected { get { return _isSelected; } }
      private readonly Property<bool> _isSelected = new Property<bool>(false);
   }

   public class DomainTreeViewModel : ViewModelBase
   {
      public DomainTreeViewModel(string name)
      {
         
      }
   }
}
