using System;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;

namespace SnORT.Core
{
	public class Episode
	{
		public FileSystemInfo FileSystemInfo { get; set; }
		public String Series { get; set; }
		public Int32 Season { get; set; }
		public String Hash { get; set; }
		private String FullName { get { return FileSystemInfo.FullName; } }

		public Episode (FileSystemInfo fileSystemInfo, String series, Int32 season)
		{
			FileSystemInfo = fileSystemInfo;
			Series = series;
			Season = season;
			Hash = this.hash (FileSystemInfo.FullName);
		}

		private string hash(string filename)
		{
			int buffer;
			string key = string.Empty;
			//TOOD: Decide how the Core will get configurable settings.
			if (!Int32.TryParse ("1200000", out buffer))
				buffer = 1200000; // Default buffer size if config contains invalid value.

			using (var md5 = MD5.Create())
			{
				using (var stream = new BufferedStream(File.OpenRead(filename), buffer))
				{
					key = BitConverter.ToString (md5.ComputeHash (stream)).Replace("-","").ToLower();;
				}
			}

			return key;
		}

		public void Copy(string destination)
		{
			File.Copy (FullName, destination);
			String destHash = hash (destination);
			if (Hash.Equals (destHash)) {
				Console.WriteLine (String.Format ("Created: {0} Verified MD5: {1}", FileSystemInfo.Name, Hash));
				File.Delete (FullName);
			} else {
				Console.WriteLine (String.Format ("Couldn't verify MD5 deleting {0}", destination));
				File.Delete (destination);
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Episode: Series={0}, Season={1}, Hash={2}]", Series, Season, Hash);
		}
	}
}

