using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using WifiShare.Properties;

namespace WifiShare.Logic
{
    public class WifiStorage
    {
        public List<WifiEntity> WifiEntities { get; set; } = new();

        private readonly ILogger<WifiStorage> _logger;

        public WifiStorage(ILogger<WifiStorage> logger)
        {
            _logger = logger;
        }

        public void GetNamesAndPasswords()
        {
            // 1. create temporary folder
            var basePath = Path.GetTempPath();
            var temporaryFolder = $"{basePath}\\{Resources.ResourceManager.GetString("TempPath")}";
            try
            {
                Directory.CreateDirectory(temporaryFolder);

                // 2. create XML files of actual wifi saved content (with names ['name'] and passwords ['keyMaterial'])
                var showProfile = $"netsh wlan export profile folder={temporaryFolder} key=clear";
                var cmd = new Process();
                cmd.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.Start();

                cmd.StandardInput.WriteLine(showProfile);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                cmd.WaitForExit();

                // 3. load values from XML files to arrays
                var files = Directory.GetFiles(temporaryFolder, "*.xml", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(file);
                    var name = xmlDocument.GetElementsByTagName("name")[0]?.InnerText;
                    var keyMaterial = xmlDocument.GetElementsByTagName("keyMaterial")[0]?.InnerText;
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(keyMaterial))
                    {
                        WifiEntities.Add(new WifiEntity
                        {
                            Name = name,
                            Password = string.Concat(Enumerable.Repeat("*", keyMaterial.Length)),
                            PasswordHidden = keyMaterial
                        });
                    }
                }

                // 4. delete temporary folder
                Directory.Delete(temporaryFolder, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        // If indices empty, take whole data
        public List<WifiEntityToExport> GetEntities(IEnumerable<WifiEntity>? items)
        {
            if (items is null || !items.Any())
                items = WifiEntities;

            return items
                .Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Password))
                .Select(item => new WifiEntityToExport { Name = item.Name, Password = item.GetPassword() })
                .ToList();
        }

        public string ExportToText(IEnumerable<WifiEntity>? items)
        {
            var entities = GetEntities(items);
            var entitiesDescriptions = entities.Select(x => $"Name={x.Name}, Password={x.Password}");
            return string.Join(Environment.NewLine, entitiesDescriptions);
        }

        public string ExportToJson(IEnumerable<WifiEntity>? items)
        {
            var entities = GetEntities(items);
            return JsonSerializer.Serialize(entities);
        }
    }
}
