using CommandLine;

namespace Theasurus.App.Options
{
	[Verb("synonyms", HelpText = "Display the list of synonyms for a word.")]
	public class SynonymsOptions
	{
		[Option('w', "word", Required = true, HelpText = "A word for which you'd want to see the synonyms")]
		public string Word { get; set; }
	}
}
