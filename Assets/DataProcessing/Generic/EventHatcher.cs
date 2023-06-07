using System.Collections;
using System.Collections.Generic;
using Microsoft.CSharp;

namespace DataProcessing.Generic
{
    /// <summary>
    /// TemplateMethod DP for Triggering new data.
    /// There no need to create an abstract factory as we only want generalize the triggering algorithm
    /// and concretize the trigger method.
    /// </summary>
    public abstract class EventHatcher<T>
    {
        /// <summary>
        ///  Trigger data that a ready to be triggered given a specific criteria
        /// </summary> 
        /// <param name="sortedData"></param>
        /// <param name="criteria"></param>
        /// <returns> Triggered data </returns>
        public ICollection<T> HatchEvents(Queue<T> sortedData,dynamic criteria)
        {
            ICollection < T > triggeredData = new List<T>();

            while (sortedData.Count > 0 && DecideIfReady(sortedData.Peek(), criteria))
            {
                triggeredData.Add(ExecuteData(sortedData.Dequeue()));
            }
            
            return triggeredData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="criteria"></param>
        /// <returns>true if Data is ready to be triggered</returns>
        protected abstract bool DecideIfReady(T data, dynamic criteria);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataToTrigger"></param>
        /// <returns>triggeredData</returns>
        protected abstract T ExecuteData(T dataToTrigger);

    }
}