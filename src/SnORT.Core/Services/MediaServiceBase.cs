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
		void Move (IMedia media);

		void SortFiles (String path = null, List<int> keys = null, bool express = false);

		Dictionary<int, String> ListFiles ();

		String Hash (String filename);
	}


	public abstract class MediaServiceBase : IMediaService
	{

		protected string episodeRegex = @"^(.*) - \[([0-9]+)x([0-9]+)\] - (.*)\.(avi|mp4|mkv|m4v)$";

		protected string Root { get { return ConfigurationManager.AppSettings ["NASroot"]; } }

		protected string Source { get { return ConfigurationManager.AppSettings ["Source"]; } }

		public virtual void Move (IMedia media)
		{
			FileInfo destFile = new FileInfo (media.Target);

			if (!Directory.Exists (destFile.Directory.FullName))
				Directory.CreateDirectory (destFile.Directory.FullName);
			File.Copy (media.FullName, media.Target, true);
			// Only generate hash if not doing an express copy

			String destHash = null;
			if (media.Md5Hash != null)
				destHash = Hash (media.Target); // gen destination hash

			if (media.Md5Hash == null || media.Md5Hash.Equals (destHash))
			{
				Console.WriteLine (String.Format ("Created: {0} Verified MD5: {1}", media.Target, media.Md5Hash));
				Console.WriteLine (media);
				File.Delete (media.FullName);
			}
			else
			{
				Console.WriteLine (String.Format ("Couldn't verify MD5 deleting {0}", media.Target));
				File.Delete (media.Target);
			}
			
		}

		public String Hash (String filename)
		{
			int buffer;
			string key = string.Empty;
			//TOOD: Decide how the Core will get configurable settings.
			if (!Int32.TryParse ("1200000", out buffer))
				buffer = 1200000; // Default buffer size if config contains invalid value.

			using (var md5 = MD5.Create ())
			{
				using (var stream = new BufferedStream (File.OpenRead (filename), buffer))
				{
					key = BitConverter.ToString (md5.ComputeHash (stream)).Replace ("-", "").ToLower ();
					;
				}
			}

			return key;
		}

		public virtual Dictionary<int, String> ListFiles ()
		{
			Dictionary<int, String> directoryMap = new Dictionary<int, string> ();
			int key = 0;

			DirectoryInfo folder = new DirectoryInfo (Source);
			foreach (FileSystemInfo fsi in folder.EnumerateFileSystemInfos())
			{
				if (Regex.IsMatch (fsi.Name, episodeRegex))
					directoryMap.Add (key++, fsi.Name);
			}

			return directoryMap;
		}

		private List<FileSystemInfo> CompatiableFiles(DirectoryInfo path)
		{			
			List<FileSystemInfo> compatiable = new List<FileSystemInfo> ();
			foreach (FileSystemInfo fsi in path.EnumerateFileSystemInfos())
				if (Regex.IsMatch (fsi.Name, episodeRegex))
					compatiable.Add (fsi);

			return compatiable;
		}

		public virtual void SortFiles (String path = null, List<int> keys = null, bool express = false)
		{
			if (!Directory.Exists (Root))
				throw new InvalidOperationException (String.Format ("Could not locate NAS root: [{0}] ensure NAS is accessible and drive is mapped", Root));
			
			//TODO: Validate source folder

			if (path == null)
				path = Source;

			DirectoryInfo folder = new DirectoryInfo (path);
			List<FileSystemInfo> listOfFiles = new List<FileSystemInfo> ();
			if (keys == null)
			{
				listOfFiles.AddRange (folder.EnumerateFileSystemInfos());
				listOfFiles.ForEach (Match);
			}
			else
			{
				List<FileSystemInfo> filter = new List<FileSystemInfo> ();
				filter.AddRange (CompatiableFiles(folder));

				foreach (int key in keys)
					listOfFiles.Add (filter [key]);
 				
				if (express)
					listOfFiles.ForEach (Express);
				else
					listOfFiles.ForEach (Match);
			}

		}

		private void Match (FileSystemInfo fsi)
		{
			// If 'folder' then sort that directory too!
			if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
				SortFiles (fsi.FullName);
			else
			if (Regex.IsMatch (fsi.Name, episodeRegex))
				Move (new Episode (fsi, Hash (fsi.FullName), Regex.Match (fsi.Name, episodeRegex)));
		}

		private void Express (FileSystemInfo fsi)
		{
			// If 'folder' then sort that directory too!
			if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
				SortFiles (fsi.FullName, express: true);
			else
			if (Regex.IsMatch (fsi.Name, episodeRegex))
				Move (new Episode (fsi, null, Regex.Match (fsi.Name, episodeRegex)));
		}
	}
}

