namespace Group5.Shared
{
	public interface ICommonMethod
	{
		Task<string> UploadImage(IFormFile formFile);
	}
}
