

namespace Group5.Shared
{
	public class CommonMethod : ICommonMethod
	{
		IWebHostEnvironment env;

		public  CommonMethod(IWebHostEnvironment env)
		{
			this.env = env;
		}

		public  async Task<string> UploadImage(IFormFile formFile)
		{

			if (formFile != null)
			{
				string imgFilename = formFile.FileName;
				var imgFolder = Path.Combine(env.ContentRootPath, "wwwroot/images");

				if (!Directory.Exists(imgFolder))
				{
					Directory.CreateDirectory(imgFolder);
				}

				var imgPath = Path.Combine(imgFolder, imgFilename);

				using (var fs = new FileStream(imgPath, FileMode.Create))
				{
					await formFile.CopyToAsync(fs);
				}

				return imgFilename;
			}
			return "false";

		}
	}
}
