using System;

namespace net.Utility.SnORT
{
	public class Episode
	{
		public String Filename { get; set; }
		public String Series { get; set; }
		public Int32 Season { get; set; }

		public Episode (String filename, String series, Int32 season)
		{
			Filename = filename;
			Series = series;
			Season = season;
		}

		public override string ToString ()
		{
			return string.Format ("[Episode: Series={0}, Season={1}]", Series, Season);
		}
	}
}

