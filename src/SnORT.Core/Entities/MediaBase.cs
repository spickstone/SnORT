using System;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;

namespace SnORT.Core.Entities
{
	public interface IMedia
	{
		String FullName { get; }
		String Md5Hash { get; }
	}

	public abstract class MediaBase : IMedia 
	{
		private FileSystemInfo fileSystemInfo;
		private string hash;

		public virtual String Md5Hash { get { return hash; } }
		public virtual String FullName { get { return fileSystemInfo.FullName; } }

		public MediaBase(FileSystemInfo fsi, String hash)
		{
			fileSystemInfo = fsi;
			this.hash = hash;
		}
	}
}

