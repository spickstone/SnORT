using System;
using SnORT.Core;
using SnORT.Core.Services;

namespace SnORT.Core.Entities
{
	/// <summary>
	/// SnORT is command line utility useful for organising re-named torrent media files in to the correct media folder on the NAS.
	/// </summary>
	public class SnORT
	{
		public static void Main (string[] args)
		{			
			try {
				MediaFactory factory = new MediaFactory ();
				IMediaService service = factory.GetMediaService ("EpisodeService");
				service.SortFiles ();
			}
			catch (Exception ex) {
				Console.WriteLine (String.Format("{0}:{1}", "ERROR", ex.Message));
			}
			finally {
				Console.WriteLine ("EOP");
			}

		}
	}
}
