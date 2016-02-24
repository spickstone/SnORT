using System;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;

namespace SnORT.Core.Entities
{
	public class Episode : MediaBase 
	{
		public String Series { get; set; }
		public Int32 Season { get; set; }
	
		public Episode (FileSystemInfo fsi, String hash, String series, Int32 season) : base(fsi, hash)
		{
			Series = series;
			Season = season;
		}

		public override string ToString ()
		{
			return string.Format ("[Episode: Series={0}, Season={1}, Hash={2}]", Series, Season, Md5Hash);
		}
	}
}

