﻿using System.IO;
using System.Text;
using Newtonsoft.Json;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Services
{
    public class FileService : IFileService
    {
        public T Read<T>(string folderPath, string fileName)
        {
            var path = Path.Combine(folderPath, fileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }

            return default;
        }

        public void Save<T>(string folderPath, string fileName, T content)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            var fileContent = JsonConvert.SerializeObject(content, jsonSerializerSettings);
            File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
        }


        public void Delete(string folderPath, string fileName)
        {
            if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
            {
                File.Delete(Path.Combine(folderPath, fileName));
            }
        }

        public static void WriteAllTextIfNew(string path, string contents)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, contents);
            }
        }

        public void SaveAccessTransformers(string folderPath, string fileName, AccessTransformersModel content)
        {

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string fileContent = "[AccessTransformers]";
            if (content.FriendTransformers != null)
            {
                foreach (var friendTransformer in content.FriendTransformers)
                {
                    fileContent += $"\nFriend=(Class=\"{friendTransformer.Class}\", FriendClass=\"{friendTransformer.FriendClass}\")";
                }
            }
            if (content.AccessorTransformers != null)
            {
                foreach (var accessorTransformer in content.AccessorTransformers)
                {
                    fileContent += $"\nAccessor=(Class=\"{accessorTransformer.Class}\", Property=\"{accessorTransformer.Property}\")";
                }
            }
            if (content.BlueprintReadWriteTransformers != null)
            {
                foreach (var blueprintReadWriteTransformer in content.BlueprintReadWriteTransformers)
                {
                    fileContent += $"\nBlueprintReadWrite=(Class=\"{blueprintReadWriteTransformer.Class}\", Property=\"{blueprintReadWriteTransformer.Property}\")";
                }
            }

            File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
        }
    }
}
