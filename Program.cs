using System;
using System.IO;

using Microsoft.Build.Construction;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

namespace VSAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("[Usage] VSAnalyzer.exe <slnファイル>");
                return;
            }



            var solutionFile = Path.GetFullPath(args[0]);

            if (!File.Exists(solutionFile))
            {
                Console.WriteLine("Error solution file not found");
                return;
            }

            var solutionDir = Path.GetDirectoryName(solutionFile);
            var solution = SolutionFile.Parse(solutionFile);

            foreach(var project in solution.ProjectsInOrder)
            {
                var projectDir = Path.GetDirectoryName(project.AbsolutePath);
                var outputDir =  "./" + project.ProjectName;

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                if (!File.Exists(project.AbsolutePath))
                {
                    Console.WriteLine("Error project file not found");
                    continue;
                }

                var config = VCppConfig.Parse(project.AbsolutePath);

                foreach (var name in config.get_config_names())
                {
                    var output_file = Path.Combine(outputDir, "compile_flags_" + name.Replace('|', '_') +".txt");
                    using (StreamWriter sw = new StreamWriter(output_file))
                    {
                        foreach (var i in config.get_includes(name).Where(i => !i.StartsWith("%(")))
                        {
                            string f = solve_project_dir(solve_solution_dir(i, solutionDir), projectDir);
                            sw.WriteLine("-I" + f);
                        }
                        foreach (var i in config.get_definitions(name).Where(i => !i.StartsWith("%(")))
                        {
                            sw.WriteLine("-D" + i);
                        }
                    }
                }
            }
        }

        static string solve_solution_dir(string path, string solution_dir)
        {
            return path.Replace("$(SolutionDir)", solution_dir);
        }

        static string solve_project_dir(string path, string project_dir)
        {
            return path.Replace("$(ProjectDir)", project_dir);
        }

    }


}