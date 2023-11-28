using System.Collections;
using UnityEngine;
using System.IO;

namespace Lynx
{
    /// <summary>
    /// Lynx Utilities to create screenshots.
    /// </summary>
    public class ScreenshotAndVideoUtilities : MonoBehaviour
    {
        //INSPECTOR
        [Header("The camera object that takes screenshots")]
        public GameObject m_cameraGameObjectForScreenShot;

        //PRIVATE
        private static int m_scWidth = 1024;
        private static int m_scHeight = 1024;

        private static string m_keywordForVideoThumbnailName = "_lynxthumb";

#if !UNITY_EDITOR && UNITY_ANDROID
    // Path the lynx shared folder where screenshots and video are stored. 
    private static string mLYNX_GALLERY_PATH = "/sdcard/DCIM/Lynx/ScreenAndVideoShots";
#endif

        public static string GetVideoThumbnailKeyword()
        {
            return m_keywordForVideoThumbnailName;
        }

        public void TakeShot()
        {
            GetGalleryPath();
            StartCoroutine(TakeScreenShotCoroutine(m_scWidth,
                                                   m_scHeight));
        }

        public static string GetGalleryPath()
        {
            string galleryFullPath;

#if !UNITY_EDITOR && UNITY_ANDROID
        galleryFullPath = mLYNX_GALLERY_PATH; 
        //Debug.Log("galleryFullPath on Android : " + galleryFullPath);// typically /storage/emulated/0/Android/data/com.a.LynxAppStartTest/files/
#else
            //Application.dataPath; // typically on Windows the Assets folder
            galleryFullPath = Application.dataPath + "/ScreenAndVideoShots";
#endif

            if (!System.IO.Directory.Exists(galleryFullPath))
            {
                Debug.Log("***** ScreenshotAndVideoUtilities::GetGalleryPath : Create Lynx folder because it doesn't exist");
                System.IO.Directory.CreateDirectory(galleryFullPath);
            }

            return galleryFullPath;
        }


        IEnumerator TakeScreenShotCoroutine(int resWidth, int resHeight)
        {
            yield return new WaitForEndOfFrame();
            TakeScreenShot(resWidth, resHeight);
        }



        /// <summary>
        /// Create a screenshot with given dimensions and save it with the day time.
        /// </summary>
        /// <param name="width">Width for the screenshot.</param>
        /// <param name="height">Height for the screenshot.</param>
        /// <returns>Saved file name.</returns>
        public void TakeScreenShot(int resWidth, int resHeight)
        {
            RenderTexture renderTexture = new RenderTexture(resWidth, resHeight, 24);
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                m_cameraGameObjectForScreenShot.SetActive(true);

                if (m_cameraGameObjectForScreenShot != null)
                {
                    //Debug.Log("goCamera find = " + cameraGameObjectForScreenShot.name);
                    Camera cameraForScreenshot = m_cameraGameObjectForScreenShot.GetComponent<Camera>();

                    if (cameraForScreenshot != null)
                    {
                        mainCamera = cameraForScreenshot;
                    }
                    else
                    {
                        Debug.LogWarning("No camera object found on Eye Left object");
                        return;
                    }
                }
            }

            mainCamera.targetTexture = renderTexture;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            mainCamera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            mainCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);
            byte[] bytes = screenShot.EncodeToJPG(60);//.EncodeToPNG();

            string filename = ComputeScreenShotPath();

            System.IO.File.WriteAllBytes(filename, bytes);

            Destroy(screenShot);
            Destroy(renderTexture);

            // cedric : change 05 septembre 2022 for Open XR version
            // don't desactivate the camera. it's no more the mono camera like on SVR, it's now on Open XR version the main running camera. 
            //cameraGameObjectForScreenShot.SetActive(false);

            Debug.Log("******** ScreenshotAndVideoUtilities::New screenshot taken with path : " + filename);
        }

        /// <summary>
        /// Create a screenshot file name based "timestamp".
        /// </summary>
        /// <returns>Saved file name.</returns>
        public static string ComputeScreenShotPath()
        {
            /*
            string filename = string.Format("{0}/screen_{1}.png",
                                        GetGalleryPath(),
                                        System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            */

            string filename = string.Format("{0}/screen_{1}.jpg",
                                        GetGalleryPath(),
                                        System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

            return filename;
        }

        /// <summary>
        /// Create a video file name based on "timestamp".
        /// </summary>
        /// <returns>Saved file name.</returns>
        public static string ComputeVideoShotPath()
        {
            string filename = string.Format("{0}/video_{1}.mp4",
                                        GetGalleryPath(),
                                        System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

            return filename;
        }


        public static bool ComputeVideoThumbnailPath(string videoShotPath, out string videoThumnailPath)
        {
            bool ret = false;
            videoThumnailPath = "";
            string strThumbnailName = "NotFound";

            int index = videoShotPath.IndexOf("video_2");

            if (index > 0)
            {
                strThumbnailName = videoShotPath.Substring(index, videoShotPath.Length - index - 4); // - 4 is to remove .mp4
                strThumbnailName = strThumbnailName + m_keywordForVideoThumbnailName + ".jpg";
                strThumbnailName = GetGalleryPath() + "/" + strThumbnailName;
                videoThumnailPath = strThumbnailName;
                ret = true;
            }

            return ret;
        }

        public static void DeleteVideoThumbnail(string videoShotPath)
        {
            string videoThumbnailPath;

            bool ret = ComputeVideoThumbnailPath(videoShotPath, out videoThumbnailPath);

            if (ret && videoThumbnailPath.Length != 0 && System.IO.File.Exists(videoThumbnailPath))
                File.Delete(videoThumbnailPath);
        }

        public static string GetThumbnailTitleName(string filePath)
        {
            string result;

            // screen_2021-11-02_18-03-04.jpg -> 
            string temp = Path.GetFileName(filePath);
            string[] stringArray = new string[5];
            stringArray = temp.Split('-');

            int indexScreen = stringArray[0].IndexOf("screen_20");
            int indexVideo = stringArray[0].IndexOf("video_20");

            if (indexScreen < 0 && indexVideo < 0) // the name is not correctly formatted, it's not a lynx screenshots or video -> so display only title name of the file.  
            {
                Debug.LogWarning("MEDIA name is not correctly formatted, it is not a lynx screenshots or video, so display only title name of the file");
                result = Path.GetFileNameWithoutExtension(filePath);
            }
            else
            {
                int index = stringArray[0].IndexOf("_");

                string year = stringArray[0].Substring(index + 1);
                string month = stringArray[1];
                string day = stringArray[2].Substring(0, 2);
                string hour = stringArray[2].Substring(3, 2);
                string min = stringArray[3];

                result = month + "." + day + "." + year + "  " + hour + ":" + min;
            }

            return result;
        }


        public static void RGBA_To_NV21YUV(byte[] yuv420sp, byte[] rgba, int width, int height)
        {
            int yIndex = 0;
            int uvIndex = width * height;//mFrameSize; // attention ici

            int pixelIndex = 0;

            //int a;
            int R, G, B, Y, U, V;

            int index = 0;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {

                    //a = (argb[index] & 0xff000000) >> 24; // a is not used obviously
                    // invert r and b
                    B = rgba[index] & 0xff;
                    G = rgba[index + 1] & 0xff;
                    R = rgba[index + 2] & 0xff;

                    // well known RGB to YUV algorithm
                    Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                    U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                    V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

                    // NV21 has a plane of Y and interleaved planes of VU each sampled by a factor of 2
                    //    meaning for every 4 Y pixels there are 1 V and 1 U.  Note the sampling is every other
                    //    pixel AND every other scanline.
                    yuv420sp[yIndex++] = (byte)((Y < 0) ? 0 : ((Y > 255) ? 255 : Y));

                    if (j % 2 == 0 && pixelIndex % 2 == 0)
                    {
                        yuv420sp[uvIndex++] = (byte)((V < 0) ? 0 : ((V > 255) ? 255 : V));
                        yuv420sp[uvIndex++] = (byte)((U < 0) ? 0 : ((U > 255) ? 255 : U));
                    }

                    pixelIndex++;
                    index += 4; // rgba
                }
            }
        }

        public static void YUVNV21_To_RGB(byte[] data, byte[] rgb24, int imageWidth, int imageHeight, bool dataIs16To240 = false)
        {
            // the bitmap we want to fill with the image      
            int numPixels = imageWidth * imageHeight;

            // Holding variables for the loop calculation
            int R = 0;
            int G = 0;
            int B = 0;

            int index = 0;

            // Get each pixel, one at a time
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    // Get the Y value, stored in the first block of data
                    // The logical "AND 0xff" is needed to deal with the signed issue
                    float Y = (float)(data[y * imageWidth + x] & 0xff);

                    // Get U and V values, stored after Y values, one per 2x2 block
                    // of pixels, interleaved. Prepare them as floats with correct range
                    // ready for calculation later.
                    int xby2 = x / 2;
                    int yby2 = y / 2;

                    // make this V for NV12/420SP
                    float U = (float)(data[numPixels + 2 * xby2 + yby2 * imageWidth] & 0xff) - 128.0f;

                    // make this U for NV12/420SP
                    float V = (float)(data[numPixels + 2 * xby2 + 1 + yby2 * imageWidth] & 0xff) - 128.0f;

                    if (dataIs16To240)
                    {
                        // Correct Y to allow for the fact that it is [16..235] and not [0..255]
                        Y = 1.164f * (Y - 16.0f);

                        // Do the YUV -> RGB conversion
                        // These seem to work, but other variations are quoted
                        // out there.
                        R = (int)(Y + 1.596f * V);
                        G = (int)(Y - 0.813f * V - 0.391f * U);
                        B = (int)(Y + 2.018f * U);
                    }
                    else
                    {
                        // No need to correct Y
                        // These are the coefficients proposed by @AlexCohn
                        // for [0..255], as per the wikipedia page referenced
                        // above
                        R = (int)(Y + 1.370705f * V);
                        G = (int)(Y - 0.698001f * V - 0.337633f * U);
                        B = (int)(Y + 1.732446f * U);
                    }

                    // Clip rgb values to 0-255
                    R = R < 0 ? 0 : R > 255 ? 255 : R;
                    G = G < 0 ? 0 : G > 255 ? 255 : G;
                    B = B < 0 ? 0 : B > 255 ? 255 : B;

                    // Put that pixel to the bitmap
                    //bitmap.SetPixel(x, y, Color.FromArgb(R, G, B));
                    rgb24[index++] = (byte)R;
                    rgb24[index++] = (byte)G;
                    rgb24[index++] = (byte)B;

                }
            }
        }


        public static Texture2D FlipTexture2DVertically(Texture2D originalTexture)
        {
            // Clone the original texture to prevent modifying it directly
            Texture2D flippedTexture = new Texture2D(originalTexture.width, originalTexture.height);

            for (int y = 0; y < originalTexture.height; y++)
            {
                for (int x = 0; x < originalTexture.width; x++)
                {
                    // Get the pixel from the original texture and copy it to the flipped texture
                    Color pixelColor = originalTexture.GetPixel(x, y);
                    flippedTexture.SetPixel(x, originalTexture.height - 1 - y, pixelColor);
                }
            }

            // Apply changes to the flipped texture
            flippedTexture.Apply();

            return flippedTexture;
        }

        private static void FlipRenderTextureVertically(RenderTexture target)
        {
            var temp = RenderTexture.GetTemporary(target.descriptor);
            Graphics.Blit(target, temp, new Vector2(1, -1), new Vector2(0, 1));
            Graphics.Blit(temp, target);
            RenderTexture.ReleaseTemporary(temp);
        }

    }// end class ScreenshotAndVideoUtilities 
}// end namespace Lynx