using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Core.Contracts.Services;

public interface IFileService
{
    T Read<T>(string folderPath, string fileName);

    void Save<T>(string folderPath, string fileName, T content);

    void Delete(string folderPath, string fileName);

    void WriteAllTextIfNew(string path, string contents);

    void SaveAccessTransformers(string folderPath, string fileName, AccessTransformersModel content);
}
