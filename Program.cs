using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImgBrick
{
    class Program
    {
        //version: major_version.minor_version.build
        static readonly string version = "0.1.1a";
        static readonly int defaultRes = 32;
        static readonly List<string> imageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".PNG", ".GIF", ".TIFF" };

        static string path_image;
        static int resX, resY;
        static Image image;
        static void Main(string[] args)
        {
            PrintVersion();
            ReadArguments(args);

            System.Console.WriteLine("Loading image from " + path_image);
            image = Image.FromFile(path_image);

            ProcessImage();

            Console.WriteLine("Conversion succesful!");
            SaveImage(ImageFormat.Png);
        }
        static void SaveImage(ImageFormat f){
            string path_result = Path.GetDirectoryName(path_image);
            path_result += "/";
            path_result += Path.GetFileNameWithoutExtension(path_image);
            path_result += "_BRICKIFIED.";
            path_result += f.ToString().ToLowerInvariant();

            System.Console.WriteLine("Saving new image as " + path_result);

            image.Save(path_result, f);
        }
        private static void ProcessImage(){
            resX = image.Size.Width*resY/image.Size.Height;
            image = ResizeImage(image, new Size(resX, resY));

            System.Console.Write("New Resolution is ");
            System.Console.WriteLine(image.Size.ToString());
        }

        public static Image ResizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private static void ReadArguments(String[] args){
            //check if enough arguments are given
            if(args.Length < 1){
                Console.Error.WriteLine("ERROR: Not enough arguments given.");
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }

            if(args[0] == "-h" || args[0] == "--help" || args[0] == "help"){
                Help();
                Environment.Exit(0);
            }

            //check if selected file exists
            if(!File.Exists(args[0])){
                Console.Error.WriteLine("ERROR: This path does not exist.");
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }

            //check if selected file is supported
            if (!imageExtensions.Contains(Path.GetExtension(args[0]).ToUpperInvariant()))
            {
                Console.Error.WriteLine("ERROR: This filetype is not supported.");
                PrintSupportedFiletypes();
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }

            if(args.Length < 2){
                resY = defaultRes;
                System.Console.WriteLine("No resolution target given, hence falling back to default resolution: " + defaultRes);
                System.Console.WriteLine("'imgbrick -h' for more info.");
            }
            else if(!int.TryParse(args[1], out resY))
            {
                Console.Error.WriteLine("ERROR: NaN");
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }

            path_image = args[0];
        }

        static void Help(){
            Console.WriteLine("ImgBrick converts normal images into low-res version reduced to a given color palette.");
            Console.WriteLine("This can be used to easily plan a LEGO-picture (thus the name \"ImgBrick\").\n\n");
            PrintUsage();
        }

        static void PrintVersion(){
            Console.WriteLine("ImgBrick v" + version);
        }

        static void PrintUsage(){
            Console.WriteLine("Usage: imgbrick [path-to-source] [target-resolution-vertical] [path-to-color_palette]\n");
            PrintSupportedFiletypes();
            System.Console.Write("target-resolution-vertical:  ");
            System.Console.WriteLine("Defines y-resolution.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Aspect ratio is matched automatically.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Fallback to 32 if no value is given.\n");
            System.Console.Write("path-to-color_palette:       ");
            System.Console.WriteLine("Path to the color palette containing the allowed colors for a given image.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Non-allowed colors get changed to the mathematically closest allowed color.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Formatting: [file-name].csv");
            System.Console.Write("                                         ");
            System.Console.WriteLine("first column: '[r], [g], [b]");
            System.Console.Write("                                         ");
            System.Console.WriteLine("one color per row, second column and onward are ignored");
            System.Console.Write("                             ");
            System.Console.WriteLine("Fallback to integrated color palette configured for locally avaliable 1x1 LEGO bricks.");
        }

        static void PrintSupportedFiletypes(){
            Console.Error.Write("Supported source filetypes:  ");
            foreach (var element in imageExtensions)
            {
                Console.Error.WriteLine(element);
                Console.Error.Write("                             ");    
            }
            Console.Error.WriteLine();
        }
    }
}
