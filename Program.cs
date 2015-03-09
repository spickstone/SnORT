using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;

namespace net.Utility.SnORT
{
	/// <summary>
	/// SnORT is command line utility useful for organising re-named torrent media files in to the correct media folder on the NAS.
	/// </summary>
	public class SnORT
	{
		private static string regexSeriesSeasonEpisode = @"^(.*) - \[([0-9]+)x([0-9]+)\] - (.*)\.(avi|mp3|mkv|m4v)$";
		private static string Root { get { return ConfigurationManager.AppSettings["NASroot"]; } }
		private static string Source { get { return ConfigurationManager.AppSettings["Source"]; } }

		public static void Main (string[] args)
		{
			if (!Directory.Exists (Root)) {

				throw new InvalidOperationException (String.Format("Could not locate NAS root: [{0}] ensure NAS is accessible and drive is mapped", Root));
			}

			Sortfiles (Source);
			Console.WriteLine ("EOP");
		}

		private static void Sortfiles(String path)
		{
			DirectoryInfo folder = new DirectoryInfo(path);
			List<FileSystemInfo> listOfFiles = new List<FileSystemInfo> ();
			listOfFiles.AddRange(folder.EnumerateFileSystemInfos ());
			listOfFiles.ForEach(Match);
		}

		private static void Move(Episode episode)
		{
			String target = String.Format (@"{0}/{1}/Season {2}/", Root, episode.Series, episode.Season);
			String destination = String.Format (@"{0}/{1}", target, episode.FileSystemInfo.Name);

			if(!System.IO.Directory.Exists(target))
				System.IO.Directory.CreateDirectory (target);


			episode.Copy (destination);
			Console.WriteLine (episode);
		}


		private static void Match(FileSystemInfo fsi)
		{
			// If 'folder' then sort that directory too!
			if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory) 
			{
				Sortfiles (fsi.FullName);
			}
			else
			{
				if (Regex.IsMatch (fsi.Name, regexSeriesSeasonEpisode))
				{
					Match m = Regex.Match (fsi.Name, regexSeriesSeasonEpisode);

					string filename = fsi.Name;
					string series = m.Groups[1].Captures[0].Value;
					Int32 season = Int32.Parse( m.Groups [2].Captures [0].Value);
					Episode ep = new Episode (fsi, series, season);
					Move (ep);
				}
			}
		}
	}
}
