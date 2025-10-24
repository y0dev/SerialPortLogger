using System;
using System.IO;
using SerialLogAnalyzer.Services;
using SerialLogAnalyzer.Models;

namespace SerialLogAnalyzer.Test
{
    class ConfigTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Sample Configurations...");
            
            string scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Scripts");
            string[] configFiles = {
                "sample_config_basic.xml",
                "sample_config_advanced.xml", 
                "sample_config_minimal.xml",
                "sample_config_tftp.xml"
            };
            
            foreach (var configFile in configFiles)
            {
                string filePath = Path.Combine(scriptsPath, configFile);
                if (File.Exists(filePath))
                {
                    try
                    {
                        var service = new ConfigurationService(filePath);
                        var config = service.LoadConfiguration();
                        
                        Console.WriteLine($"\n✓ {configFile} loaded successfully:");
                        Console.WriteLine($"  - Theme: {config?.Settings?.Theme ?? "None"}");
                        Console.WriteLine($"  - Font: {config?.Settings?.Font ?? "None"}");
                        Console.WriteLine($"  - Font Size: {config?.Settings?.FontSize ?? 0}");
                        Console.WriteLine($"  - Items: {config?.Items?.Count ?? 0}");
                        Console.WriteLine($"  - Computer Configs: {config?.ComputerConfigs?.Count ?? 0}");
                        Console.WriteLine($"  - Recent Activities: {config?.RecentActivity?.Activities?.Count ?? 0}");
                        
                        if (config?.ComputerConfigs != null)
                        {
                            foreach (var computer in config.ComputerConfigs)
                            {
                                Console.WriteLine($"    - {computer.Name}: {computer.SerialConsoleConfigs?.Count ?? 0} serial configs");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ {configFile} failed to load: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"✗ {configFile} not found");
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
