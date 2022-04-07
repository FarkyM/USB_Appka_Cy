using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        DateTime t1, t2;
        TimeSpan elapsed;
        double XferBytes;
        long xferRate;
        Thread tListen;

        int len = 5;


        public Form1()
        {
            InitializeComponent();
            usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);
            // Get the first device having VendorID == 0x04B4 and ProductID == 0x00F1 -> Streamer example
            //myDevice = usbDevices[0x04B4, 0x00F1] as CyUSBDevice;
            myDevice = usbDevices[0] as CyUSBDevice; //první usb pouzivajici cyUSB3.sys -> dat pak napevno VID/PID
            if (myDevice != null)
            {
                lbl_Status.Text = myDevice.FriendlyName + " connected.";
                  foreach (CyUSBEndPoint endpt in myDevice.EndPoints)
                   {
                       if(endpt.bIn && (endpt.Attributes == 2)) //Attributes -> 2 je bulk
                           myBulkIn = endpt as CyBulkEndPoint;
                   }
            }
            else
            {
                lbl_Status.Text = "No FX3 device connected";
                btn_Info.Enabled = false;
                //Exit,Refresh nebo tak neco
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
            int PaketsPerXfer = 256; //udelat do boxiku?
            int XfersToQueue = 16;
            int BufSz = myBulkIn.MaxPktSize * PaketsPerXfer;


            myBulkIn.XferSize = BufSz;

            IsoPktBlockSize = 0; //0 pro bulk transfer

            bRunning = true;
            
            tListen = new Thread(new ThreadStart(XferThread);
            tListen.IsBackground = true;
            tListen.Priority = ThreadPriority.Highest;
            tListen.Start();


            else
            {
                if (tListen.IsAlive)
                {
                    DevicesComboBox.Enabled = true;
                    EndPointsComboBox.Enabled = true;
                    PpxBox.Enabled = true;
                    QueueBox.Enabled = true;
                    StartBtn.Text = "Start";
                    bRunning = false;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;
                    xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    xferRate = xferRate / (int)100 * (int)100;

                    if (tListen.Join(5000) == false)
                        tListen.Abort();

                    tListen = null;

                    StartBtn.BackColor = Color.Aquamarine;
                }

            }
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
        }

        private void DeviceTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selNode = DeviceTreeView.SelectedNode;
            log.Text = selNode.Tag.ToString();
        }
    }
}
    
