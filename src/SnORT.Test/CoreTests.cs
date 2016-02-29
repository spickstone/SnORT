using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using SnORT.Core;
using SnORT.Core.Services;

namespace SnORT.Test
{
	[TestFixture ()]
	public class CoreTests
	{
		protected MediaFactory factory;
		protected IMediaService service;
		protected string root = ConfigurationManager.AppSettings["NASroot"];
		protected string source = ConfigurationManager.AppSettings["Source"];

		private static List<String> mockedFilenames = new List<String>() { "a.txt","b.txt","c.txt"};

		private static string md5_fn1 = "36dc7e16fee91d13c807a356177ee404";

		[TestFixtureSetUp()]
		public void Init()
		{
			factory = new MediaFactory ();
			service = factory.GetMediaService ("EpisodeService");
			MockSourceFolder ();
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

		[TestFixtureTearDown()]
		public void TearDown()
		{
			if (Directory.Exists (source)) {
				var dir = new DirectoryInfo (source);
				dir.Delete (true);
			}
		}

		/// <summary>
		/// Verify the factory and service are created as expected.
		/// </summary>
		[Test ()]
		public void TestFactoryAndService ()
		{
			
			Assert.IsNotNull (service);
			Assert.IsInstanceOf<EpisodeService> (service);
		}

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


			Assert.AreEqual (mockedFilenames.Count, map.Count);
			Assert.AreEqual (mockedFilenames [0], map [0]);
		}

		[Test()]
		public void TestHash ()
		{
			var hash = service.Hash (source + mockedFilenames [0]);
			Assert.AreEqual (hash, md5_fn1);
		}
	}
}

