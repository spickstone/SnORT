using System;
using System.IO;

namespace net.Utility.SnORT
{
	public class Episode
	{
		public FileSystemInfo FileSystemInfo { get; set; }
		public String Series { get; set; }
		public Int32 Season { get; set; }

		public Episode (FileSystemInfo fileSystemInfo, String series, Int32 season)
		{
			FileSystemInfo = fileSystemInfo;
			Series = series;
			Season = season;
		}

		public override string ToString ()
		{
			return string.Format ("[Episode: Series={0}, Season={1}]", Series, Season);
		}
	}
}

