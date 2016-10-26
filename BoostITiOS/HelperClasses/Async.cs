using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BoostITiOS
{
    public static class Async
    {
        public static void BackgroundProcess(DoWorkEventHandler workerFunction, RunWorkerCompletedEventHandler completeFunction, object stateVariable = null)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = false;
            bw.WorkerReportsProgress = false;
            bw.DoWork += new DoWorkEventHandler(workerFunction);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completeFunction);
            if (stateVariable != null)
                bw.RunWorkerAsync(stateVariable);
            else            
                bw.RunWorkerAsync();
        }        
    }
}