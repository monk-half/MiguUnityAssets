
namespace UGlue.Kit{

    using UnityEngine;
    using ZXing;
    using ZXing.QrCode;

    public class BarCode {
        private const int m_iTextureSize = 256;
        private static int m_iBlankLines = 5;//留白行数
        public static int BlankLines { get => m_iBlankLines; set => m_iBlankLines = value; }

        /// <summary>
        /// get a texture by string transfered
        /// </summary>
        /// <param name="textForEncoding"></param>
        /// <returns></returns>
        public static Texture2D GetTexture(string strContentIn) {
            Texture2D encoded = new Texture2D(m_iTextureSize, m_iTextureSize);

            if (strContentIn != null) {
                var color32 = Encode(strContentIn, encoded.width, encoded.height);
                int offset = CalcBlankLine(color32);
                encoded.SetPixels32(color32);
                encoded.Apply();

                int newSize = m_iTextureSize - 2 * offset + 2 * m_iBlankLines;
                if (newSize > 0) {
                    Log.D("BarCode texture new size : " + newSize);
                    Texture2D newEncoded = new Texture2D(newSize, newSize);
                    newEncoded.SetPixels(encoded.GetPixels(offset - m_iBlankLines, offset - m_iBlankLines, newSize, newSize));
                    newEncoded.Apply();
                    return newEncoded;
                }
            }

            return encoded;
        }

        /// <summary>
        /// Encode string data as Color array
        /// </summary>
        /// <param name="textForEncoding"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Color32[] Encode(string textForEncoding, int width, int height) {
            var writer = new BarcodeWriter {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions {
                    CharacterSet = "UTF-8",
                    Height = height,
                    Width = width
                }
            };
            return writer.Write(textForEncoding);
        }

        /// <summary>
        /// calculate the number of blank line. 
        /// Since the picture is center-symmetrical, only the line blank should be detected from top to bottom.
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private static int CalcBlankLine(Color32[] raw) {
            int i = 0;
            for (Color32 colWhite = new Color32(255, 255, 255, 255); i < raw.Length; i++) {
                if (!raw[i].Equals(colWhite)) {
                    break;
                }
            }
            return i / m_iTextureSize;
        }
    }
}

