using CommandLine;
using System.Collections.Generic;

namespace Theasurus.App.Options
{
	[Verb("add")]
	public class AddOptions
	{
		[Option('w',"word", Required = true, HelpText = "A word to be added to the dictionary")]
		public string Word { get; set; }

		[Option('s', "synonyms", Separator = ',', HelpText = "Synonyms of the word separated by coma")]
		public IEnumerable<string> Synonyms { get; set; }
	}
}
