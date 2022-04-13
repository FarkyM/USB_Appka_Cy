using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace USB_Appka_Cy
{
    
    public class WriteToFile
    {
       /* public static void StartWriteThread(object TxBuffer)
        {
            
            if (TxBuffer == null)
                return;
            StartWriteEx((byte[][])TxBuffer);
        }*/
        public static void StartWriteThread(object TxQueue)
        {
            while (true)
            {
                try {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch(ThreadInterruptedException e)
                {
                    if (((ConcurrentQueue<byte[][]>)TxQueue).Count > 0)
                        WriteFromQueue((ConcurrentQueue<byte[][]>)TxQueue);
                }

            }

        }
        private static void StartWriteEx(byte[][] TxBuffer)
        {
            string FilePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "logy\\log.hex");
            FileStream fs = new FileStream(FilePath, FileMode.Create);
            using (BinaryWriter logfile = new BinaryWriter(fs))
            {
                foreach (byte[] array in TxBuffer)
                {
                    logfile.Write(array);

                }
            }
            for (int i = 0; i < 16; i++)
            {
                TxBuffer[i] = null;
            }

        }

        private static void WriteFromQueue(ConcurrentQueue<byte[][]> TxQueue)
        {
            string FilePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "logy\\log.dat");
            FileStream fs;
            if (File.Exists(FilePath))
            {
                fs = File.Open(FilePath, FileMode.Append);
            }              
            else
            {
                fs = File.Open(FilePath, FileMode.Create);
            }
                
            using (BinaryWriter logfile = new BinaryWriter(fs))
            {
                while (TxQueue.Count > 0)
                { //TODO osetrit null??
                    TxQueue.TryDequeue(out byte[][] Data);

                    foreach (byte[] Paket in Data)
                    {
                        if (Paket == null)
                            break;
                        logfile.Write(Paket);

                    }                    
                }
            }
        }

    }
}
