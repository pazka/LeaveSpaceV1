using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

namespace DataProcessing.Generic
{
    public abstract class DataExtrapolator : IDataExtrapolator
    {
        protected readonly Mutex GeneratingData;
        protected Thread GenerationThread;
        
        public DataExtrapolator()
        {
            GeneratingData = new Mutex();
        }
        
        public void InitExtrapolation(IEnumerable<IData> inputData,object parameters)
        {
            WaitForMutex();
            SetConcreteDataToExtrapolate(inputData);
            ReleaseMutex();
                
            GenerationThread = new Thread(ExecuteExtrapolation);
            GenerationThread.IsBackground = true;  
            GenerationThread.Start(parameters);
        }
        
        public IEnumerable<IData> RetrieveExtrapolation() {
                GenerationThread.Join();
            
            return GetConcreteExtrapolation();
        }
        
        protected abstract IEnumerable<IData> GetConcreteExtrapolation();
        protected abstract void SetConcreteDataToExtrapolate(IEnumerable<IData> dataToExtrapolate);

        protected void WaitForMutex()
        {
            GeneratingData.WaitOne();
        }
        protected void ReleaseMutex()
        {
            GeneratingData.ReleaseMutex();
        }
        
        protected abstract void ExecuteExtrapolation(object parameters);
    }
}