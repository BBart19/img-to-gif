using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0 || args.Length % 2 != 0) 
        {
            Console.WriteLine("Incorrect or no arguments provided. Use --imgX \"path_to_imageX.jpg\" for images and --output \"path_to_output.gif\" for the output path.");
            return;
        }

        try
        {
            
            Dictionary<string, string> arguments = ParseArguments(args);
            string outputFilePath = arguments.ContainsKey("--output") ? arguments["--output"] : "output.gif";

            
            List<string> imageFiles = new List<string>();
            int index = 1;
            while (arguments.ContainsKey($"--img{index}"))
            {
                imageFiles.Add(arguments[$"--img{index}"]);
                index++;
            }

            if (imageFiles.Count == 0)
            {
                Console.WriteLine("No image paths provided.");
                return;
            }

            // Creating a GIF from the first image
            using (Image gifImage = Image.FromFile(imageFiles[0]))
            {
                var frameDimension = new FrameDimension(gifImage.FrameDimensionsList[0]);

                // Setting GIF properties
                Encoder encoder = Encoder.SaveFlag;
                var encoderInfo = GetEncoderInfo("image/gif");
                var encoderParameters = new EncoderParameters(1);

                encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);

                // Initializing GIF creation
                gifImage.Save(outputFilePath, encoderInfo, encoderParameters);

                // Adding additional images as frames
                encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.FrameDimensionTime);

                foreach (var file in imageFiles.Skip(1))
                {
                    using (Image frame = Image.FromFile(file))
                    {
                        gifImage.SaveAdd(frame, encoderParameters);
                    }
                }

                // Finalizing the GIF
                encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.Flush);
                gifImage.SaveAdd(encoderParameters);
            }

            Console.WriteLine($"GIF has been saved as {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>();
        for (int i = 0; i < args.Length; i += 2)
        {
            if (args.Length > i + 1)
            {
                arguments[args[i]] = args[i + 1];
            }
            else
            {
                throw new ArgumentException("Unmatched argument pair");
            }
        }
        return arguments;
    }

    private static ImageCodecInfo GetEncoderInfo(string mimeType)
    {
        return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.MimeType == mimeType);
    }
}
