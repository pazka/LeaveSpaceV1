using System.Collections.Generic;

namespace DataProcessing.Generic
{
    public interface IDataExtrapolator
    {
         void InitExtrapolation(IEnumerable<IData> inputData,object parameters);
         
         IEnumerable<IData> RetrieveExtrapolation();
    }
}