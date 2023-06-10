namespace DataProcessing.Generic
{
    /// <summary>
    /// A Data Reader is a class responsible of the Data Aquisition Layer. 
    /// So transforming raw Data in a program-readable Data. 
    /// 
    /// It hold only reading logic, and has no knowlege of visuals or Data Collection. 
    /// It just reads one data after the next.
    /// 
    /// The goal is to have a Data reader generic enough to be able to read Streams or Fixed Data.
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// Create the runtime variable used later by the entity
        /// Manage the init position of a data cursor
        /// 
        /// </summary>
        void Init();

        /// <summary>
        /// Clear the necessary ressources : 
        /// Connexion / Objects / Semaphores etc...
        /// 
        /// </summary>
        void Clean();
    
        /// <summary>
        /// Get Data at the current cursor
        /// </summary>
        TimedData GetData();

        /// <summary>
        /// Move the cursor
        /// </summary>
        void GoToNextData();
    }
}
