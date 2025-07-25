namespace Hestia.Persistence.Models
{
    public class PersistenceSettings
    {
        public StorageDrivers Driver { get; set; } = StorageDrivers.Memory;
        public bool DropDatabaseOnStartup { get; set; } = false;

        public SqliteSettings? Sqlite { get; set; }
    }


    public class SqliteSettings
    {
        public string FilePath { get; set; } = "hestia.db";
    }
}
