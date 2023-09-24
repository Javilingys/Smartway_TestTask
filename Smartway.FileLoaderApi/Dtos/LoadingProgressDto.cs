namespace Smartway.FileLoaderApi.Dtos;

public class LoadingProgressDto
{
    public LoadingProgressDto(int? percent)
    {
        if (percent.HasValue)
        {
            Message = $"Ваши файли загружены на: {percent.Value}%";
        }
        else
        {
            Message = "У вас нет активных загружающихся файлов.";
        }
    }

    public string Message { get; private set; }
}
