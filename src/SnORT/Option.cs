using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace SnORT
{
	class Options 
	{
		[Option('e', "express", Required = false, HelpText = "Enable express copy (bypasses MDS checksum)")]
		public bool Express { get; set; }

		[Option('l', "list", Required = false, HelpText = "List files pending SnORTing")]
		public bool ListOnly { get; set; }

		[OptionList('f', "files", ',')]
		public List<string> Identifiers { get; set; }

		// omitting long name, default --verbose
		//[Option(DefaultValue = true, HelpText = "Prints all messages to standard output.")]
		//public bool Verbose { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage() 
		{
			return HelpText.AutoBuild(this,	(HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}