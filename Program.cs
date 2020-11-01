using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ImgBrick
{
    class Program
    {
        //version: major_version.minor_version.build
        public static readonly string version = "0.0.1a";
        public static readonly List<string> imageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".PNG", ".GIF", ".TIFF" };
        static void Main(string[] args)
        {
            CheckArguments(args);
            ProcessImage(args[0]);
            //Image toBrick = Image.FromFile(args[0]);       
            Console.WriteLine("Finished");
        }
        private static void ProcessImage(string path){
            
        }
        private static void CheckArguments(String[] args){
            //check if enough arguments are given
            if(args.Length < 1){
                Help();
                Environment.Exit(0);
            }

            if(args[0] == "-h" || args[0] == "--help"){
                Help();
                Environment.Exit(0);
            }

            //check if selected file exists
            if(!File.Exists(args[0])){
                Console.Error.WriteLine("This path does not exist.");
                PrintUsage();
                Environment.Exit(0);
            }

            //check if selected file is supported
            if (!imageExtensions.Contains(Path.GetExtension(args[0]).ToUpperInvariant()))
            {
                Console.Error.WriteLine("This filetype is not supported.");
                PrintSupportedFiletypes();
                Environment.Exit(0);
            }
        }

        static void Help(){
            Console.WriteLine("ImgBrick v" + version + "\n");
            Console.WriteLine("ImgBrick converts normal images into low-res version reduced to a given color palette.");
            Console.WriteLine("This can be used to easily plan a LEGO-picture (thus the name \"ImgBrick\").\n");
            PrintUsage();
            PrintSupportedFiletypes();
        }

        static void PrintUsage(){
            Console.WriteLine("Usage: imgbrick [path-to-source] [path-to-color_palette] [target-resolution-vertical]");   
        }

        static void PrintSupportedFiletypes(){
            Console.Error.Write("Supported filetypes:  ");
            foreach (var element in imageExtensions)
            {
                Console.Error.WriteLine(element);
                Console.Error.Write("                      ");    
            }
            Console.Error.WriteLine();
        }
    }
}
