using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theasurus.App.Options
{
	[Verb("words")]
	public class WordsOptions
	{
		[Option('p',"page-size", Default = (uint)10, HelpText = "How many words per page should be displayed")]
		public uint PageSize { get; set; }

		[Option("max", Default = (uint)100, HelpText = "Maximum amount of words returned")]
		public uint Maximum { get; set; }
	}
}
