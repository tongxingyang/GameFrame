using System;
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
                bitMatrix = multiFormatWriter.encode(content, BarcodeFormat.QR_CODE, width, height);
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
    }
}