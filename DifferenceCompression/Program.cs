using System;
using System.IO;
using System.Linq;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DifferenceCompression
{

    public class RGBlock
    {
        public byte[] R = new byte[16];
        public byte[] G = new byte[16];
        public byte[] B = new byte[16];

        public RGBlock(byte[] input, int offset)
        {
            for (int i = 0; i < 4; i++)
            {
                R[i] = input[i * 3 + 0 + offset];
                G[i] = input[i * 3 + 1 + offset];
                B[i] = input[i * 3 + 2 + offset];
                // Console.WriteLine($"R{i} - {i * 3 + 0 + offset} G{i} - {i * 3 + 1 + offset} B{i} - {i * 3 + 2 + offset} ");
            }
            
            for (int i = 0; i < 4; i++)
            {
                R[i+4] = input[i * 3 + 640 + 0 + offset];
                G[i+4] = input[i * 3 + 640 + 1 + offset];
                B[i+4] = input[i * 3 + 640 + 2 + offset];
                // Console.WriteLine($"R{i+4} - {i * 3 + 640 + 0 + offset} G{i+4} - {i * 3 + 1 + 640 + 0 + offset} B{i+4} - {i * 3 + 2 + 640 + 0 + offset} ");
            }
            
            for (int i = 0; i < 4; i++)
            {
                R[i+8] = input[i * 3 + 640 * 2 + 0 + offset];
                G[i+8] = input[i * 3 + 640 * 2 + 1 + offset];
                B[i+8] = input[i * 3 + 640 * 2 + 2 + offset];
                // Console.WriteLine($"R{i+8} - {i * 3 + 640 * 2 + 0 + offset} G{i+8} - {i * 3 + 1 + 640 * 2 + 0 + offset} B{i+8} - {i * 3 + 2 + 640 * 2 + 0 + offset} ");
            }
            
            for (int i = 0; i < 4; i++)
            {
                R[i+12] = input[i * 3 + 640 * 3 + 0 + offset];
                G[i+12] = input[i * 3 + 640 * 3 + 1 + offset];
                B[i+12] = input[i * 3 + 640 * 3 + 2 + offset];
                // Console.WriteLine($"R{i+12} - {i * 3 + 640 * 3 + 0 + offset} G{i+12} - {i * 3 + 1 + 640 * 3 + 0 + offset} B{i+12} - {i * 3 + 2 + 640 * 3 + 0 + offset} ");
            }
            
        }
    }
    
    public class Program
    {
        
        private static string MoviePath = @"/home/paulm/Pictures/test/";
        private static string OutputPath = @"/home/paulm/Pictures/test/REPLACE";
        private static int Treshold = 16;
        

        static void Main(string[] args)
        {
            var header = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame0001.bmp")).Take(54).ToArray();
            var todo = 450;
            
            for (int currentPictureIndex = 1; currentPictureIndex < todo; currentPictureIndex=currentPictureIndex+29)
            {
                var referencePicture = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame{currentPictureIndex.ToString().PadLeft(4, '0')}.bmp")).Skip(54).ToArray();
                var bps = 14400;
                
                using (var fs = new MemoryStream())
                {
                    fs.Write(header);
                    fs.Write(referencePicture);
                    fs.Seek(0, SeekOrigin.Begin);
                    using (Image image = Image.Load(fs))
                    {
                        int width = image.Width * 2;
                        int height = image.Height * 2;
                        image.Mutate(x => x.Resize(width, height));
                        image.Save(Path.Combine(OutputPath, $"xFrame{currentPictureIndex.ToString().PadLeft(4, '0')}.bmp"));
                    }
                }
                
                
                // File.Delete(Path.Combine(OutputPath, $"xFrame{currentPictureIndex.ToString().PadLeft(4, '0')}_tmp.bmp"));


                //todo check channels differently
                for (int iFrameIndex = 1; iFrameIndex < 30; iFrameIndex++)
                {
                    var iFrame = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}.bmp")).Skip(54).ToArray();
                    var iFrame2 = File.ReadAllBytes(Path.Combine(MoviePath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}.bmp")).Skip(54).ToArray();
                    
                    var counter = 14400;
                    var bitmap = new byte[14400 / 8];
                    for (int i = 0; i < bitmap.Length; i++)
                    {
                        bitmap[i] = 0;
                    }
                    var currentBlock = 0;


                    // var elCount = 0;
                    // var elValue = 1;
                    // var elDiff = 0;
                    
                    for (int height = 0; height < 90; height++)
                    {
                        for (int width = 0; width < 160; width++)
                        {
                            var position = height * 640 * 4 * 3 + width * 4 * 3;
                            // Console.WriteLine(position);
                            var originalBlock = new RGBlock(referencePicture, position);
                            // Console.WriteLine("---");
                            var iFrameBlock = new RGBlock(iFrame, position);
                            // Thread.Sleep(500);
                            
                            
                            
                            
                            var diffR = 0;
                            var diffG = 0;
                            var diffB = 0;
                            var shouldCopy = true;
                            if (GetDiff(originalBlock, iFrameBlock) > 32)
                            {
                                bps++;

                                byte val = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position] = val;
                                    iFrame2[i*3 + 1 + position] = val;
                                    iFrame2[i*3 + 2 + position] = val;
                                }
                                
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position + 640 * 1] = val;
                                    iFrame2[i*3 + 1 + position + 640 * 1] = val;
                                    iFrame2[i*3 + 2 + position + 640 * 1] = val;
                                }
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position + 640 * 2] = val;
                                    iFrame2[i*3 + 1 + position + 640 * 2] = val;
                                    iFrame2[i*3 + 2 + position + 640 * 2] = val;
                                }
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position + 640 * 3] = val;
                                    iFrame2[i*3 + 1 + position + 640 * 3] = val;
                                    iFrame2[i*3 + 2 + position + 640 * 3] = val;
                                }
                                
                                // for (int i = 0; i < 4; i++)
                                // {
                                //     iFrame2[i*3 + 0 + position] = 255;
                                //     iFrame2[i*3 + 1 + position] = 255;
                                //     iFrame2[i*3 + 2 + position] = 255;
                                // }
                                //
                                // for (int i = 0; i < 4; i++)
                                // {
                                //     iFrame2[i*3 + 0 + position + 640 * 1] = 255;
                                //     iFrame2[i*3 + 1 + position + 640 * 1] = 255;
                                //     iFrame2[i*3 + 2 + position + 640 * 1] = 255;
                                // }
                                // for (int i = 0; i < 4; i++)
                                // {
                                //     iFrame2[i*3 + 0 + position + 640 * 2] = 255;
                                //     iFrame2[i*3 + 1 + position + 640 * 2] = 255;
                                //     iFrame2[i*3 + 2 + position + 640 * 2] = 255;
                                // }
                                // for (int i = 0; i < 4; i++)
                                // {
                                //     iFrame2[i*3 + 0 + position + 640 * 3] = 255;
                                //     iFrame2[i*3 + 1 + position + 640 * 3] = 255;
                                //     iFrame2[i*3 + 2 + position + 640 * 3] = 255;
                                // }
                                
                                
                                // Replace2(ref iFrame2, originalBlock, position + 0, 0);
                                // Replace2(ref iFrame2, originalBlock, position + 3, 1);
                                // Replace2(ref iFrame2, originalBlock, position + 6, 2);
                                // Replace2(ref iFrame2, originalBlock, position + 9, 3);
                                // Replace2(ref iFrame2, originalBlock, position + 0 + 640, 4);
                                // Replace2(ref iFrame2, originalBlock, position + 3 + 640, 5);
                                // Replace2(ref iFrame2, originalBlock, position + 6 + 640, 6);
                                // Replace2(ref iFrame2, originalBlock, position + 9 + 640, 7);
                                // Replace2(ref iFrame2, originalBlock, position + 0 + 640 * 2, 8);
                                // Replace2(ref iFrame2, originalBlock, position + 3 + 640 * 2, 9);
                                // Replace2(ref iFrame2, originalBlock, position + 6 + 640 * 2, 10);
                                // Replace2(ref iFrame2, originalBlock, position + 9 + 640 * 2, 11);
                                // Replace2(ref iFrame2, originalBlock, position + 0 + 640 * 3, 12);
                                // Replace2(ref iFrame2, originalBlock, position + 3 + 640 * 3, 13);
                                // Replace2(ref iFrame2, originalBlock, position + 6 + 640 * 3, 14);
                                // Replace2(ref iFrame2, originalBlock, position + 9 + 640 * 3, 15);
                            }
                            else
                            {
                                byte val = 255;
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position] = val;
                                    iFrame2[i*3 + 1 + position] = val;
                                    iFrame2[i*3 + 2 + position] = val;
                                }
                                
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position + 640 * 1] = val;
                                    iFrame2[i*3 + 1 + position + 640 * 1] = val;
                                    iFrame2[i*3 + 2 + position + 640 * 1] = val;
                                }
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position + 640 * 2] = val;
                                    iFrame2[i*3 + 1 + position + 640 * 2] = val;
                                    iFrame2[i*3 + 2 + position + 640 * 2] = val;
                                }
                                for (int i = 0; i < 4; i++)
                                {
                                    iFrame2[i*3 + 0 + position + 640 * 3] = val;
                                    iFrame2[i*3 + 1 + position + 640 * 3] = val;
                                    iFrame2[i*3 + 2 + position + 640 * 3] = val;
                                }
                                
                                
                                // Replace(ref iFrame2, originalBlock, position + 0, 0);
                                // Replace(ref iFrame2, originalBlock, position + 3, 1);
                                // Replace(ref iFrame2, originalBlock, position + 6, 2);
                                // Replace(ref iFrame2, originalBlock, position + 9, 3);
                                // Replace(ref iFrame2, originalBlock, position + 0 + 640, 4);
                                // Replace(ref iFrame2, originalBlock, position + 3 + 640, 5);
                                // Replace(ref iFrame2, originalBlock, position + 6 + 640, 6);
                                // Replace(ref iFrame2, originalBlock, position + 9 + 640, 7);
                                // Replace(ref iFrame2, originalBlock, position + 0 + 640 * 2, 8);
                                // Replace(ref iFrame2, originalBlock, position + 3 + 640 * 2, 9);
                                // Replace(ref iFrame2, originalBlock, position + 6 + 640 * 2, 10);
                                // Replace(ref iFrame2, originalBlock, position + 9 + 640 * 2, 11);
                                // Replace(ref iFrame2, originalBlock, position + 0 + 640 * 3, 12);
                                // Replace(ref iFrame2, originalBlock, position + 3 + 640 * 3, 13);
                                // Replace(ref iFrame2, originalBlock, position + 6 + 640 * 3, 14);
                                // Replace(ref iFrame2, originalBlock, position + 9 + 640 * 3, 15);
                            }
                            
                            
                            
                            // for (int i = 0; i < 16; i++)
                            // {
                            //     diffR += Math.Abs(originalBlock.R[i] - iFrameBlock.R[i]);
                            //     if (Math.Abs(originalBlock.R[i] - iFrameBlock.R[i]) > 8) shouldCopy = false;
                            // }
                            // Treshold = 32;
                            // if (diffR <= Treshold && shouldCopy)
                            // {
                            //     for (int i = 0; i < 16; i++)
                            //     {
                            //         diffG += Math.Abs(originalBlock.G[i] - iFrameBlock.G[i]);
                            //         if (Math.Abs(originalBlock.G[i] - iFrameBlock.G[i]) > 8) shouldCopy = false;
                            //     }
                            //
                            //     if (diffG <= Treshold && shouldCopy)
                            //     {
                            //         for (int i = 0; i < 16; i++)
                            //         {
                            //             diffB += Math.Abs(originalBlock.B[i] - iFrameBlock.B[i]);
                            //             if (Math.Abs(originalBlock.B[i] - iFrameBlock.B[i]) > 8) shouldCopy = false;
                            //         }
                            //
                            //         if (diffB <= Treshold && shouldCopy)
                            //         {
                            //             ++counter;
                            //             Replace(ref iFrame2, originalBlock, position + 0, 0);
                            //             Replace(ref iFrame2, originalBlock, position + 3, 1);
                            //             Replace(ref iFrame2, originalBlock, position + 6, 2);
                            //             Replace(ref iFrame2, originalBlock, position + 9, 3);
                            //             Replace(ref iFrame2, originalBlock, position + 0 + 640, 4);
                            //             Replace(ref iFrame2, originalBlock, position + 3 + 640, 5);
                            //             Replace(ref iFrame2, originalBlock, position + 6 + 640, 6);
                            //             Replace(ref iFrame2, originalBlock, position + 9 + 640, 7);
                            //             Replace(ref iFrame2, originalBlock, position + 0 + 640 * 2, 8);
                            //             Replace(ref iFrame2, originalBlock, position + 3 + 640 * 2, 9);
                            //             Replace(ref iFrame2, originalBlock, position + 6 + 640 * 2, 10);
                            //             Replace(ref iFrame2, originalBlock, position + 9 + 640 * 2, 11);
                            //             Replace(ref iFrame2, originalBlock, position + 0 + 640 * 3, 12);
                            //             Replace(ref iFrame2, originalBlock, position + 3 + 640 * 3, 13);
                            //             Replace(ref iFrame2, originalBlock, position + 6 + 640 * 3, 14);
                            //             Replace(ref iFrame2, originalBlock, position + 9 + 640 * 3, 15);
                            //             // Console.Write("SAME: "); GetDiff(originalBlock, iFrameBlock);
                            //         }
                            //         else
                            //         {
                            //             Replace2(ref iFrame2, originalBlock, position + 0, 0);
                            //             Replace2(ref iFrame2, originalBlock, position + 3, 1);
                            //             Replace2(ref iFrame2, originalBlock, position + 6, 2);
                            //             Replace2(ref iFrame2, originalBlock, position + 9, 3);
                            //             Replace2(ref iFrame2, originalBlock, position + 0 + 640, 4);
                            //             Replace2(ref iFrame2, originalBlock, position + 3 + 640, 5);
                            //             Replace2(ref iFrame2, originalBlock, position + 6 + 640, 6);
                            //             Replace2(ref iFrame2, originalBlock, position + 9 + 640, 7);
                            //             Replace2(ref iFrame2, originalBlock, position + 0 + 640 * 2, 8);
                            //             Replace2(ref iFrame2, originalBlock, position + 3 + 640 * 2, 9);
                            //             Replace2(ref iFrame2, originalBlock, position + 6 + 640 * 2, 10);
                            //             Replace2(ref iFrame2, originalBlock, position + 9 + 640 * 2, 11);
                            //             Replace2(ref iFrame2, originalBlock, position + 0 + 640 * 3, 12);
                            //             Replace2(ref iFrame2, originalBlock, position + 3 + 640 * 3, 13);
                            //             Replace2(ref iFrame2, originalBlock, position + 6 + 640 * 3, 14);
                            //             Replace2(ref iFrame2, originalBlock, position + 9 + 640 * 3, 15);
                            //             // Console.Write("NOT: "); GetDiff(originalBlock, iFrameBlock);
                            //         }
                            //     }
                            // }
                            //
                            // if (diffR > Treshold || diffG > Treshold || diffB > Treshold)
                            // {
                            //     ++bps;
                            //     
                            //     // if (elValue == 1)
                            //     // {
                            //     //     Console.Write($"[{elCount.ToString().PadLeft(4,'0')}] - 1 | ");
                            //     //     elCount = 1;
                            //     //     elValue = 0;
                            //     //     elDiff++;
                            //     //     // Console.Write("0-");
                            //     // }
                            //     // else
                            //     // {
                            //     //     elCount++;
                            //     // }
                            //     SetBlockStatus(ref bitmap, currentBlock, false);
                            // }
                            // else
                            // {
                            //     // if (elValue == 0)
                            //     // {
                            //     //     Console.Write($"[{elCount.ToString().PadLeft(4,'0')}] - 0 | ");
                            //     //     elCount = 1;
                            //     //     elValue = 1;
                            //     //     elDiff++;
                            //     // }
                            //     // else
                            //     // {
                            //     //     elCount++;
                            //     // }
                            //     SetBlockStatus(ref bitmap, currentBlock, true);
                            // }
                            currentBlock++;
                        }
                    }

                    bps += 225;
                    // Console.WriteLine();
                    // Console.WriteLine(BitConverter.ToString(bitmap));
                    
                    using (var fs = new MemoryStream())
                    {
                        fs.Write(header);
                        fs.Write(iFrame2);
                        fs.Seek(0, SeekOrigin.Begin);
                        using (Image image = Image.Load(fs))
                        {
                            int width = image.Width * 1;
                            int height = image.Height * 1;
                            image.Mutate(x => x.Resize(width, height));
                            image.Save(Path.Combine(OutputPath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}.bmp"));
                        }
                    }
                    
                    // using (var fs = new FileStream(Path.Combine(OutputPath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}_tmp.bmp"), FileMode.Create))
                    // {
                    //     fs.Write(header);
                    //     fs.Write(iFrame2);
                    //     fs.Flush();
                    //     fs.Seek(0, SeekOrigin.Begin);
                    //     using (Image image = Image.Load(fs))
                    //     {
                    //         int width = image.Width * 2;
                    //         int height = image.Height * 2;
                    //         image.Mutate(x => x.Resize(width, height));
                    //         image.Save(Path.Combine(OutputPath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}.bmp"));
                    //     }
                    // }

                    // using (var fs = new FileStream(Path.Combine(OutputPath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}_tmp.bmp"), FileMode.Open))
                    // {
                    //     using (Image image = Image.Load(fs))
                    //     {
                    //         int width = image.Width * 2;
                    //         int height = image.Height * 2;
                    //         image.Mutate(x => x.Resize(width, height));
                    //         image.Save(Path.Combine(OutputPath, $"xFrame{(iFrameIndex+currentPictureIndex).ToString().PadLeft(4, '0')}.bmp"));
                    //     }
                    // }
                    // Console.WriteLine(elDiff);
                    // Console.WriteLine("---------------------");
                    referencePicture = iFrame;
                }
                Console.WriteLine($"{Math.Round(bps / 1024.0 / 1024 * 8, 2).ToString("F" + 2)} MBps");
            }
        }

        private static void SetBlockStatus(ref byte[] bitmap, int position, bool isClone)
        {
            var byteNum = position / 8;
            var bitNum = position % 8;
            if (isClone)
            {
                // SET BIT
                bitmap[byteNum] |= (byte)(1 << bitNum);
                return;
            }
            bitmap[byteNum] &= (byte)~(1 << bitNum);
        }


        static void Replace(ref byte[] input, RGBlock block, int offset, int pos)
        {
            input[offset + 0] = block.R[pos];
            input[offset + 1] = block.G[pos];
            input[offset + 2] = block.B[pos];
        }
        
        static void Replace2(ref byte[] input, RGBlock block, int offset, int pos)
        {
            input[offset + 0] = 255;
            input[offset + 1] = 255;
            input[offset + 2] = 255;
        }

        private static double GetDiff(RGBlock e1, RGBlock e2)
        {
            double diff = 0;
            for (int i = 0; i < 16; i++)
            {
                long rmean = ( e1.R[i] + (long)e2.R[i] ) / 2;
                long r = e1.R[i] - (long)e2.R[i];
                long g = e1.G[i] - (long)e2.G[i];
                long b = e1.B[i] - (long)e2.B[i];
                diff += Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            }
            // Console.WriteLine(diff);
            return diff;
        }

        public static void Test()
        {
            
        }
    }
}