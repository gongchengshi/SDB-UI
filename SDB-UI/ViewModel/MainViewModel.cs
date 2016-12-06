using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Input;
using Amazon;
using Amazon.SimpleDB.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Amazon.SimpleDB;
using Sdb.Ui.Messages;
using Sdb.Ui.Views;
using Gongchengshi;
using Gongchengshi.Collections.Generic;

namespace Sdb.Ui.ViewModel
{
   public class MainViewModel : ViewModelBase
   {
      public static IDialogService QueryErrorDialog;
      public static IDialogService DeleteQueryConfirmationDialog;
      public static DeleteDomainsConfirmationDialog DeleteDomainsConfirmationDialog;
      public static IDialogService SaveBeforeCloseDialog;
      public static IInputDialogService EnterNameDialog;

      private AmazonSimpleDBClient _sdb;
      private readonly ViewModelLocator _vmLocator = new ViewModelLocator();
      private readonly ConfigViewModel _configViewModel;

      public MainViewModel()
      {
         Domains = new ObservableCollection<DomainViewModel>();
         _configViewModel = _vmLocator.Config;

         _sdb = new AmazonSimpleDBClient(_configViewModel.AwsAccessKey, _configViewModel.AwsSecretKey, RegionEndpoint.USWest2);
         UpdateDomains();

         _configViewModel.AwsAccessKeysChanged += () =>
         {
            _sdb = new AmazonSimpleDBClient(_configViewModel.AwsAccessKey, _configViewModel.AwsSecretKey,
               RegionEndpoint.USWest2);
            UpdateDomains();
         };

         ClosingCommand = new RelayCommand(SaveRecent);
         ShowConfigViewCommand = new RelayCommand(() => Messenger.Default.Send(new ShowConfig()));
         ExecuteQueryCommand = new RelayCommand(() => CurrentQuery.Value.ExecuteQuery());
         PageRightCommand = new RelayCommand(() => CurrentQuery.Value.ExecuteNext());
         NewQueryCommand = new RelayCommand(() => OpenQuery(new QueryViewModel(this)));
         SaveQueryCommand = new RelayCommand(() => CurrentQuery.Value.Save());
         SaveAllQueriesCommand = new RelayCommand(() =>
         {
            foreach (var query in OpenQueries)
            {
               query.Save();
            }
         });
         RefreshCommand = new RelayCommand(UpdateDomains);
         DeleteDomainsCommand = new RelayCommand(DeleteDomains);
         CreateDomainCommand = new RelayCommand(CreateDomain);

         OpenQueries = new ObservableCollection<QueryViewModel>();
         SavedQueries = new ObservableCollection<QueryViewModel>();

         var recentQueries = new HashSet<string>();
         var storage = IsolatedStorageFile.GetUserStoreForAssembly();
         if (storage.FileExists("RecentQueries"))
         {
            using (var reader = new StreamReader(storage.OpenFile("RecentQueries", FileMode.Open, FileAccess.Read)))
            {
               string line;
               while ((line = reader.ReadLine()) != null)
               {
                  recentQueries.Add(line);
               }
            }
         }

         foreach (var path in storage.GetFileNames("SavedQueries\\*"))
         {
            var name = Path.GetFileNameWithoutExtension(path);
            using (var reader = new StreamReader(storage.OpenFile(Path.Combine("SavedQueries", path), FileMode.Open, FileAccess.Read)))
            {
               var query = new QueryViewModel(this, name, reader.ReadToEnd());
               SavedQueries.Add(query);
               if (recentQueries.Contains(name))
               {
                  OpenQueries.Add(query);
               }
            }
         }

         if (!OpenQueries.Any())
         {
            OpenQueries.Add(new QueryViewModel(this));
         }

         CurrentQuery.Value = OpenQueries.First();
      }

      private void CreateDomain()
      {
         var name = EnterNameDialog.Show();
         _sdb.CreateDomainAsync(new CreateDomainRequest {DomainName = name});
         UpdateDomains();
      }

      private void SaveRecent()
      {
         var storage = IsolatedStorageFile.GetUserStoreForAssembly();
         using (var writer = new StreamWriter(storage.CreateFile("RecentQueries")))
         {
            foreach (var query in OpenQueries)
            {
               if (query.IsNamed)
               {
                  writer.WriteLine(query.Name.Value);
               }
            }
         }
      }

      public RelayCommand ShowConfigViewCommand { private set; get; }
      public RelayCommand ExecuteQueryCommand { private set; get; }
      public RelayCommand PageRightCommand { private set; get; }
      public RelayCommand SaveQueryCommand { private set; get; }
      public RelayCommand SaveCurrentQueryCommand { private set; get; }
      public RelayCommand SaveAllQueriesCommand { private set; get; }
      public RelayCommand NewQueryCommand { private set; get; }
      public RelayCommand RefreshCommand { private set; get; }
      public RelayCommand DeleteDomainsCommand { private set; get; }
      public RelayCommand CreateDomainCommand { private set; get; }

      public ObservableCollection<DomainViewModel> Domains { get; set; }

      public ObservableCollection<QueryViewModel> OpenQueries { get; set; }
      public ObservableCollection<QueryViewModel> SavedQueries { get; set; }

      public Property<QueryViewModel> CurrentQuery { get { return _currentQuery; } }
      private readonly Property<QueryViewModel> _currentQuery = new Property<QueryViewModel>();

      private void DeleteDomains()
      {
         var selectedDomains = Domains.Where(d => d.IsSelected.Value).ToList();

         if (DeleteDomainsConfirmationDialog.Show(string.Join("", selectedDomains.Select(d => "\n\t" + d.Name))))
         {
            foreach (var domain in selectedDomains)
            {
               _sdb.DeleteDomainAsync(new DeleteDomainRequest {DomainName = domain.Name});
               Domains.Remove(domain);
            }
         }
      }

      private void UpdateDomains()
      {
         Domains.Clear();
         var domainTree = new PrefixTree<string, DomainViewModel>();

         var listDomainsResponse = _sdb.ListDomains();
         foreach (var domainName in listDomainsResponse.DomainNames)
         {
            var parts = domainName.Split('.');
            var domain = new DomainViewModel(domainName);
            domainTree.Add(parts, domain);
            Domains.Add(domain);
         }
      }

      public void OpenQuery(QueryViewModel query)
      {
         if (OpenQueries.Contains(query))
         {
            CurrentQuery.Value = query;
            return;
         }
         OpenQueries.Add(query);
         CurrentQuery.Value = query;
      }

      public void CloseQuery(QueryViewModel query)
      {
         if (!query.IsSaved.Value && SaveBeforeCloseDialog.Show(query.Name.Value))
         {
            query.Save();
         }

         OpenQueries.Remove(query);

         if (CurrentQuery.Value == query)
         {
            if (OpenQueries.Any())
            {
               CurrentQuery.Value = OpenQueries.First();
            }
            else
            {
               OpenQuery(new QueryViewModel(this));
            }
         }
      }

      public void DeleteQuery(QueryViewModel query)
      {
         if (!DeleteQueryConfirmationDialog.Show(query.Name.Value))
         {
            return;
         }

         CloseQuery(query);

         SavedQueries.Remove(query);

         var storage = IsolatedStorageFile.GetUserStoreForAssembly();
         storage.DeleteFile(Path.Combine("SavedQueries", query.Name.Value));
      }

      public ICommand ClosingCommand { get; set; }
   }
}
