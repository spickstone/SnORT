using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using SnORT;
using SnORT.Core;
using SnORT.Core.Services;
using System.Text.RegularExpressions;

namespace SnORT.Test
{
	[TestFixture ()]
	public class CoreTests
	{
		protected MediaFactory factory;
		protected IMediaService service;
		protected string root = ConfigurationManager.AppSettings["NASroot"];
		protected string source = ConfigurationManager.AppSettings["Source"];
		protected string episodeRegex = ConfigurationManager.AppSettings["regexSeriesSeasonEpisode"];

		private static List<String> mockedFilenames = new List<String>() { 
			"Blindspot - [01x11] - Cease Forcing Enemy.mkv",
			"Castle (2009) - [08x13] - And Justice For All.mkv",
			"Castle.S08E08.Mr.and.Mrs.Castle.720p.WEB-DL.DD5.1.H.264.mkv",
			"Jane the Virgin - [02x13] - Chapter Thirty-Five.mkv"
		};

		private static string md5_fn1 = "36dc7e16fee91d13c807a356177ee404";

		[TestFixtureSetUp()]
		public void Init()
		{
			factory = new MediaFactory ();
			service = factory.GetMediaService ("EpisodeService");
			MockSourceFolder ();
			MockRootFolder ();
		}

		[TearDown()]
		public void ResetPreTest()
		{
			TearDown ();
			MockSourceFolder ();
			MockRootFolder ();
		}

		public void MockSourceFolder()
		{
			Directory.CreateDirectory (source);

			foreach (String filename in mockedFilenames) {
				using (StreamWriter sw = File.CreateText (source + filename)) {
					
					sw.WriteLine ("The quick brown fox jumped over the lazy dog");
				}
			}
		}

		public void MockRootFolder()
		{
			Directory.CreateDirectory (root);
		}

		[TestFixtureTearDown()]
		public void TearDown()
		{
			PurgeFolder (source);
			PurgeFolder (root);
		}

		private void PurgeFolder(string folder)
		{
			if (Directory.Exists (folder)) {
				var dir = new DirectoryInfo (folder);
				dir.Delete (true);
			}
		}

		/// <summary>
		/// Verify the factory and service is created as expected.
		/// </summary>
		[Test ()]
		public void TestFactoryAndService ()
		{
			
			Assert.IsNotNull (service);
			Assert.IsInstanceOf<EpisodeService> (service);
		}

		/// <summary>
		/// Verify test Root and Source from Test configuration settings is as expected
		/// </summary>
		[Test()]
		public void TestConfiguration ()
		{
			Assert.AreEqual (root, "./Root/");
			Assert.AreEqual (source, "./Source/");
		}

		[Test()]
		public void TestListFiles()
		{
			var map = service.ListFiles ();

			Assert.AreEqual (mockedFilenames.Count - 1, map.Count);
			Assert.AreEqual (mockedFilenames [0], map [0]);
		}

		[Test()]
		public void TestHash ()
		{
			var hash = service.Hash (source + mockedFilenames [0]);
			Assert.AreEqual (hash, md5_fn1);
		}

		[Test()]
		public void TestRegex()
		{
			Assert.IsTrue(Regex.IsMatch(mockedFilenames[0], episodeRegex));
			Assert.IsFalse (Regex.IsMatch (mockedFilenames [2], episodeRegex));
		}

		[Test()]
		public void VerifyDuplicateSort ()
		{
			CreateDuplicate ();
			service.SortFiles();
			VerifySort ();
		}

		private void CreateDuplicate()
		{
			service.SortFiles (keys: new List<int>() {0});
			PurgeFolder (source);
			MockSourceFolder ();
		}

		[Test()]
		public void VerifyVanillaSort()
		{
			service.SortFiles ();
			VerifySort ();
		}

		[Test()]
		public void cmd_VerifyVanillaSort()
		{
			string[] args = new string[0];
			SnORT.Main (args);
			VerifySort ();
		}

		[Test()]
		public void cmd_VerifyVanillaSort_express()
		{
			string[] args = new string[1] {"-e"};
			SnORT.Main (args);
			VerifySort ();
		}

		[Test()]
		public void cmd_VerifyMultiItemCopyWithExpress()
		{
			string[] args = new string[2] {"-e", "-f 0,2"};
			SnORT.Main (args);

			// Assert first item was copied (sorted) only
			var src = new DirectoryInfo (source);
			List<FileInfo> remaining = new List<FileInfo> ();
			remaining.AddRange (src.EnumerateFiles ());

			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[0])));
		}

		[Test()]
		public void VerifySingleItemCopyWithExpress()
		{
			var map = service.ListFiles ();
			// Express copy Blindspot
			service.SortFiles (keys: new List<int>() {0}, express: true);

			// Assert first item was copied (sorted) only
			var src = new DirectoryInfo (source);
			List<FileInfo> remaining = new List<FileInfo> ();
			remaining.AddRange (src.EnumerateFiles ());

			Assert.IsTrue (map.Count == 3);
			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[0])));
		}

		[Test()]
		public void VerifySingleItemCopy()
		{
			var map = service.ListFiles ();
			// Copy Blindspot
			service.SortFiles (keys: new List<int>() {0}, express: false);

			// Assert first item was copied (sorted) only
			var src = new DirectoryInfo (source);
			List<FileInfo> remaining = new List<FileInfo> ();
			remaining.AddRange (src.EnumerateFiles ());

			Assert.IsTrue (map.Count == 3);
			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[0])));
		}

		[Test()]
		public void VerifyMultiItemCopyWithExpress()
		{
			var map = service.ListFiles ();
			// Express copy Blindspot, and Jane the Virgin
			service.SortFiles (keys: new List<int>() {0, 2}, express: true);

			// Assert first item was copied (sorted) only
			var src = new DirectoryInfo (source);
			List<FileInfo> remaining = new List<FileInfo> ();
			remaining.AddRange (src.EnumerateFiles ());

			Assert.IsTrue (map.Count == 3);
			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[0])));
			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[3])));
		}

		[Test()]
		public void VerifyMultiItemCopy()
		{
			var map = service.ListFiles ();
			// Express copy Blindspot, and Jane the Virgin
			service.SortFiles (keys: new List<int>() {0, 2}, express: false);

			// Assert first item was copied (sorted) only
			var src = new DirectoryInfo (source);
			List<FileInfo> remaining = new List<FileInfo> ();
			remaining.AddRange (src.EnumerateFiles ());

			Assert.IsTrue (map.Count == 3);
			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[0])));
			Assert.IsTrue (remaining.TrueForAll(x => !x.Name.Equals(mockedFilenames[3])));
		}

		private void VerifySort()
		{
			var src = new DirectoryInfo (source);
			List<FileInfo> remaining = new List<FileInfo> ();
			remaining.AddRange (src.EnumerateFiles ());

			Assert.AreEqual (mockedFilenames [2], remaining[0].Name);
			int interation = 0;
			var target = new DirectoryInfo (root);
			foreach(DirectoryInfo di in target.EnumerateDirectories())
			{
				switch (interation++) {
				case 0:
					Assert.AreEqual (di.Name, "Blindspot");
					AssertSeasonNumber(di, "Season 1");
					break;
				case 1:
					Assert.AreEqual (di.Name, "Castle (2009)");
					AssertSeasonNumber(di, "Season 8");
					break;
				case 2:
					Assert.AreEqual (di.Name, "Jane the Virgin");
					AssertSeasonNumber(di, "Season 2");
					break;
				default:
					break;	
				}
			}
		}

		private void AssertSeasonNumber(DirectoryInfo parent, string season)
		{
			var c = parent.EnumerateDirectories();
			List<DirectoryInfo> children = new List<DirectoryInfo> ();
			children.AddRange (c);
			Assert.IsTrue(children.TrueForAll (x => x.Name.Equals (season)));
		}
	}
}

