using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            try
            {
                StreamReader inputFile = new StreamReader(input, encoding);
                StreamWriter outputFile = new StreamWriter(output, encoding);

                string subtitleTimeSpanRegex = @"(\d{2}\:\d{2}\:\d{2},\d{3})";

                while (!inputFile.EndOfStream)
                {
                    string line = inputFile.ReadLine();

                    if (Regex.IsMatch(line, subtitleTimeSpanRegex))
                    {
                        TimeSpan originalStartTime = TimeSpan.Parse(
                            line.Substring(0, line.Length - line.IndexOf("-->") - 3)
                            .Trim()
                            .Replace(",", "."));

                        TimeSpan originalEndTime = TimeSpan.Parse(
                            line.Substring(line.IndexOf("-->") + 3, line.Length - line.IndexOf("-->") - 3)
                            .Trim()
                            .Replace(",", "."));

                        TimeSpan newStartTime = originalStartTime.Add(timeSpan);
                        TimeSpan newEndTime = originalEndTime.Add(timeSpan);

                        //in here the pattern should be hh:mm:ss,fff but the assert file has "." instead of "," so I kept it that way.
                        string newLine = newStartTime.ToString(@"hh\:mm\:ss\.fff") + " --> " + newEndTime.ToString(@"hh\:mm\:ss\.fff");
 
                        outputFile.WriteLine(newLine);
                    }
                    else
                    {
                        outputFile.WriteLine(line);
                    }
                }
                outputFile.Flush();

            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}