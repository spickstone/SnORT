using System;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using System.Text.RegularExpressions;

namespace SnORT.Core.Entities
{
	public class Episode : MediaBase 
	{
		public String Series { get; private set; }
		public Int32 Season { get; private set; }
		public override String Target { get { return String.Format (@"{0}/{1}/Season {2}/{3}", Root, Series, Season, fileSystemInfo.Name); } }
	
		public Episode (FileSystemInfo fsi, String hash, Match m) : base(fsi, hash)
		{
			Series = m.Groups[1].Captures[0].Value;
			Season = Int32.Parse( m.Groups [2].Captures [0].Value);
		}

		public override string ToString ()
		{
			return string.Format ("[Episode: Series={0}, Season={1}, Hash={2}]", Series, Season, Md5Hash);
		}
	}
}

