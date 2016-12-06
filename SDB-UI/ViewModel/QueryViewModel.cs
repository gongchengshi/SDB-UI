using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using Amazon;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Gongchengshi;

namespace Sdb.Ui.ViewModel
{
   public class QueryViewModel : ViewModelBase
   {
      private readonly MainViewModel _mainViewModel;
      public Property<string> Name { get { return _name; } }
      private readonly Property<string> _name = new Property<string>();

      public bool IsNamed { get { return !string.IsNullOrEmpty(Name.Value); } }

      public Property<bool> IsSaved { get { return _isSaved; } }
      private readonly Property<bool> _isSaved = new Property<bool>(true);

      public Property<string> QueryText { get { return _queryText; } }
      private readonly Property<string> _queryText = new Property<string>();

      public Property<DataTable> QueryResults { get { return _queryResults; } }
      private readonly Property<DataTable> _queryResults = new Property<DataTable>();

      private AmazonSimpleDBClient _sdb;
      private readonly ViewModelLocator _vmLocator = new ViewModelLocator();
      private readonly ConfigViewModel _configViewModel;
      private string _nextToken;

      public RelayCommand CloseQueryCommand { get; private set; }
      public RelayCommand OpenQueryCommand { private set; get; }
      public RelayCommand DeleteQueryCommand { private set; get; }

      public QueryViewModel(MainViewModel mainViewModel)
      {
         _mainViewModel = mainViewModel;
         _configViewModel = _vmLocator.Config;
         _sdb = new AmazonSimpleDBClient(_configViewModel.AwsAccessKey, _configViewModel.AwsSecretKey, RegionEndpoint.USWest2);

         _configViewModel.AwsAccessKeysChanged += () =>
         {
            _sdb = new AmazonSimpleDBClient(_configViewModel.AwsAccessKey, _configViewModel.AwsSecretKey, RegionEndpoint.USWest2);
         };

         new DerivedProperty<bool>(_isSaved, () => false, QueryText, Name);
         CloseQueryCommand = new RelayCommand(() => _mainViewModel.CloseQuery(this));
         OpenQueryCommand = new RelayCommand(() => _mainViewModel.OpenQuery(this));
         DeleteQueryCommand = new RelayCommand(() => _mainViewModel.DeleteQuery(this));
      }

      // Used when building a saved QueryViewModel.
      public QueryViewModel(MainViewModel mainViewModel, string name, string queryText) : this(mainViewModel)
      {
         Name.Value = name;
         QueryText.Value = queryText;
         IsSaved.Value = true;         
      }

      private void Execute()
      {
         SelectResponse response;

         try
         {
            response = _sdb.Select(new SelectRequest { SelectExpression = QueryText.Value });
         }
         catch (AmazonSimpleDBException ex)
         {
            MainViewModel.QueryErrorDialog.Show(ex.Message);
            return;
         }

         var dt = new DataTable();
         var foundColumnNames = new HashSet<string>();

         dt.Columns.Add("name");
         foundColumnNames.Add("name");

         foreach (var item in response.Items)
         {
            var row = dt.NewRow();
            row["name"] = item.Name;
            foreach (var attribute in item.Attributes)
            {
               if (!dt.Columns.Contains(attribute.Name))
               {
                  dt.Columns.Add(attribute.Name);
               }

               row[attribute.Name] = attribute.Value;
            }
            dt.Rows.Add(row);
         }

         QueryResults.Value = dt;
      }

      private SelectResponse RunQuery(bool getNext = false)
      {
         SelectResponse response;

         try
         {
            response = _sdb.Select(new SelectRequest { 
               SelectExpression = QueryText.Value, 
               NextToken = (getNext && _nextToken != null) ? _nextToken : null});
         }
         catch (AmazonSimpleDBException ex)
         {
            MainViewModel.QueryErrorDialog.Show(ex.Message);
            return null;
         }

         _nextToken = response.NextToken;
         return response;
      }

      private void SetQueryResults(SelectResponse selectResponse)
      {
         var dt = new DataTable();
         var foundColumnNames = new HashSet<string>();

         dt.Columns.Add("name");
         foundColumnNames.Add("name");

         foreach (var item in selectResponse.Items)
         {
            var row = dt.NewRow();
            row["name"] = item.Name;
            foreach (var attribute in item.Attributes)
            {
               if (!dt.Columns.Contains(attribute.Name))
               {
                  dt.Columns.Add(attribute.Name);
               }

               row[attribute.Name] = attribute.Value;
            }
            dt.Rows.Add(row);
         }

         QueryResults.Value = dt;         
      }

      public void ExecuteQuery()
      {
         var response = RunQuery();
         if (response == null)
         {
            return;
         }

         SetQueryResults(response);
      }

      public void ExecuteNext()
      {
         var response = RunQuery(true);
         if (response == null)
         {
            return;
         }

         SetQueryResults(response);
      }

      public void Save()
      {
         if (!IsNamed)
         {
            Name.Value = MainViewModel.EnterNameDialog.Show();
         }

         if (IsSaved.Value)
         {
            return;
         }

         var storage = IsolatedStorageFile.GetUserStoreForAssembly();
         using (var writer = new StreamWriter(storage.OpenFile(Path.Combine("SavedQueries", Name.Value), FileMode.Create, FileAccess.Write)))
         {
            writer.Write(QueryText.Value);
         }

         IsSaved.Value = true;
      }
   }
}
