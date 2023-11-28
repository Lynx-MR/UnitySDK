/**
 * @file VideoEncoderNatifMng.cs
 * 
 * @author Cédric Morel Francoz
 * 
 * @brief Manage encoder Video : all calls to native plug in are made here. 
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;


namespace Lynx
{
    public class VideoEncoderNatifMng
    {
        // Plug ins imported fonction : 
        [DllImport("AndroidNativeVideoEncoder")]
        private static extern void InitEncoder(string videoFileName, int width, int height, int fps, int bitRate);

        [DllImport("AndroidNativeVideoEncoder")]
        private static extern void EncodeFrame(byte[] data, int frameIndex);

        [DllImport("AndroidNativeVideoEncoder")]
        private static extern void EncodeRGBAFrame(byte[] data, int frameIndex);

        [DllImport("AndroidNativeVideoEncoder")]
        private static extern void EndEncoding();

        // Public Properties
        [Header("Max duration in seconds of one video shot")]
        public int m_maxDuration = 300; // 5 minutes. 

        // Size of one eye screen for lynx. 
        const int m_screenWidth  = 1536;
        const int m_screenHeight = 1404;

        // Size of the video. 
        int m_videoWidth;
        int m_videoHeight;

        Texture2D m_thumbnailTexture = null;

        // Serialize frame part 
        [HideInInspector]
        public bool m_processStarted = false;

        // Timing Data
        private int   m_frameRate = 25; 
        private float m_captureFrameTime;
        private float m_lastFrameTime;
        private int   m_frameIndex;
        private int   m_maxFrames = 0;
        private int   m_frameForThumbnailIndex = 15;

        // Video file file path
        private string m_videoFilePath;

        // Thumbnail file path computed from m_videoFilePath
        private string m_thumbnailPath;
        private bool   m_generateThumbnail = false;
        private bool   m_thumbnailRecorded = false;

        byte[] m_YuvBufferForThumbnail;

        // Thread part :  
        private bool m_threadIsProcessing = false;
        private bool m_terminateThreadWhenDone = false;
        // The Encoder Thread
        private Thread m_encoderThread = null;
        private List<byte[]> m_frameQueue = new List<byte[]>();

        // Just to test performance :
        private System.Diagnostics.Stopwatch m_sw = new System.Diagnostics.Stopwatch();


        public VideoEncoderNatifMng(int maxDuration = 300)
        {
            m_maxDuration = maxDuration;
            // useful for debug : 
            //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            //Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }


        /// <summary>
        /// Record a video with with fixed parameters :
        /// It generates a video file with compatible lynx launcher filename format : /sdcard/DCIM/Lynx/ScreenAndVideoShots/video_YYYY-MM-D_HH-MM-SS.mp4.
        /// It generates a thumbnail with compatible lynx launcher thumbnail filename format.
        /// Video width is fixed to 768 which is the half of the lynx screen
        /// Video height is fixed to 702 which is the half of the lynx screen
        /// fps = 25 img/s
        /// bit rate = 2 000 000 bits/s
        /// </summary>
        public void RecordVideo()
        {
            if (m_processStarted)
            {
                Debug.LogError("Impossible to record a new video because a record video process is already underway");
                Debug.Log("Please, stop former process");
                return;
            }
                
            RecordVideo("",
                        m_screenWidth / 2,
                        m_screenHeight / 2,
                        25,
                        2000000,
                        true);
        }

        /// <summary>
        /// Record a mp4 video of the lynx image flow.
        /// </summary>
        /// <param name="videoPath">The video file of the final mp4 video. Important : if videoPath is void, this function generates automatically a path 
        /// of this type : /sdcard/DCIM/Lynx/ScreenAndVideoShots/video_YYYY-MM-D_HH-MM-SS.mp4 compatible with the Lynx launcher</param>
        /// <param name="width">Video width</param>
        /// <param name="height">Video height</param>
        /// <param name="framePerSecond">The desired image per second for the video</param>
        /// <param name="bitRate">The bit rate of the video</param>
        /// <param name="generateThumbnail">A boolean that ask if we generate a thumbnail of the video or not</param>
        public void RecordVideo(string videoPath, int width, int height, int framePerSecond, int bitRate, bool generateThumbnail = true)
        {
            Debug.Log("Start record");
            m_videoWidth  = width;
            m_videoHeight = height;

            if (m_videoWidth%2 == 1 || m_videoHeight % 2 == 1)
            {
                Debug.LogError("Impossible to record video with a width or height not EVEN ");
                return;
            }

            if (videoPath.Length == 0)
            {
                // compute video file path : it's in DCIM/Lynx/screenAndVideoShots
                string strVideoPath = ScreenshotAndVideoUtilities.ComputeVideoShotPath();
                Debug.Log("RecordVideo with video file Path : " + strVideoPath);
                m_videoFilePath = strVideoPath;
            }
            else
            {
                m_videoFilePath = videoPath;
            }

            m_thumbnailRecorded = false;

            if (generateThumbnail)
            {
                m_generateThumbnail = true;
                string videoThumbnailPath;
                ScreenshotAndVideoUtilities.ComputeVideoThumbnailPath(m_videoFilePath, out videoThumbnailPath);
                m_thumbnailPath = videoThumbnailPath;
                Debug.Log("boolean m_generateThumbnail is TRUE and thumbnail path is : " + m_thumbnailPath);
                CreateThumbnailTexture(m_videoWidth, m_videoHeight);
                m_YuvBufferForThumbnail = new byte[m_videoWidth * m_videoHeight * 3/2];
            }

            m_frameIndex = 0;

            m_frameRate = framePerSecond;
            m_captureFrameTime = 1.0f / (float)m_frameRate;
            m_lastFrameTime = Time.time;
            m_maxFrames = m_maxDuration * m_frameRate;

            // First call to native plug in : 
            InitEncoder(m_videoFilePath,
                        m_videoWidth,
                        m_videoHeight,
                        framePerSecond,
                        bitRate);
         
            // Add a Thread
            // Kill the encoder thread if running from a previous execution
            if (m_encoderThread != null && (m_threadIsProcessing || m_encoderThread.IsAlive))
            {
                Debug.Log(" m_encoderThread.Join(); ");
                m_threadIsProcessing = false;
                m_encoderThread.Join();
            }

            // Start a new encoder thread
            m_threadIsProcessing = true;
            m_encoderThread = new Thread(EncodeFrameToMp4);
            m_encoderThread.Start();

            // Go !!!
            m_processStarted = true;
            Debug.Log("Video capture and encoding process START");
        }


        public void SetFrameAndEncode(byte[] bArray)
        {
            if (!m_processStarted) return;

            if (m_frameIndex <= m_maxFrames)
            {
                // Calculate number of video frames to produce from this game frame
                // Generate 'padding' frames if desired framerate is higher than actual framerate
                float thisFrameTime = Time.time;

                int framesToCapture = ((int)(thisFrameTime / m_captureFrameTime)) - ((int)(m_lastFrameTime / m_captureFrameTime));

                // Capture the frame :
                if (framesToCapture > 0)
                {
                    //m_sw.Reset();
                    //m_sw.Start();

                    // Fill the frame queue : 
                    m_frameQueue.Add(bArray);

                    //m_sw.Stop();

                    //Debug.Log(" encodeYUVFrame : " + m_sw.Elapsed.Ticks);
                    //Debug.Log(" encodeYUVFrame : " + m_sw.Elapsed.Milliseconds); //-> 20ms en half frame.  

                    m_frameIndex++;

                    if (m_generateThumbnail && m_frameIndex == m_frameForThumbnailIndex)
                    {
                        //Debug.Log("RecordThumbnail called");
                        RecordThumbnailFromYUV(bArray);
                    }
                }

                m_lastFrameTime = thisFrameTime;
            }
            else
            {
                Debug.Log("--------------------- End of capture because m_frameIndex > m_maxFrames");
                StopRecord();
            }      
        }


        public void StopRecord()
        {
            Debug.Log("Stop Video Record called");
            m_processStarted = false;
            m_terminateThreadWhenDone = true;

            EndEncoding();

            if (m_generateThumbnail)
            {        
                SaveVideoThumbnailFromYUV();
            }
          
            DestroyTextures();
        }

        private void CreateThumbnailTexture(int width, int height)
        {        
            m_thumbnailTexture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
        }

        private void DestroyTextures()
        {
            if (m_thumbnailTexture != null)
            {
                GameObject.Destroy(m_thumbnailTexture);
                m_thumbnailTexture = null;
            }
        }

        private void RecordThumbnail(byte[] rgbaBuffer)
        {       
            m_thumbnailRecorded = true;
            m_thumbnailTexture.LoadRawTextureData(rgbaBuffer);
        }


        private void RecordThumbnailFromYUV(byte[] yuvBuffer)
        {       
            m_thumbnailRecorded = true;    
            Array.Copy(yuvBuffer, m_YuvBufferForThumbnail, m_YuvBufferForThumbnail.Length);
        }

        private void encodeRGBAFrame(byte[] rgbaBuffer, int indexFrame)
        {
            EncodeRGBAFrame(rgbaBuffer, indexFrame);
        }

        private void encodeYUVFrame(byte[] yuvBuffer, int indexFrame)
        {
            EncodeFrame(yuvBuffer, indexFrame);
        }

        private void SaveVideoThumbnailFromYUV()
        {
            var fileInfo = new System.IO.FileInfo(m_videoFilePath);
            Debug.Log("Video file final length : " + fileInfo.Length + " bytes");

            if (m_thumbnailPath.Length != 0 && m_thumbnailRecorded && fileInfo.Length > 0 && m_generateThumbnail)
            {
                byte[] rgbBuffer = new byte[m_videoWidth * m_videoHeight * 3];

                ScreenshotAndVideoUtilities.YUVNV21_To_RGB(m_YuvBufferForThumbnail, rgbBuffer, m_videoWidth, m_videoHeight);

                m_thumbnailTexture.LoadRawTextureData(rgbBuffer);
            
                Texture2D newText = ScreenshotAndVideoUtilities.FlipTexture2DVertically(m_thumbnailTexture);
                byte[] bytes = newText.EncodeToJPG(60);       

                if (bytes.Length != 0)
                {
                    System.IO.File.WriteAllBytes(m_thumbnailPath, bytes);
                    Debug.Log("Video Thumbnail saved");
                }
                else
                {
                    Debug.LogError("Video Thumbnail NOT saved ");
                }
          
                if (newText != null)
                {
                    GameObject.Destroy(newText);
                }

                if (m_YuvBufferForThumbnail != null)
                    m_YuvBufferForThumbnail = null; // inform GC that m_YuvBufferForThumbnail can be collected. 
            }
            else
            {
                Debug.LogError("Video Thumbnail NOT saved because the video has not been generated correctly");
            }
        }


        private void SaveVideoThumbnailFromRGBAData()
        {
            var fileInfo = new System.IO.FileInfo(m_videoFilePath);
            Debug.Log("Video file final length : " + fileInfo.Length + " bytes");

            if (m_thumbnailPath.Length != 0 && m_thumbnailRecorded && fileInfo.Length > 0 && m_generateThumbnail)
            {
                Texture2D newText = ScreenshotAndVideoUtilities.FlipTexture2DVertically(m_thumbnailTexture);
                byte[] bytes = newText.EncodeToJPG(60);

                if (bytes.Length != 0)
                {
                    System.IO.File.WriteAllBytes(m_thumbnailPath, bytes);
                    Debug.Log("Video Thumbnail saved");
                }
                else
                {
                    Debug.LogError("Video Thumbnail NOT saved ");
                }

                if (newText != null)
                {
                    GameObject.Destroy(newText);
                }
            }
            else
            {
                Debug.LogError("Video Thumbnail NOT saved because the video has not been generated correctly");
            }
        }

        private void EncodeFrameToMp4()
        {      
            while (m_threadIsProcessing)
            {
                if (m_frameQueue.Count > 0)
                {                        
                    encodeYUVFrame(m_frameQueue[0], m_frameIndex);
                    m_frameQueue.RemoveAt(0);
                    GC.Collect();
                }
                else
                {
                    if (m_terminateThreadWhenDone)
                    {
                        //Debug.Log("m_terminateThreadWhenDone = true ");
                        break;
                    }

                    Thread.Sleep(1);
                }
            }

            m_terminateThreadWhenDone = false;
            m_threadIsProcessing = false;
            m_frameQueue.Clear();

            Debug.Log("FRAMES SAVER THREAD FINISHED");
        }
    }
}