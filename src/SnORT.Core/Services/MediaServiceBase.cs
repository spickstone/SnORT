using System;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using SnORT.Core.Entities;

namespace SnORT.Core.Services
{
	public interface IMediaService
	{
		void Copy (IMedia media, string destination);

		void Move (IMedia media, string destination);

		String Hash (String filename);
	}

	public abstract class MediaServiceBase : IMediaService
	{		
		public virtual void Copy(IMedia media, string destination)
		{
			File.Copy (media.FullName, destination);
			String destHash = Hash (destination);
			if (media.Md5Hash.Equals (destHash)) {
				Console.WriteLine (String.Format ("Created: {0} Verified MD5: {1}", destination, media.Md5Hash));
				File.Delete (media.FullName);
			} else {
				Console.WriteLine (String.Format ("Couldn't verify MD5 deleting {0}", destination));
				File.Delete (destination);
			}
		}

		public virtual void Move(IMedia media, string destination)
		{
			throw new NotImplementedException ("TODO");
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
	}
}

