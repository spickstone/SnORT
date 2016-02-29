using System;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using SnORT.Core.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SnORT.Core.Services
{
	public interface IMediaService
	{
		void Move (IMedia media, string destination);

		void SortFiles (String path = null, int key = int.MinValue);

		Dictionary<int, String> ListFiles ();

		String Hash (String filename);
	}


	public abstract class MediaServiceBase : IMediaService
	{

		protected internal string episodeRegex = @"^(.*) - \[([0-9]+)x([0-9]+)\] - (.*)\.(avi|mp4|mkv|m4v)$";
		protected internal string Root { get { return ConfigurationManager.AppSettings["NASroot"]; } }
		protected internal string Source { get { return ConfigurationManager.AppSettings["Source"]; } }

		private void Copy(IMedia media, string destination)
		{
			File.Copy (media.FullName, destination);
			String destHash = Hash (destination);
			if (media.Md5Hash.Equals (destHash)) {
				Console.WriteLine (String.Format ("Created: {0} Verified MD5: {1}", destination, media.Md5Hash));
				Console.WriteLine (media);
				File.Delete (media.FullName);
			} else {
				Console.WriteLine (String.Format ("Couldn't verify MD5 deleting {0}", destination));
				File.Delete (destination);
			}
		}

		public virtual void Move(IMedia media, string destination)
		{
			FileInfo destFile = new FileInfo (destination);

			if(!System.IO.Directory.Exists(destFile.Directory.FullName))
				System.IO.Directory.CreateDirectory (destFile.Directory.FullName);

			if (!System.IO.Directory.Exists (destination))	
				Copy (media, destination);			
			//TODO: else condition and exception logging
		}

		public String Hash(String filename)
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

		public virtual Dictionary<int, String> ListFiles()
		{
			Dictionary<int, String> directoryMap = new Dictionary<int, string> ();
			int key = 0;

			DirectoryInfo folder = new DirectoryInfo(Source);
			foreach (FileSystemInfo fsi in folder.EnumerateFileSystemInfos()) {
				directoryMap.Add (key++, fsi.Name);
			}

			return directoryMap;
		}

		public virtual void SortFiles(String path = null, int key = int.MinValue)
		{
			if (!Directory.Exists (Root))
				throw new InvalidOperationException (String.Format("Could not locate NAS root: [{0}] ensure NAS is accessible and drive is mapped", Root));
			
			//TODO: VAlidate source folder

			if (path == null)
				path = Source;

			DirectoryInfo folder = new DirectoryInfo(path);
			List<FileSystemInfo> listOfFiles = new List<FileSystemInfo> ();
			listOfFiles.AddRange(folder.EnumerateFileSystemInfos ());
			listOfFiles.ForEach(Match);
		}			

		private void Match(FileSystemInfo fsi)
		{
			// If 'folder' then sort that directory too!
			if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory) 
			{
				SortFiles (fsi.FullName);
			}
			else
			{
				if (Regex.IsMatch (fsi.Name, episodeRegex))
				{
					Match m = Regex.Match (fsi.Name, episodeRegex);

					string filename = fsi.Name;
					string series = m.Groups[1].Captures[0].Value;
					Int32 season = Int32.Parse( m.Groups [2].Captures [0].Value);
					var ep = new Episode (fsi, Hash(fsi.FullName), series, season);

					String target = String.Format (@"{0}/{1}/Season {2}/", Root, ep.Series, ep.Season);
					Move (ep, String.Format (@"{0}/{1}", target, ep.FullName));
				}
			}
		}
	}
}

