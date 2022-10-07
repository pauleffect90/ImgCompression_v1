using System;
using System.IO;
using System.Text;

namespace LutCalc
{
    class Program
    {
        
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
                // ((512 + (R1 + R2) / 2) * (R1 - R2) * (R1 - R2)/256
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
                long uR = (e1[i*3] + e2[i*3]) / 256;
                
                diff += cR * cR * (2 + uR / 256) + cG * cG * 4 + cB * cB * (2 + (255 - uR) / 256);
                
            }
            return diff;
        }
        
        static void Main(string[] args)
        {
            var sb = new StringBuilder();

            sb.Append("#include \"stdint.h\"\n uint8_t redLUT[] = {");
            double max = 0;
            double max2 = 0;

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    var l = (512.0 + (i + j) / 2.0) * (i - j) * (i - j) / 256.0;
                    int left = (int)Math.Round(l / 4.0,0);
                    sb.Append(left + ",");
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("};\n");
            
            sb.Append("#include \"stdint.h\"\n uint8_t blueLUT[] = {");
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    var r = (767.0 + (i + j) / 2.0) * (i - j) * (i - j) / 256.0;
                    int right = (int)Math.Round(r / 4.0,0);
                    sb.Append(right + ",");
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("};\n");
            File.WriteAllText("lutsX.c", sb.ToString());
            
            
            Console.WriteLine(max/4);
            Console.WriteLine(max2/4);
            return;

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    long cR = i - j;
                    long uR = i + j;
                    long uR2 = 255 - uR;
                    long cB = i - j;
                    double plm = cR * cR * (2 + uR / 256.0);
                    double plm2 = cB * cB * (2 + uR2 / 256.0);
                    int waw2 = (int)Math.Round(plm2 / 4);
                    int waw = (int)Math.Round(plm / 4);
                    max = Math.Max(max, waw2);
                    sb.Append(waw2);
                    sb.Append(",");
                    // Console.WriteLine($"{i} - {j} : {cR * cR * (2 + uR / 256.0)}");
                    // sb.Append($"{i}, {j}, {2 + uR / 256.0}\n");
                    // Console.WriteLine($"{i} - {j} : {2 + uR / 256.0}");
                }
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append("};\n");
            File.WriteAllText("luts2.c", sb.ToString());

            sb.Clear();
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    long cR = i - j;
                    long uR = i + j;
                    double plm = cR * cR * (2 + uR / 256.0);
                    int waw = (int)plm / 4;
                    sb.Append($"{i}, {j}, {plm}\n");
                }
            }
            Console.WriteLine(max);
            File.WriteAllText("DA.csv", sb.ToString());
        }
    }
}