using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DeckLinkAPI;
using FFM2;
using Media;

namespace DecklinkStreamer
{
    public partial class MainForm : Form, DeckLinkAPI.IDeckLinkInputCallback
    {
        long frameCount = 0;
        IDeckLink device;
        IDeckLinkInput input;
        byte[] mediaFrameBuffer = new byte[1920 * 1080 * 4];
        PipeServer pipeServer = null;

        public MainForm()
        {
            InitializeComponent();
            try
            {
                pipeServer = new PipeServer(@"test");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        private void StartDecklinkInput(DeckLinkAPI.IDeckLink device)
        {
            this.device = device;
            input = (DeckLinkAPI.IDeckLinkInput)device;
            input.SetCallback(this);
            input.EnableVideoInput(_BMDDisplayMode.bmdModeHD1080p25, _BMDPixelFormat.bmdFormat8BitYUV, _BMDVideoInputFlags.bmdVideoInputFlagDefault);
            input.StartStreams();
            frameCount = 0;
        }

        public void VideoInputFrameArrived(IDeckLinkVideoInputFrame videoFrame, IDeckLinkAudioInputPacket audioPacket)
        {
            try
            {
                if (videoFrame != null)
                {
                    IntPtr srcPtr;
                    videoFrame.GetBytes(out srcPtr);
                    System.Runtime.InteropServices.Marshal.Copy(srcPtr, mediaFrameBuffer, 0, videoFrame.GetRowBytes() * videoFrame.GetHeight());
                    pipeServer.SendPacket(mediaFrameBuffer, 0, videoFrame.GetRowBytes() * videoFrame.GetHeight());
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(videoFrame);
                    //unsafe
                    //{
                    //    fixed (byte* p = mediaFrameBuffer)
                    //    {
                    //        IntPtr ptr = (IntPtr)p;
                    //        Bitmap bmp = new Bitmap(960, 1080, 1920*2, System.Drawing.Imaging.PixelFormat.Format32bppRgb, ptr);
                    //        frameCount++;
                    //        //if (frameCount % 25 == 0)
                    //        {
                    //            if (InvokeRequired) this.Invoke((Action)(() =>
                    //            {
                    //                //this.frameCounterLabel.Text = "Frame count: " + frameCount++;

                    //                pictureBox.Image = bmp;
                    //            }));
                    //        }
                    //    }
                    //}
                }

                if (audioPacket != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(audioPacket);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {
            MessageBox.Show("format changed");
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            try
            {
                DeckLinkAPI.IDeckLink decklink;
                DeckLinkAPI.CDeckLinkIterator decklinkIterator = new DeckLinkAPI.CDeckLinkIterator();
                do
                {
                    decklinkIterator.Next(out decklink);
                    if (decklink != null)
                    {
                        break;
                    }
                }
                while (decklink != null);
                if (decklink == null) throw new Exception("no decklink device found");
                StartDecklinkInput(decklink);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            pipeServer.Close();
        }

        private void testFFMButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (FileStream stream = File.OpenWrite("test.ffm"))
                {
                    FFM2Muxer muxer = new FFM2Muxer(stream);
                    muxer.AddTrack(new MediaDescriptor()
                    {
                        Bitrate = 0x31704000,
                        Codec = CodecEnum.RawVideo,
                        CodecType = CodecTypeEnum.Video,
                        ExtraData = null,
                        Flags = 0,
                        RecommendedEncoderConfiguration = "b=829440000,time_base=1/25,codec_tag=1498831189,pkt_timebase=1/1000000,pixel_format=uyvy422,video_size=1920x1080"
                    });
                    muxer.WriteHeader();
                    MessageBox.Show("ok");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
