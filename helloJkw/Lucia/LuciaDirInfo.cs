using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class LuciaDirInfo
	{
		string _path;
		List<LuciaDirInfo> _dirList = new List<LuciaDirInfo>();
		List<FileInfo> _fileList = new List<FileInfo>();
		DefaultDictionary<string, LuciaDirInfo> _dirMap = new DefaultDictionary<string, LuciaDirInfo>();
		public ProductInfo ProductInfo;

		public LuciaDirInfo(string path)
		{
			_path = path.Replace('\\', '/');
			foreach (var dir in Directory.GetDirectories(_path).OrderBy(e => e))
				_dirList.Add(new LuciaDirInfo(dir));
			foreach (var file in Directory.GetFiles(_path).OrderBy(e => e))
				_fileList.Add(new FileInfo(file.Replace('\\', '/')));
			foreach (var dir in _dirList)
				_dirMap.Add(Path.GetFileName(dir._path), dir);

			ProductInfo = new ProductInfo(this);
		}

		public LuciaDirInfo this[string dirName]
		{
			get
			{
				return _dirList.Where(e => Path.GetFileName(e._path).Contains(dirName)).FirstOrDefault();
			}
		}

		public string FolderName
		{
			get { return Path.GetFileName(_path); }
		}

		public IEnumerable<LuciaDirInfo> GetSubDirList()
		{
			return _dirList;
		}

		public IEnumerable<ProductInfo> GetProductList()
		{
			return _dirList
				.Where(e => e.ProductInfo.ImageList.Count() > 0)
				.Select(e => e.ProductInfo);
		}

		public IEnumerable<string> GetDirNames()
		{
			return _dirList.Select(e => Path.GetFileName(e._path));
		}

		public IEnumerable<FileInfo> GetFiles()
		{
			return _fileList;
		}
	}

	public static class DirectoryHelper
	{
		public static LuciaDirInfo CreateDirInfo(this string path)
		{
			return new LuciaDirInfo(path);
		}
	}
}
