namespace BlackBrownie;

public interface IFunction
{
    public string DescriptionFunction();

    public string DescriptionArgs();

    public Task Do(string[] args, CancellationToken token);
}
