using System;
using System.IO;
using System.IO.Compression;
using BitMiracle;
using BitMiracle.LibJpeg;

namespace JpegToRaw
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Load("/home/paulm/RiderProjects/ImgCompression/JpegToRaw/Test/");
            Load("/home/paulm/RiderProjects/ImgCompression/JpegToRaw/Output/");

        }
        
        public static void Load2(string fileName) 
        {
            for (int i = 1; i < 2; i++)
            {
                MemoryStream stream = new MemoryStream();
                stream.Write(File.ReadAllBytes($"/home/paulm/RiderProjects/ImgCompression/JpegToRaw/Test/Output/{i.ToString().PadLeft(3, '0')}.raw"));
                // using (var tst = new JpegImage())
                // Bitmap bm = new Bitmap(stream);
                // bm.Save("C:\example.jpg", System.Drawing.Imaging.ImageFormat.JPEG);


                using(Stream BitmapStream = System.IO.File.Open(fileName+i.ToString().PadLeft(3, '0')+".jpg",System.IO.FileMode.Open ))
                {
                    using (FileStream fs =
                           new FileStream($"/home/paulm/RiderProjects/ImgCompression/JpegToRaw/Test/Output/{i.ToString().PadLeft(3, '0')}.raw",
                               FileMode.Create))
                    {
                        using (JpegImage x = new JpegImage(BitmapStream))
                        {
                            for (int j = 0; j < 480; j++)
                            {
                                var cRow = x.GetRow(j);
                                for (int k = 0; k < 640; k++)
                                {
                                    var cSample = cRow.GetAt(k);
                                    //Y = 0.299 R + 0.587 G + 0.114 B
                                    var luma = (byte)Math.Round(cSample.GetComponent(0) * 0.299 + cSample.GetComponent(1) * 0.587 + cSample.GetComponent(2) * 0.114, 0);
                                    fs.WriteByte(luma);
                                }
                            }
                        } 
                    }
                   
                }
            }
            
        }
        
        public static void Load(string fileName) 
        {
            for (int i = 1; i < 448; i++)
            {
                using(Stream BitmapStream = System.IO.File.Open(fileName+i.ToString().PadLeft(3, '0')+".jpg",System.IO.FileMode.Open ))
                {
                    using (FileStream fs =
                           new FileStream($"/home/paulm/RiderProjects/ImgCompression/JpegToRaw/Test/Output/{i.ToString().PadLeft(3, '0')}.raw",
                               FileMode.Create))
                    {
                        using (JpegImage x = new JpegImage(BitmapStream))
                        {
                            for (int j = 0; j < 480; j++)
                            {
                                var cRow = x.GetRow(j);
                                for (int k = 0; k < 640; k++)
                                {
                                    var cSample = cRow.GetAt(k);
                                    //Y = 0.299 R + 0.587 G + 0.114 B
                                    var luma = (byte)Math.Round(cSample.GetComponent(0) * 0.299 + cSample.GetComponent(1) * 0.587 + cSample.GetComponent(2) * 0.114, 0);
                                    fs.WriteByte(luma);
                                }
                            }
                        } 
                    }
                   
                }
            }
            
        }
    }
    

}