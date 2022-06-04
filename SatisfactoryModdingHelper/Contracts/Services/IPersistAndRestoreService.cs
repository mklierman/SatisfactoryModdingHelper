using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Contracts.Services
{
    public interface IPersistAndRestoreService
    {
        void RestoreData();

        void PersistData();

        SavedSettings Settings { get; set; }

        //dynamic GetSavedProperty(string propertyName);

        //void SaveProperty(string propertyName, dynamic value);
    }
}
