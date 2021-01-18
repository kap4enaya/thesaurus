using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theasurus.App.Options
{
	[Verb("synonyms")]
	public class SynonymsOptions
	{
		[Option('w', "word", Required = true, HelpText = "A word for which you'd want to see the synonyms")]
		public string Word { get; set; }
	}
}
