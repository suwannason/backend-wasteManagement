


public class CompaniesDatabaseSettings : ICompanieDatabaseSettings
{
    public string CompanyCollectionName { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}

public interface ICompanieDatabaseSettings
{
    string CompanyCollectionName { get; set; }
    string ConnectionString { get; set; }
    string DatabaseName { get; set; }
}    
