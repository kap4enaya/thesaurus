using System.Collections.Generic;
using System.Threading.Tasks;

namespace Theasurus.Core
{
	public interface ITheasurusService
	{
		/// <summary>
		/// Adds a word to the dictionary if it wasn't already present.
		/// </summary>
		Task AddAsync(string word);
		/// <summary>
		/// Adds synonyms to the specified word. 
		/// If the word doesn't yet exist in the dictionary, it will be added. 
		/// If the word already exists in the dictionary, new synonyms will be appended to the list of existing synonyms.
		/// </summary>
		Task AddAsync(string word, IEnumerable<string> synonyms);
		/// <summary>
		/// Returns all the synonyms of the existing word. Case insensitive.
		/// If the word doesn't exist, <see cref="System.ArgumentException" will be thrown/>
		/// </summary>
		Task<IEnumerable<string>> GetSynonymsAsync(string word);
		/// <summary>
		/// Returns one page of words from the dictionary.
		/// </summary>
		/// <param name="take">Indicates how many elements should one page contain.</param>
		/// <param name="skip">Indicates how many contiguous elements from the beginning of the sequence should be skipped.</param>
		Task<SearchResult> GetWordsAsync(uint take, uint skip);
	}

	public record SearchResult(IEnumerable<string> Words, int TotalResults, uint? NextSkip);
}
