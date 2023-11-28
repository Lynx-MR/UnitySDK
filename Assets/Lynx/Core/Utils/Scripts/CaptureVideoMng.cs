using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Lynx
{
    public class CaptureVideoMng : MonoBehaviour
    {
        // Public Properties
        public int m_maxFrames = 5000;     // maximum number of frames you want to record in one video
        public int m_frameRate = 25; // number of frames to capture per second

        // private 
        Texture2D mTextureVideoReceive = null;

        // important : size of one eye screen for lynx. 
        int mWidth = 1536;
        int mHeight = 1404;

        int mResizedWidth;
        int mResizedHeight;

        int mSizeInBytes = 0;

        RenderTexture mRenderTexture;
        Texture2D mResizedTexture;


        // Lynx Android plug in part : 
        private const string mLynxPluginInName = "com.lynx.lynxandroidsystemcom.LynxAndroidSystemComMng";
        private AndroidJavaClass mAndroidSystemComPlugIn = null;
        private AndroidJavaObject mCurrentActivity = null;
        private AndroidJavaObject mApplicationContext = null;

        // Serialize frame part 
        private bool mThreadIsProcessing;
        private bool mTerminateThreadWhenDone;
        //private bool mProcessStarted = false;

        private string mTempFramePath;

        private List<byte[]> mFrameQueue;
        private int mSavingFrameNumber;

        // Timing Data
        private float mCaptureFrameTime;
        private float mLastFrameTime;
        private int frameNumber;


        // The Encoder Thread
        private Thread mEncoderThread;

        // just to test perf :
        private System.Diagnostics.Stopwatch m_sw = new System.Diagnostics.Stopwatch();

        private Material PlaneMaterialForVideo;

        private void InitAndroidJavaPlugin()
        {
            try
            {
                Debug.Log("begin InitAndroidJavaPlugin");

                var plugin = new AndroidJavaClass(mLynxPluginInName);

                if (plugin != null)
                {
                    mAndroidSystemComPlugIn = plugin;
                }
                else
                {
                    Debug.LogError("mAndroidSystemComPlugIn is NULL");
                    return;
                }

                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

                if (unityPlayer == null)
                {
                    Debug.LogError("unityPlayer is NULL");
                    return;
                }


                mCurrentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                if (mCurrentActivity == null)
                {
                    Debug.LogError("CurrentActivity is NULL");
                    return;
                }


                mApplicationContext = mCurrentActivity.Call<AndroidJavaObject>("getApplicationContext");

                if (mApplicationContext == null)
                {
                    Debug.LogError("ApplicationContext is NULL");
                    return;
                }

                // test a fonction of the plugin :
                // code frame encoding :           
                int BatteryLevel = mAndroidSystemComPlugIn.CallStatic<int>("getBatteryPercentage", mApplicationContext);
                Debug.Log("BatteryLevel to test plug in validity :" + BatteryLevel);


                mAndroidSystemComPlugIn.CallStatic("createVideoEncoder", mApplicationContext);

                Debug.Log("----------- Init AndroidSystemComPlugIn SUCCEED");

            }
            catch (System.Exception e)
            {
                Debug.LogError(e, this);
                Debug.Log("----------- Init AndroidSystemComPlugIn FAILED");
            }
        }


        void Awake()
        {
            mSizeInBytes = mWidth * mHeight * 4; // RGBA

            mTextureVideoReceive = new Texture2D(mWidth, mHeight, TextureFormat.RGBA32, false);

            mResizedWidth = mWidth / 2;  // 768  
            mResizedHeight = mHeight / 2;  // 702

            mRenderTexture = new RenderTexture(mResizedWidth, mResizedHeight, 24);
            mResizedTexture = new Texture2D(mResizedWidth, mResizedHeight);

            mFrameQueue = new List<byte[]>();
        }


        void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitAndroidJavaPlugin();
#endif
        }

        public void RecordVideo()
        {

        }

        public void StopRecord()
        {

        }

        bool isNewFrameAvailable = false;
        int indexOfAvailableFrame = -1;

        void SetFrameStateToEncoder()
        {
            if (isNewFrameAvailable)
            {
                mAndroidSystemComPlugIn.CallStatic("setFrameAvailable", indexOfAvailableFrame);
                isNewFrameAvailable = false;
            }
        }


        void SetNewFrameAvailable(int iFrameNumber)
        {
            isNewFrameAvailable = true;
            indexOfAvailableFrame = iFrameNumber;
        }


        private void EncodeAndSave()
        {
            while (mThreadIsProcessing)
            {
                if (mFrameQueue.Count > 0)
                {
                    // attention
                    string path = mTempFramePath + "/frame" + mSavingFrameNumber + ".raw";

                    // a remettre
                    System.IO.File.WriteAllBytes(path, mFrameQueue[0]);

                    //mFrameQueue.Remove(myBytes);
                    mFrameQueue.RemoveAt(0);

                    GC.Collect();

                    SetNewFrameAvailable(mSavingFrameNumber);

                    // Done
                    mSavingFrameNumber++;
                }
                else
                {

                    if (mTerminateThreadWhenDone)
                    {
                        break;
                    }

                    Thread.Sleep(1);

                }
            }

            mTerminateThreadWhenDone = false;
            mThreadIsProcessing = false;
            mFrameQueue.Clear();

            Debug.Log("FRAMES SAVER THREAD FINISHED");
        }


        private static void VerticallyFlipRenderTexture(RenderTexture target)
        {
            var temp = RenderTexture.GetTemporary(target.descriptor);
            Graphics.Blit(target, temp, new Vector2(1, -1), new Vector2(0, 1));
            Graphics.Blit(temp, target);
            RenderTexture.ReleaseTemporary(temp);
        }


        void DisplayNativeImageBufferVideo()
        {
            // Load data into the texture and upload it to the GPU.
            //mTextureVideoReceive.LoadRawTextureData(mManagedArray);
            mTextureVideoReceive.Apply();

            // Assign texture to renderer's material.
            PlaneMaterialForVideo.mainTexture = mTextureVideoReceive;
        }
    }
}