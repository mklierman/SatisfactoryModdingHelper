using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Contracts.Services
{
    public interface IFileService
    {
        T Read<T>(string folderPath, string fileName);

        void Save<T>(string folderPath, string fileName, T content);

        void SaveAccessTransformers(string folderPath, string fileName, AccessTransformersModel content);

        void Delete(string folderPath, string fileName);
    }
}
