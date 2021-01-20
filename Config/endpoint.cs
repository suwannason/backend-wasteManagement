
public class Endpoint : IEndpoint {
    public string global_api { get; set; }
}

public interface IEndpoint {
    string global_api { get; set; }
}