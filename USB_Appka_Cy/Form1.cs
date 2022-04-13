using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using CyUSB;
/*
 * BOS - Binary (Device) Object Store
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */
namespace USB_Appka_Cy
{
    public partial class Form1 : Form
    {
        USBDeviceList usbDevices;
        CyUSBDevice myDevice;
        CyBulkEndPoint myBulkIn = null;
        StringBuilder sb = new StringBuilder();
        string FilePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"log.txt");
        DateTime t1, t2;
        TimeSpan elapsed;
        double XferBytes;
        long xferRate;

        Thread tListen;
        bool bRunning;

        int len = 5;
        int PaketsPerXfer = 256; //udelat do boxiku?
        int XfersToQueue = 16;
        int BufSz;
        int Successes=0;
        int Failures=0;

        // These are needed to close the app from the Thread exception(exception handling)
        delegate void ExceptionCallback();
        ExceptionCallback handleException;

        // These are  needed for Thread to update the UI
        delegate void UpdateUICallback();
        UpdateUICallback updateUI;

        public Form1()
        {
            InitializeComponent();
            usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);
            SelectDevice();
 

            // Setup the callback routine for updating the UI
            updateUI = new UpdateUICallback(StatusUpdate);



        }

        /*  public unsafe void ListenThread()
          {
              if (myDevice == null) return;
              CyBulkEndPoint InEndpt = myDevice.BulkInEndPt;
              byte i = 0;
              int PaketsPerXfer = 256; //udelat do boxiku?
              int XfersToQueue = 16;

              int BufSz = InEndpt.MaxPktSize * PaketsPerXfer;
              InEndpt.XferSize = BufSz;

              // Setup the queue buffers
              byte[][] cmdBufs = new byte[XfersToQueue][];
              byte[][] xferBufs = new byte[XfersToQueue][];
              byte[][] ovLaps = new byte[XfersToQueue][];
              for (i = 0; i < XfersToQueue; i++)
              {
                  cmdBufs[i] = new byte[CyConst.SINGLE_XFER_LEN + ((InEndpt.XferMode == XMODE.BUFFERED) ? BufSz : 0)];
                  xferBufs[i] = new byte[BufSz];
                  ovLaps[i] = new byte[CyConst.OverlapSignalAllocSize];

                  fixed (byte* tmp0 = ovLaps[i])
                  {
                      OVERLAPPED* ovLapStatus = (OVERLAPPED*)tmp0;
                      ovLapStatus->hEvent = PInvoke.CreateEvent(0, 0, 0, 0);
                  }
              }
              // Pre-load the queue with requests
              int len = BufSz;

              for (i = 0; i < XfersToQueue; i++)
                  InEndpt.BeginDataXfer(ref cmdBufs[i], ref xferBufs[i], ref len, ref ovLaps[i]);
              i = 0;
              int Successes = 0;
              int Failures = 0;
              XferBytes = 0;
              t1 = DateTime.Now;
              for (; StartBtn.Text.Equals("Stop");)
              {
                  fixed (byte* tmp0 = ovLaps[i])
                  {
                      OVERLAPPED* ovLapStatus = (OVERLAPPED*)tmp0;
                      if (!InEndpt.WaitForXfer(ovLapStatus->hEvent, 500))
                      {
                          InEndpt.Abort();
                          PInvoke.WaitForSingleObject(ovLapStatus->hEvent, CyConst.INFINITE);
                      }
                  }
                  if (InEndpt.FinishDataXfer(ref cmdBufs[i], ref xferBufs[i], ref len, ref ovLaps[i]))
                  {
                      XferBytes += len;
                      Successes++;
                  }
                  else
                      Failures++;

                  // Re-submit this buffer into the queue
                  len = BufSz;
                  InEndpt.BeginDataXfer(ref cmdBufs[i], ref xferBufs[i], ref len, ref ovLaps[i]);
                  i++;
                  if (i == XfersToQueue)
                  {
                      i = 0;
                      t2 = DateTime.Now;
                      elapsed = t2 - t1;
                      xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                      xferRate = xferRate / (int)100 * (int)100;
                      if (xferRate > ProgressBar.Maximum)
                      ProgressBar.Maximum = (int)(xferRate * 1.25);
                      ProgressBar.Value = (int)xferRate;
                      ThroughputLabel.Text = ProgressBar.Value.ToString();
                      SuccessBox.Text = Successes.ToString();
                      FailuresBox.Text = Failures.ToString();
                      Thread.Sleep(0);
                  }
              }
          }

          */

        /*Summary
 Data Xfer Thread entry point. Starts the thread on Start Button click 
*/
        public unsafe void XferThread()
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
                this.Invoke(handleException);
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
        public unsafe void LockNLoad(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo)
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
        public unsafe void XferData(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo, GCHandle[] handleOverlap)
        {
            int k = 0;
            int len = 0;

            Successes = 0;
            Failures = 0;

            XferBytes = 0;
            t1 = DateTime.Now;
            long nIteration = 0;
            CyUSB.OVERLAPPED ovData = new CyUSB.OVERLAPPED();

            for (; bRunning;)
            {
                nIteration++;
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
                    sb.Append(xBufs[k]);
                    XferBytes += len;
                        Successes++;
                    }
                    else
                        Failures++;


                // Re-submit this buffer into the queue
                len = BufSz;
                if (myBulkIn.BeginDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]) == false)
                    Failures++;

                k++;
                if (k == XfersToQueue)  // Only update displayed stats once each time through the queue
                {
                    k = 0;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;

                    xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    xferRate = xferRate / (int)100 * (int)100;

                    // Call StatusUpdate() in the main thread
                    if (bRunning == true) this.Invoke(updateUI);

                    // For small XfersToQueue or PPX, the loop is too tight for UI thread to ever get service.   
                    // Without this, app hangs in those scenarios.
                    Thread.Sleep(0);
                }
                Thread.Sleep(0);

            } // End infinite loop
            // Let's recall all the queued buffer and abort the end point.
            File.AppendAllText(FilePath, sb.ToString());
            sb.Clear();
            myBulkIn.Abort();
        }















        /*
         *          |       |
         *          |       |
         *          |       |
         *         \ /     \ /
         *         UI Handles
         */


        void usbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;
            lbl_Status.Text = usbEvent.FriendlyName + " removed.";
        }
        void usbDevices_DeviceAttached(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;
            lbl_Status.Text = usbEvent.Device.FriendlyName + " connected.";
        }

        private void btn_Info_Click(object sender, EventArgs e)
        {
            if (myDevice.bSuperSpeed)
            {
                log.Text = "USB3.0 Device";
            }
            else
            {
                log.Text = "Nejsem USB3.0 device";
            }

        }

        private void fX3SPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //mozna do budoucna
        }

        private void bOSInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string usb3_BOS = myDevice.USBBos.ToString();
            log.Text = usb3_BOS;
        }

        private void btn_recive_Click(object sender, EventArgs e)
        {
            if (myDevice == null)
                return;

            /*  if (QueueBox.Text == "")
              {
                  MessageBox.Show("Please Select Xfers to Queue", "Invalid Input");
                  return;
              }*/

            btn_close.Enabled = true;
            PaketsPerXfer = 256; //udelat do boxiku?
            XfersToQueue = 16;
            BufSz = myBulkIn.MaxPktSize * PaketsPerXfer;


            myBulkIn.XferSize = BufSz;

            bRunning = true;
            
            tListen = new Thread(new ThreadStart(XferThread));
            tListen.IsBackground = true;
            tListen.Priority = ThreadPriority.Highest;
            tListen.Start();          
            
        }
        /*
         * Veme si strukturu device infa a rozepise vlastnosti jednotlive node.
         */
        private void treeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceTreeView.Nodes.Clear();
            log.Text = "";
            foreach (USBDevice dev in usbDevices)
                DeviceTreeView.Nodes.Add(dev.Tree);
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            btn_close.Enabled = false;
            if (tListen.IsAlive)
            {

                bRunning = false;

                t2 = DateTime.Now;
                elapsed = t2 - t1;
                xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                xferRate = xferRate / (int)100 * (int)100;

                if (tListen.Join(5000) == false)
                    tListen.Abort();

                tListen = null;

                btn_close.Enabled = false;
            }

        }

        private void DeviceTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selNode = DeviceTreeView.SelectedNode;
            log.Text = selNode.Tag.ToString();
        }

        /*Summary
          The callback routine delegated to updateUI.
        */
        public void StatusUpdate()
        {
            if (bRunning == false) return;
          /* if (xferRate > ProgressBar.Maximum)
                ProgressBar.Maximum = (int)(xferRate * 1.25);

            ProgressBar.Value = (int)xferRate;
            ThroughputLabel.Text = ProgressBar.Value.ToString();

            SuccessBox.Text = Successes.ToString();
            FailuresBox.Text = Failures.ToString();*/
          lbl_Throughout.Text = xferRate.ToString();
        }

        public void SelectDevice()
        {
            // Get the first device having VendorID == 0x04B4 and ProductID == 0x00F1 -> Streamer example
            //myDevice = usbDevices[0x04B4, 0x00F1] as CyUSBDevice;
            myDevice = usbDevices[0] as CyUSBDevice; //první usb pouzivajici cyUSB3.sys -> dat pak napevno VID/PID
            if (myDevice != null)
            {
                lbl_Status.Text = myDevice.FriendlyName + " connected.";
                foreach (CyUSBEndPoint endpt in myDevice.EndPoints)
                {
                    if (endpt.bIn && (endpt.Attributes == 2)) //Attributes -> 2 je bulk
                        myBulkIn = endpt as CyBulkEndPoint;
                }


                if (myDevice.bSuperSpeed)
                {
                    lbl_led.Text = "USB 3.0 Compatible Device";
                    led_SS.BackColor = Color.PaleGreen;
                }
                else
                {
                    lbl_led.Text = "Device is not USB3 compatible";
                    led_SS.BackColor = Color.Red;
                }

            }
            else
            {
                lbl_Status.Text = "No FX3 device connected";
                btn_Info.Enabled = false;
                //Exit,Refresh nebo tak neco
            }

        }

    }
}
    
