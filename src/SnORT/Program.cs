using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using SnORT.Core;
using SnORT.Core.Services;

namespace SnORT
{
	/// <summary>
	/// SnORT is command line utility useful for organising re-named torrent media files in to the correct media folder on the NAS.
	/// </summary>
	public class SnORT
	{
		public static void Main (string[] args)
		{
			var options = new Options();
			CommandLine.Parser.Default.ParseArguments(args, options);

			try 
			{
				MediaFactory factory = new MediaFactory ();
				IMediaService service = factory.GetMediaService ("EpisodeService");

				if(options.ListOnly)
					PrintFilesToConsole(service.ListFiles());
				else
				{
					if(options.Identifiers !=null && options.Identifiers.Count > 0)
					{
						List<int> ids = options.Identifiers.ConvertAll<int>(new Converter<string, int>(CovertString));
						service.SortFiles(keys: ids, express: options.Express);
					}
					else
						service.SortFiles (express: options.Express);
				}
			}
			catch (Exception ex) {
				Console.WriteLine (String.Format("{0}:{1}", "ERROR", ex.Message));
			}
			finally {
				Console.WriteLine ("EOP");
			}

		}


		// List<Point> lp = lpf.ConvertAll( 
		//new Converter<PointF, Point>(PointFToPoint));

		private static int CovertString(string id)
		{
			return Int32.Parse (id);
		}

		public static void PrintFilesToConsole(Dictionary<int, string> files)
		{
			if(files.Count > 0)
			{
				Console.WriteLine ("The following media is available for transfer:");
				foreach (var key in files.Keys) 
				{
					Console.WriteLine (String.Format("k:{0} - {1}", key, files[key]));
				}
			}
		}
	}
}
