using System;
using SkiaSharp;
using System.IO;
using System.Text;

namespace BMP
{
    public class BMP
    {
        public string ArbHeader { get; set; }
        public int[,] matrix { get {
                return _matrix;
            } set
            {
                _matrix = value;
                _W = value.GetLength(0);
                _H = value.GetLength(1);
            }
        }
        public int W { get { return _W; } } public int H { get { return _H; } }

        int _W, _H;
        int[,] _matrix;
        MemoryStream Contents;
        public bool built = false;

        public void Save(string filename)
        {
            File.WriteAllBytes(filename, GetContents());
        }

        public byte[] GetPicData()
        {
            BuildBMP();
            Contents.Position = 54;
            var toRead = Contents.Length - Contents.Position;
            byte[] arr = new byte[toRead];
            Contents.Read(arr, 0, (int)toRead);
            return arr;
        }

        public void SetPicData(byte[] data, int width, int height)
        {
            matrix = new int[width, height];
            MemoryStream ms = new MemoryStream(data);
            for (int y = height - 1; y >= 0; y--)
            {
                int paddingLen = W % 4;
                byte[] toRead = new byte[width * 3 + paddingLen];
                ms.Read(toRead, 0, toRead.Length);
                for (int i = 0; i<width; i++)
                {
                    byte B = toRead[i * 3];
                    byte G = toRead[i * 3 + 1];
                    byte R = toRead[i * 3 + 2];
                    matrix[i, y] = R * 0x10000 + G * 0x100 + B;
                }
            }
        }

        public byte[] GetContents()
        {
            BuildBMP();
            Contents.Position = 0;
            byte[] arr = Contents.ToArray();
            return arr;
        }

        private void BuildBMP()
        {
            Contents = new MemoryStream();
            Contents.Position = 0;

            if (matrix == null || matrix.GetLength(0) < 1 || matrix.GetLength(1) < 1) throw new ArgumentException("BMP contents are empty. Nothing to build");

            Baker.BMHeader(Contents);
            int pixelAmount = W * H;
            int fileSize = pixelAmount * 3 + H * (W % 4) + 54;
            Baker.FileSizeHeader(Contents, fileSize);
            Baker.ArbitraryHeader(Contents, ArbHeader == null ? new byte[] { 0x00, 0x00, 0x00, 0x00 } : Encoding.UTF8.GetBytes(ArbHeader));
            Baker.PixelOffsetHeader(Contents);
            Baker.DIB(Contents, W, H);

            int paddingLen = W % 4;
            for (int y = H - 1; y >= 0; y--)
            {
                for (int x = 0; x < W; x++)
                {
                    int RGB = matrix[x, y];
                    byte R = (byte)((RGB >> 16) & 0xff);
                    byte G = (byte)((RGB >> 8) & 0xff);
                    byte B = (byte)((RGB >> 0) & 0xff);
                    Contents.Write(new byte[] { B, G, R }, 0, 3);
                }
                Contents.Write(new byte[paddingLen], 0, paddingLen);
            }

            built = true;
        }

        /// <summary>
        /// Loads a BMP class from a System.Drawing.Bitmap
        /// </summary>
        /// <param name="bmp">Bitmap to load BMP from</param>
        public void Load(SKBitmap bmp)
        {
            int w = bmp.Width; int h = bmp.Height;

            int[,] newMatrix = new int[w, h];

            for (int x = 0; x<w; x++)
            {
                for (int y = 0; y<h; y++)
                {
                    SKColor c = bmp.GetPixel(x, y);
                    newMatrix[x, y] = c.Red * 0x10000 + c.Green * 0x100 + c.Blue;
                }
            }

            matrix = newMatrix;
        }

        /// <summary>
        /// Loads a BMP class from filename
        /// </summary>
        /// <param name="filename">Filename of a file to load BMP from</param>
        public void Load(string filename)
        {
            Load(SKBitmap.Decode(filename));
        }

        #region Initializers
        /// <summary>
        /// Initializes a BMP class.
        /// </summary>
        public BMP()
        {
            
        }

        /// <summary>
        /// Initializes a BMP class from filename
        /// </summary>
        /// <param name="filename">Filename of a file to load BMP from</param>
        public BMP(string filename)
        {
            Load(filename);
        }

        /// <summary>
        /// Initializes a BMP class from a System.Drawing.Bitmap
        /// </summary>
        /// <param name="bmp">Bitmap to load BMP from</param>
        public BMP(SKBitmap bmp)
        {
            Load(bmp);
        }
        #endregion
    }

    /// <summary>
    /// A class managing 24-bit BMP building
    /// </summary>
    internal static class Baker
    {
        #region Static data (Headers)
        private readonly static byte[] IDField = { 0x42, 0x4D }; //"BM". BMP magic.
        private readonly static byte[] DataOffset = { 0x36, 0x00, 0x00, 0x00 }; //54. Shows the location of pixel data.
        private readonly static byte[] DIBNumber = { 0x28, 0x00, 0x00, 0x00 }; //40. DIB header size in bytes.
        private readonly static byte[] DIB_MagicBytes_1 = {
            0x01, 0x00,//plane amount.
            0x18, 0x00,//Bits per pixel.
            0x00, 0x00, 0x00, 0x00 //zero compression
        };
        private static readonly byte[] DIB_MagicBytes_2 =
        {
            0x13, 0x0B, 0x00, 0x00, //Printing details =======
            0x13, 0x0B, 0x00, 0x00, //========================
            0x00, 0x00, 0x00, 0x00, //colors amount
            0x00, 0x00, 0x00, 0x00 // all colors are important
        };
        #endregion

        /// <summary>
        /// Writes a "BM" Header to the Stream.
        /// </summary>
        /// <param name="s">Stream to write the header to</param>
        public static void BMHeader(Stream s)
        {
            s.Write(IDField, 0, IDField.Length);
        }
        /// <summary>
        /// Adds BMP size header to the file.
        /// </summary>
        /// <param name="s">Stream to write the header to</param>
        /// <param name="BMPSize">Size of a BMP file</param>
        public static void FileSizeHeader(Stream s, int BMPSize)
        {
            byte[] Bytes = BitConverter.GetBytes(BMPSize);
            s.Write(Bytes, 0, Bytes.Length);
        }
        /// <summary>
        /// Adds program-specific unused value to the file. (Can be anything)
        /// </summary>
        /// <param name="s">Stream to write the header to</param>
        /// <param name="header">String header representation</param>
        public static void ArbitraryHeader(Stream s, string header)
        {
            ArbitraryHeader(s, Encoding.UTF8.GetBytes(header));
        }

        /// <summary>
        /// Adds program-specific unused value to the file. (Can be anything)
        /// </summary>
        /// <param name="s">Stream to write the header to</param>
        /// <param name="header">Byte array to be added</param>
        public static void ArbitraryHeader(Stream s, byte[] header)
        {
            if (header.Length == 4)
            {
                s.Write(header, 0, 4);
            }
            else
            {
                throw new ArgumentException("Header length should be 4 bytes.");
            }
        }
        /// <summary>
        /// Writes offset to pixel array to the file.
        /// </summary>
        /// <param name="s">Stream to write pixel offset to</param>
        public static void PixelOffsetHeader(Stream s)
        {
            s.Write(DataOffset, 0, DataOffset.Length);
        }
        /// <summary>
        /// Manages DIB header creation
        /// </summary>
        /// <param name="s">Stream to write DIB header to</param>
        /// <param name="w">Width of the BMP</param>
        /// <param name="h">Height of the BMP</param>
        public static void DIB(Stream s, int w, int h)
        {
            s.Write(DIBNumber, 0, DIBNumber.Length);
            byte[] wBytes = BitConverter.GetBytes(w);
            byte[] hBytes = BitConverter.GetBytes(h);
            s.Write(wBytes, 0, wBytes.Length);
            s.Write(hBytes, 0, hBytes.Length);
            s.Write(DIB_MagicBytes_1, 0, DIB_MagicBytes_1.Length);
            int padding = w % 4;
            byte[] RawBitmapSize = BitConverter.GetBytes((w * 3 + padding) * h);
            s.Write(RawBitmapSize, 0, RawBitmapSize.Length);
            s.Write(DIB_MagicBytes_2, 0, DIB_MagicBytes_2.Length);
        }
    }
}
