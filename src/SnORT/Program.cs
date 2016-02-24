using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;
using SnORT.Core;
using SnORT.Core.Entities;
using SnORT.Core.Services;

namespace SnORT.Core.Entities
{
	/// <summary>
	/// SnORT is command line utility useful for organising re-named torrent media files in to the correct media folder on the NAS.
	/// </summary>
	public class SnORT
	{
		private static string episodeRegex = @"^(.*) - \[([0-9]+)x([0-9]+)\] - (.*)\.(avi|mp4|mkv|m4v)$";
		private static string Root { get { return ConfigurationManager.AppSettings["NASroot"]; } }
		private static string Source { get { return ConfigurationManager.AppSettings["Source"]; } }

		private static MediaFactory factory;
		private static IMediaService service;

		public static void Main (string[] args)
		{
			
			if (!Directory.Exists (Root)) {

				throw new InvalidOperationException (String.Format("Could not locate NAS root: [{0}] ensure NAS is accessible and drive is mapped", Root));
			}
			factory = new MediaFactory ();
			service = factory.GetMediaService ("EpisodeService");
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
			String destination = String.Format (@"{0}/{1}", target, episode.FullName);

			if(!System.IO.Directory.Exists(target))
				System.IO.Directory.CreateDirectory (target);

			if (!System.IO.Directory.Exists (destination)) {	
				service.Copy (episode, destination);
				Console.WriteLine (episode);
			}
			//TODO: else condition and exception logging
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
				if (Regex.IsMatch (fsi.Name, episodeRegex))
				{
					Match m = Regex.Match (fsi.Name, episodeRegex);

					string filename = fsi.Name;
					string series = m.Groups[1].Captures[0].Value;
					Int32 season = Int32.Parse( m.Groups [2].Captures [0].Value);
					var ep = new Episode (fsi, service.Hash(fsi.FullName), series, season);
					Move (ep);
				}
			}
		}
	}
}
