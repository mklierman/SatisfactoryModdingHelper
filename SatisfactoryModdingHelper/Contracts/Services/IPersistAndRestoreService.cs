namespace SatisfactoryModdingHelper.Contracts.Services
{
    public interface IPersistAndRestoreService
    {
        void RestoreData();

        void PersistData();

        dynamic GetSavedProperty(string propertyName);

        void SaveProperty(string propertyName, dynamic value);
    }
}
