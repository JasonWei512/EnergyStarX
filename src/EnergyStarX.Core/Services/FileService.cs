using EnergyStarX.Core.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace EnergyStarX.Core.Services;

public class FileService : IFileService
{
    public T Read<T>(string folderPath, string fileName)
    {
        string path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        return default;
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileContent = JsonSerializer.Serialize(content);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }
}
