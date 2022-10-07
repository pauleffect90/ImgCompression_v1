// #define TEST

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;


namespace VCompress
{
    
    
    class Program
    {
        // private static string MoviePath = @"/home/paulm/Videos/compress/";
        // private static string OutputPath = @"/home/paulm/Videos/compress/compressed/";
        private static string MoviePath = @"/media/paulm/ADATA HD720/Compression/";
        private static string OutputPath = @"/media/paulm/ADATA HD720/Compression/compressed/";
        private static int Treshold = 16;
        private static int WIDTH = 640;
        private static int HEIGHT = 360;
        
        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        static void Main(string[] args)
        {
            
            var header = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame0001.bmp")).Take(54).ToArray();
            var noBlocksDiff = 0;
            double totalSize = 129600;
            double max = 0;
            var referencePicture = File.ReadAllBytes(Path.Combine(MoviePath,$"xFrame0001.bmp")).Skip(54).ToArray();
            var bps = 14400;
            var hit = 0;
                var cnt = 1;
                using (var fs = new FileStream(Path.Combine(OutputPath, $"xFrame0001.bmp"), FileMode.Create))
                {
                    fs.Write(header);
                    fs.Write(referencePicture);
                }

                for (int iFrameIndex = 2; iFrameIndex < 2401; iFrameIndex++)
                {
                    var iFrame = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame{(iFrameIndex).ToString().PadLeft(4, '0')}.bmp")).Skip(54).ToArray();
                    var iFrame2 = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame{(iFrameIndex).ToString().PadLeft(4, '0')}.bmp")).Skip(54).ToArray();

                    for (int height = 0; height < HEIGHT / 4; height++)
                    {
                        for (int width = 1; width < WIDTH / 4; width++)
                        {
                            var offset = width * 4 * 3 + height * WIDTH * 4 * 3;
                            var offset2 = (width - 1) * 4 * 3 + height * WIDTH * 4 * 3;

                            var currentBlock = new byte[48];
                            var referenceBlock = new byte[48];

                            var pos = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    referenceBlock[pos] = referencePicture[offset + i * 3 + 0 + WIDTH * j * 3];
                                    currentBlock[pos++] = iFrame[offset + i * 3 + 0 + WIDTH * j * 3];
                                    referenceBlock[pos] = referencePicture[offset + i * 3 + 1 + WIDTH * j * 3];
                                    currentBlock[pos++] = iFrame[offset + i * 3 + 1 + WIDTH * j * 3];
                                    referenceBlock[pos] = referencePicture[offset + i * 3 + 2 + WIDTH * j * 3];
                                    currentBlock[pos++] = iFrame[offset + i * 3 + 2 + WIDTH * j * 3];
                                }
                            }

                            // max = Math.Max(GetDiff(currentBlock, referenceBlock), max);

                            // SAME BLOCK ~ ASSUME LEFT IS SAME
                            
                            // if (GetDiff(currentBlock, referenceBlock) < 128+64)
                            if (GetDiff3(currentBlock, referenceBlock))
                            // if (GetDiff2(currentBlock, referenceBlock) < 5500)
                            {
                                // max = Math.Max(GetDiff2(referenceBlock, currentBlock), max);

                                ++noBlocksDiff;
                                for (int i = 0; i < 4; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
#if TEST
                                        iFrame2[offset + i * 3 + 0 + WIDTH * j * 3] = 0;
                                        iFrame2[offset + i * 3 + 1 + WIDTH * j * 3] = 0;
                                        iFrame2[offset + i * 3 + 2 + WIDTH * j * 3] = 0;
#else
                                        iFrame2[offset + i * 3 + 0 + WIDTH * j * 3] = referencePicture[offset + i * 3 + 0 + WIDTH * j * 3];
                                        iFrame2[offset + i * 3 + 1 + WIDTH * j * 3] = referencePicture[offset + i * 3 + 1 + WIDTH * j * 3];
                                        iFrame2[offset + i * 3 + 2 + WIDTH * j * 3] = referencePicture[offset + i * 3 + 2 + WIDTH * j * 3];
#endif
                                    }
                                }
                            }
                            else
                            {
                                
                                pos = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        referenceBlock[pos] = referencePicture[offset2 + i * 3 + 0 + WIDTH * j * 3];
                                        currentBlock[pos++] = iFrame[offset2 + i * 3 + 0 + WIDTH * j * 3];
                                        referenceBlock[pos] = referencePicture[offset2 + i * 3 + 1 + WIDTH * j * 3];
                                        currentBlock[pos++] = iFrame[offset2 + i * 3 + 1 + WIDTH * j * 3];
                                        referenceBlock[pos] = referencePicture[offset2 + i * 3 + 2 + WIDTH * j * 3];
                                        currentBlock[pos++] = iFrame[offset2 + i * 3 + 2 + WIDTH * j * 3];
                                    }
                                }
                                
                                
                                // CHECK LEFT BLOCK
                                if (!GetDiff3(currentBlock, referenceBlock))
                                {
                                    ++bps;
                                }
                                else
                                {
                                    ++hit;
                                }
                                ++bps;
                                for (int i = 0; i < 4; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
#if TEST
                                        iFrame2[offset + i * 3 + 0 + WIDTH * j * 3] = 255;
                                        iFrame2[offset + i * 3 + 1 + WIDTH * j * 3] = 255;
                                        iFrame2[offset + i * 3 + 2 + WIDTH * j * 3] = 255;
#endif

                                    }
                                }
                            }
                        }
                    }
                    
                    using (var fs = new FileStream(Path.Combine(OutputPath, $"xFrame{(iFrameIndex).ToString().PadLeft(4, '0')}.bmp"), FileMode.Create))
                    {
                        fs.Write(header);
                        fs.Write(iFrame2);
                    }

                    referencePicture = iFrame;
                    if (cnt++ == 24)
                    {
                        bps += 2700;
                        Console.WriteLine($"{Math.Round(bps / 1024.0 / 1024 * 8, 2).ToString("F" + 2)} MBps - {hit} BLOCKS SAVED");
                        totalSize += bps;
                        cnt = 0;
                        hit = 0;
                        bps = 0;
                    }
                }
                
                Console.WriteLine($"{Math.Round(totalSize / 1024.0 / 1024 * 8, 2).ToString("F" + 2)} MB vs {Math.Round(2400*WIDTH*HEIGHT*3 / 1024.0 / 1024, 2).ToString("F" + 2)} : COMPRESS RATION {Math.Round(2400*WIDTH*HEIGHT*3 / totalSize / 8, 0).ToString("F" + 2)}");
        }
        
        private static double GetDiff(byte[] e1, byte[] e2)
        {
            double diff = 0;
            for (int i = 0; i < 16; i++)
            {
                long rmean = ( e1[i*3] + e2[i*3] ) / 2;
                long r = e1[i*3] - e2[i*3];
                long g = e1[i*3+1] - (long)e2[i*3+1];
                long b = e1[i*3+2] - (long)e2[i*3+2];
                diff += Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            }
            return diff;
        }
        private static double GetDiff2(byte[] e1, byte[] e2)
        {
            double diff = 0;

            for (int i = 0; i < 16; i++)
            {
                long cR = e1[i*3] - e2[i*3];
                long cG = e1[i*3+1] - e2[i*3+1];
                long cB = e1[i*3+2] - (long)e2[i*3+2];
                long uR = e1[i*3] + e2[i*3];
                diff += cR * cR * (2 + uR / 256) + cG * cG * 4 + cB * cB * (2 + (255 - uR) / 256);
            }
            return diff;
        }
        
        
        private static bool GetDiff3(byte[] e1, byte[] e2)
        {
            double diff = 0;
            var cnt = 0;

            for (int i = 0; i < 16; i++)
            {
                long cR = e1[i*3] - e2[i*3];
                long cG = e1[i*3+1] - e2[i*3+1];
                long cB = e1[i*3+2] - (long)e2[i*3+2];
                long uR = e1[i*3] + e2[i*3];
                if (cR * cR * (2 + uR / 256) + cG * cG * 4 + cB * cB * (2 + (255 - uR) / 256) > 1500) ++cnt;
                if (cnt == 3) return false;
            }
            return true;
        }
    }
}