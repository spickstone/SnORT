using System;
using System.IO;
using SnORT.Core.Services;
using AutoFactory;

namespace SnORT.Core
{
	public class MediaFactory
	{
		//private TypedParameter[] p = new TypedParameter[3] {TypedParameter.From<FileSystemInfo>(), TypedParameter.From<String>(), TypedParameter.From<Int32>()};

		private IAutoFactory<IMediaService> factory = Factory.Create<IMediaService>();

		public MediaFactory ()
		{
			
		}

		public IMediaService GetMediaService(string kind)
		{
			return factory.SeekPart(t => t.Name.Equals(kind));
		}
	}
}

