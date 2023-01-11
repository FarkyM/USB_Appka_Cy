using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CyUSB; //potreba?

namespace USB_Appka_Cy
{
    internal class ListenThread : Form1
    {


        public static void StartListenThread(object config)
        {

            XferThread();

        }

        /*Summary
Data Xfer Thread entry point. Starts the thread on Start Button click 
*/
        public static unsafe void XferThread()
        {

            // Setup the queue buffers
            byte[][] cmdBufs = new byte[XfersToQueue][];
            byte[][] xferBufs = new byte[XfersToQueue][];
            byte[][] ovLaps = new byte[XfersToQueue][];
            ISO_PKT_INFO[][] pktsInfo = new ISO_PKT_INFO[XfersToQueue][];

            //int xStart = 0;

            //////////////////////////////////////////////////////////////////////////////
            ///////////////Pin the data buffer memory, so GC won't touch the memory///////
            //////////////////////////////////////////////////////////////////////////////

            GCHandle cmdBufferHandle = GCHandle.Alloc(cmdBufs[0], GCHandleType.Pinned);
            GCHandle xFerBufferHandle = GCHandle.Alloc(xferBufs[0], GCHandleType.Pinned);
            GCHandle overlapDataHandle = GCHandle.Alloc(ovLaps[0], GCHandleType.Pinned);
            GCHandle pktsInfoHandle = GCHandle.Alloc(pktsInfo[0], GCHandleType.Pinned);

            try
            {
                LockNLoad(cmdBufs, xferBufs, ovLaps, pktsInfo);
            }
            catch (NullReferenceException e)
            {
                // This exception gets thrown if the device is unplugged 
                // while we're streaming data
                e.GetBaseException();
               // this.Invoke(handleException);
            }

            //////////////////////////////////////////////////////////////////////////////
            ///////////////Release the pinned memory and make it available to GC./////////
            //////////////////////////////////////////////////////////////////////////////
            cmdBufferHandle.Free();
            xFerBufferHandle.Free();
            overlapDataHandle.Free();
            pktsInfoHandle.Free();
        }




        /*Summary
          This is a recursive routine for pinning all the buffers used in the transfer in memory.
        It will get recursively called XfersToQueue times.  On the XfersToQueue_th call, it will call
        XferData, which will loop, transferring data, until the stop button is clicked.
        Then, the recursion will unwind.
        */
        public static unsafe void LockNLoad(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo)
        {
            int j = 0;
            int nLocalCount = j;

            GCHandle[] bufSingleTransfer = new GCHandle[XfersToQueue];
            GCHandle[] bufDataAllocation = new GCHandle[XfersToQueue];
            GCHandle[] bufPktsInfo = new GCHandle[XfersToQueue];
            GCHandle[] handleOverlap = new GCHandle[XfersToQueue];

            while (j < XfersToQueue)
            {
                // Allocate one set of buffers for the queue, Buffered IO method require user to allocate a buffer as a part of command buffer,
                // the BeginDataXfer does not allocated it. BeginDataXfer will copy the data from the main buffer to the allocated while initializing the commands.
                cBufs[j] = new byte[CyConst.SINGLE_XFER_LEN + ((myBulkIn.XferMode == XMODE.BUFFERED) ? BufSz : 0)];

                xBufs[j] = new byte[BufSz];

                //initialize the buffer with initial value 0xA5
                for (int iIndex = 0; iIndex < BufSz; iIndex++)
                    xBufs[j][iIndex] = 0xA5;

                int sz = Math.Max(CyConst.OverlapSignalAllocSize, sizeof(OVERLAPPED));
                oLaps[j] = new byte[sz];
                pktsInfo[j] = new ISO_PKT_INFO[PaketsPerXfer];

                /*/////////////////////////////////////////////////////////////////////////////
                 * 
                 * fixed keyword is getting thrown own by the compiler because the temporary variables 
                 * tL0, tc0 and tb0 aren't used. And for jagged C# array there is no way, we can use this 
                 * temporary variable.
                 * 
                 * Solution  for Variable Pinning:
                 * Its expected that application pin memory before passing the variable address to the
                 * library and subsequently to the windows driver.
                 * 
                 * Cypress Windows Driver is using this very same memory location for data reception or
                 * data delivery to the device.
                 * And, hence .Net Garbage collector isn't expected to move the memory location. And,
                 * Pinning the memory location is essential. And, not through FIXED keyword, because of 
                 * non-usability of temporary variable.
                 * 
                /////////////////////////////////////////////////////////////////////////////*/
                //fixed (byte* tL0 = oLaps[j], tc0 = cBufs[j], tb0 = xBufs[j])  // Pin the buffers in memory
                //////////////////////////////////////////////////////////////////////////////////////////////
                bufSingleTransfer[j] = GCHandle.Alloc(cBufs[j], GCHandleType.Pinned);
                bufDataAllocation[j] = GCHandle.Alloc(xBufs[j], GCHandleType.Pinned);
                bufPktsInfo[j] = GCHandle.Alloc(pktsInfo[j], GCHandleType.Pinned);
                handleOverlap[j] = GCHandle.Alloc(oLaps[j], GCHandleType.Pinned);
                // oLaps "fixed" keyword variable is in use. So, we are good.
                /////////////////////////////////////////////////////////////////////////////////////////////            

                unsafe
                {
                    //fixed (byte* tL0 = oLaps[j])
                    {
                        CyUSB.OVERLAPPED ovLapStatus = new CyUSB.OVERLAPPED();
                        ovLapStatus = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[j].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                        ovLapStatus.hEvent = (IntPtr)PInvoke.CreateEvent(0, 0, 0, 0);
                        Marshal.StructureToPtr(ovLapStatus, handleOverlap[j].AddrOfPinnedObject(), true);

                        // Pre-load the queue with a request
                        int len = BufSz;
                        if (myBulkIn.BeginDataXfer(ref cBufs[j], ref xBufs[j], ref len, ref oLaps[j]) == false)
                            Failures++;
                    }
                    j++;
                }
            }

            XferData(cBufs, xBufs, oLaps, pktsInfo, handleOverlap);          // All loaded. Let's go!

            unsafe
            {
                for (nLocalCount = 0; nLocalCount < XfersToQueue; nLocalCount++)
                {
                    CyUSB.OVERLAPPED ovLapStatus = new CyUSB.OVERLAPPED();
                    ovLapStatus = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[nLocalCount].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                    PInvoke.CloseHandle(ovLapStatus.hEvent);

                    /*////////////////////////////////////////////////////////////////////////////////////////////
                     * 
                     * Release the pinned allocation handles.
                     * 
                    ////////////////////////////////////////////////////////////////////////////////////////////*/
                    bufSingleTransfer[nLocalCount].Free();
                    bufDataAllocation[nLocalCount].Free();
                    bufPktsInfo[nLocalCount].Free();
                    handleOverlap[nLocalCount].Free();

                    cBufs[nLocalCount] = null;
                    xBufs[nLocalCount] = null;
                    oLaps[nLocalCount] = null;
                }
            }
            GC.Collect();
        }

        /*Summary
          Called at the end of recursive method, LockNLoad().
          XferData() implements the infinite transfer loop
        */
        public static unsafe void XferData(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo, GCHandle[] handleOverlap)
        {
            int k = 0;
            int len = 0;
            bool FailFlag = false;
            Successes = 0;
            Failures = 0;

            XferBytes = 0;
            t1 = DateTime.Now;
            long nIteration = 0;
            CyUSB.OVERLAPPED ovData = new CyUSB.OVERLAPPED();

            for (; bRunning;)
            {
             //   nIteration++;
                // WaitForXfer
                unsafe
                {
                    //fixed (byte* tmpOvlap = oLaps[k])
                    {
                        ovData = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[k].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                        if (!myBulkIn.WaitForXfer(ovData.hEvent, 500))
                        {
                            myBulkIn.Abort();
                            PInvoke.WaitForSingleObject(ovData.hEvent, 500);
                        }
                    }
                }



                // FinishDataXfer
                if (myBulkIn.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]))
                {
                    // sb.Append(BitConverter.ToString(xBufs[k]).Replace("-"," "));
                    XferBytes += len;
                    Successes++;                              
                }
                else
                {
                    Failures++;
                    FailFlag=true;
                }



                // Re-submit this buffer into the queue
                len = BufSz;
                if (myBulkIn.BeginDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]) == false)
                    Failures++;
                //TODO pokud se nepovede tak neprenest
                if(FailFlag)
                {
                  k--;
                  FailFlag=false;
                }

                k++;

                if((Failures/(Failures+Successes+10.0)>0.5))
                {
                    bRunning = false;
                    ThreadEnd = true;
                    break;
                    
                }

                if (k == XfersToQueue)  // Only update displayed stats once each time through the queue
                {

                    DataQueue.Enqueue(xBufs);
                    if (tFileWrite.ThreadState == (ThreadState.WaitSleepJoin | ThreadState.Background))
                        tFileWrite.Interrupt();
                    k = 0;
                    
                    t2 = DateTime.Now;
                    elapsed = t2 - t1;

                    xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    xferRate = xferRate / (int)100 * (int)100;

                    // Call StatusUpdate() in the main thread
                    //if (bRunning == true) Invoke(updateUI);
                    
                    // For small XfersToQueue or PPX, the loop is too tight for UI thread to ever get service.   
                    // Without this, app hangs in those scenarios.
                    Thread.Sleep(0);
                }
                Thread.Sleep(0);

            } // End infinite loop
              // Let's recall all the queued buffer and abort the end point.
              //  tFileWrite.Start();

            myBulkIn.Abort();

        }


    }
}