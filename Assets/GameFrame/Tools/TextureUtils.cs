using System;
using System.Collections.Generic;
using System.IO;
using GameFrameDebuger;
using UnityEngine;
using ZXing;
using ZXing.Common;

namespace GameFrame
{
    /// <summary>
    /// 二维码工具类
    /// </summary>
    public class QRCodeUtils
    {
        /// <summary>
        /// 生成二维码图片
        /// </summary>
        /// <param name="content"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Texture2D EncodeToImage(string content,int width,int height)
        {
            Texture2D texture2D = null;
            BitMatrix bitMatrix = null;
            try
            {
                MultiFormatWriter multiFormatWriter = new MultiFormatWriter();
                Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
                hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
                hints.Add(EncodeHintType.MARGIN, 1);
                hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.M);
                bitMatrix = multiFormatWriter.encode(content, BarcodeFormat.QR_CODE, width, height,hints);
            }
            catch (Exception e)
            {
                Debuger.LogError("生成二维码图片失败 "+e.Message);
                return null;
            }
            texture2D = new Texture2D(width,height);
            for (int i = 0; i < bitMatrix.Height; i++)
            {
                for (int j = 0; j < bitMatrix.Width; j++)
                {
                    int px = i;
                    int py = j;
                    if (bitMatrix[i, j])
                    {
                        texture2D.SetPixel(px, py, Color.black);
                    }
                    else
                    {
                        texture2D.SetPixel(px, py, Color.white);
                    }
                }
            }
            texture2D.Apply();
            return texture2D;
        }
        
        /// <summary>
        /// 生成二维码图片带ICON
        /// </summary>
        /// <param name="content"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static Texture2D EncodeToImage(string content,int width,int height,Texture2D icon)
        {
            Texture2D texture2D = null;
            BitMatrix bitMatrix = null;
            try
            {
                MultiFormatWriter multiFormatWriter = new MultiFormatWriter();
                Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
                hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
                hints.Add(EncodeHintType.MARGIN, 1);
                hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
                bitMatrix = multiFormatWriter.encode(content, BarcodeFormat.QR_CODE, width, height,hints);
            }
            catch (Exception e)
            {
                Debuger.LogError("生成二维码图片失败 "+e.Message);
                return null;
            }
            int w = bitMatrix.Width;
            int h = bitMatrix.Height;
            texture2D = new Texture2D(width,height);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int px = i;
                    int py = j;
                    if (bitMatrix[i, j])
                    {
                        texture2D.SetPixel(px, py, Color.black);
                    }
                    else
                    {
                        texture2D.SetPixel(px, py, Color.white);
                    }
                }
            }
            // add icon
            int halfWidth = texture2D.width / 2;
            int halfHeight = texture2D.height / 2;
            int halfWidthOfIcon = icon.width / 2;
            int halfHeightOfIcon = icon.height / 2;
            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    var centerOffsetX = x - halfWidth;
                    var centerOffsetY = y - halfHeight;
                    if (Mathf.Abs(centerOffsetX) <= halfWidthOfIcon && Mathf.Abs(centerOffsetY) <= halfHeightOfIcon)
                    {
                        texture2D.SetPixel(x, y, icon.GetPixel(centerOffsetX + halfWidthOfIcon, centerOffsetY + halfHeightOfIcon));
                    }
                }
            }

            texture2D.Apply();
            return texture2D;
        }
        
        /// <summary>
        /// 添加二维码图片背景
        /// </summary>
        /// <param name="tex_base"></param>
        /// <param name="tex_code"></param>
        /// <returns></returns>
        public static Texture2D AddImagAndQRCode(Texture2D tex_base, Texture2D tex_code)
        {
            Texture2D newTexture = UnityEngine.Object.Instantiate(tex_base) as Texture2D; ;
            Vector2 uv = new Vector2((tex_base.width - tex_code.width) / tex_base.width, (tex_base.height - tex_code.height) / tex_base.height);
            for (int i = 0; i < tex_code.width; i++)
            {
                for (int j = 0; j < tex_code.height; j++)
                {
                    float w = uv.x * tex_base.width - tex_code.width + i;
                    float h = uv.y * tex_base.height - tex_code.height + j;
                    Color baseColor = newTexture.GetPixel((int)w, (int)h);
                    Color codeColor = tex_code.GetPixel(i, j);
                    newTexture.SetPixel((int)w, (int)h, codeColor);
                }
            }
            newTexture.Apply();
            return newTexture;
        }

        public static string DecodeFromImage(Texture2D image)
        {
            try
            {
                Color32LuminanceSource src = new Color32LuminanceSource(image.GetPixels32(), image.width, image.height);

                Binarizer bin = new GlobalHistogramBinarizer(src);
                BinaryBitmap bmp = new BinaryBitmap(bin);

                MultiFormatReader mfr = new MultiFormatReader();
                Result result = mfr.decode(bmp);

                if (result != null)
                {
                    return result.Text;
                }
                else
                {
                    Debuger.LogError("解码二维码图片出错");
                }
            }
            catch (Exception e)
            {
                Debuger.LogError("解码二维码图片异常",  e.Message);
            }

            return "";
        }
        
        public static void SaveTextureCodeToPng(string fileNameUrl, Texture2D tex)
        {
            byte[] bytes = tex.EncodeToJPG();
            string folderUrl = Path.GetDirectoryName(fileNameUrl);
            if (FileManager.IsDirectoryExist(folderUrl))
            {
                FileManager.CreateDirectory(folderUrl);
            }
            FileManager.WriteBytesToFile(fileNameUrl, bytes,bytes.Length);
        }
    }
    /// <summary>
    /// 屏幕截图的工具
    /// </summary>
    public class CaptureScreenShotTools
    {
        /// <summary>
        /// 普通截图函数 可以截取UI画面
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="rect"></param>
        /// <param name="textureFormat"></param>
        /// <returns></returns>
        public static Texture2D NormalCaptrueScreenShot(string savePath,Rect rect,TextureFormat textureFormat)
        {
            Texture2D screenShot = new Texture2D((int)rect.width,(int)rect.height,textureFormat,false);
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();
            byte[] bytes = screenShot.EncodeToPNG();
            if (FileManager.IsFileExist(savePath))
            {
                FileManager.DeleteFile(savePath);
            }
            FileManager.WriteBytesToFile(savePath,bytes,bytes.Length);
            return screenShot;
        }
        /// <summary>
        /// 使用单个摄像机截取图像
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="camera"></param>
        /// <param name="rect"></param>
        /// <param name="textureFormat"></param>
        /// <returns></returns>
        public static Texture2D CaptureScreenShotWithOneCamera(string savePath,Camera camera,Rect rect,TextureFormat textureFormat)
        {
            RenderTexture renderTexture = new RenderTexture((int)rect.width,(int)rect.height,0);
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;//设置当前render为active
            Texture2D screenShot = new Texture2D((int)rect.width,(int)rect.height,textureFormat,false);
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();
            //清除之前的参数
            camera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(renderTexture);
            byte[] bytes = screenShot.EncodeToPNG();
            if (FileManager.IsFileExist(savePath))
            {
                FileManager.DeleteFile(savePath);
            }
            FileManager.WriteBytesToFile(savePath,bytes,bytes.Length);
            return screenShot;
        }
        /// <summary>
        /// 使用多个摄像机截取图像
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="cameras"></param>
        /// <param name="rect"></param>
        /// <param name="textureFormat"></param>
        /// <returns></returns>
        public static Texture2D CaptureScreenShotWithCameras(string savePath,Camera[] cameras,Rect rect,TextureFormat textureFormat)
        {
            RenderTexture renderTexture = new RenderTexture((int)rect.width,(int)rect.height,0);
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].targetTexture = renderTexture;
                cameras[i].Render();
            }
          
            RenderTexture.active = renderTexture;//设置当前render为active
            Texture2D screenShot = new Texture2D((int)rect.width,(int)rect.height,textureFormat,false);
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();
            //清除之前的参数
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].targetTexture = null;
            }
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(renderTexture);
            byte[] bytes = screenShot.EncodeToPNG();
            if (FileManager.IsFileExist(savePath))
            {
                FileManager.DeleteFile(savePath);
            }
            FileManager.WriteBytesToFile(savePath,bytes,bytes.Length);
            return screenShot;
        }
    }
}