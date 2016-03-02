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

		void SortFiles (String path = null, int key = int.MinValue);

		Dictionary<int, String> ListFiles ();

		String Hash (String filename);
	}


	public abstract class MediaServiceBase : IMediaService
	{

		protected string episodeRegex = @"^(.*) - \[([0-9]+)x([0-9]+)\] - (.*)\.(avi|mp4|mkv|m4v)$";
		protected string Root { get { return ConfigurationManager.AppSettings["NASroot"]; } }
		protected string Source { get { return ConfigurationManager.AppSettings["Source"]; } }

		public virtual void Move(IMedia media)
		{
			FileInfo destFile = new FileInfo (media.Target);

			if(!Directory.Exists(destFile.Directory.FullName))
				Directory.CreateDirectory (destFile.Directory.FullName);


			File.Copy (media.FullName, media.Target, true);
			String destHash = Hash (media.Target); // gen destination hash

			if (media.Md5Hash.Equals (destHash)) {
				Console.WriteLine (String.Format ("Created: {0} Verified MD5: {1}", media.Target, media.Md5Hash));
				Console.WriteLine (media);
				File.Delete (media.FullName);
			} else {
				Console.WriteLine (String.Format ("Couldn't verify MD5 deleting {0}", media.Target));
				File.Delete (media.Target);
			}
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
				if(Regex.IsMatch(fsi.Name, episodeRegex))
					directoryMap.Add (key++, fsi.Name);
			}

			return directoryMap;
		}

		public virtual void SortFiles(String path = null, int key = int.MinValue)
		{
			if (!Directory.Exists (Root))
				throw new InvalidOperationException (String.Format ("Could not locate NAS root: [{0}] ensure NAS is accessible and drive is mapped", Root));
			
			//TODO: VAlidate source folder

			if (path == null)
				path = Source;


			DirectoryInfo folder = new DirectoryInfo (path);
			List<FileSystemInfo> listOfFiles = new List<FileSystemInfo> ();
			if (key == int.MinValue)
				listOfFiles.AddRange (folder.EnumerateFileSystemInfos ());
			else {
				List<FileSystemInfo> filter = new List<FileSystemInfo> ();
				filter.AddRange (folder.EnumerateFileSystemInfos ());
				listOfFiles.Add(filter[key]);
			}
			listOfFiles.ForEach (Match);
		}		

		private void Match(FileSystemInfo fsi)
		{
			// If 'folder' then sort that directory too!
			if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory) 
				SortFiles (fsi.FullName);
			else
				if (Regex.IsMatch (fsi.Name, episodeRegex))
					Move (new Episode (fsi, Hash(fsi.FullName), Regex.Match (fsi.Name, episodeRegex)));
		}
	}
}

