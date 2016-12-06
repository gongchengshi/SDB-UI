using Sdb.Ui.ViewModel;

namespace Sdb.Ui.Messages
{
   public class ShowConfig
   {}

   public class CloseQuery
   {
      public CloseQuery(QueryViewModel sender)
      {
         Sender = sender;
      }

      public QueryViewModel Sender { get; private set; }
   }
}
