public class InternalHandler
{
    public Task<InternalModel> Invoke()
    {
        var response = new InternalModel("value 1", "value 2", 23);
        return Task.FromResult(response);
    }
}

public record InternalModel(string param1, string param2, int number1);
