using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Theasurus.Core
{
	public class Theasurus: ITheasurus
	{
		private TheasurusDbContext _dbContext;

		public Theasurus(TheasurusDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task AddSynonymsAsync(string word, IEnumerable<string> synonyms)
		{
			var wordId = (await GetWordOrCreate(word)).Id;
			foreach(var synonymText in synonyms)
			{
				var synonymId = (await GetWordOrCreate(synonymText)).Id;
				await _dbContext.SynonymMapping.AddAsync(new WordSynonym(wordId, synonymId));
			}
			await _dbContext.SaveChangesAsync();
		}

		public async Task<IEnumerable<string>> GetSynonymsAsync(string word)
		{
			var existingWord = GetWordOrNull(word);
			if (existingWord == null)
			{
				throw new ArgumentException($"Word {word} is not present in the dictionary");
			}

			var synonymIds = await _dbContext.SynonymMapping.Where(x => x.WordId == existingWord.Id).Select(x => x.SynonymId).ToListAsync();
			return await _dbContext.Words.Where(x => synonymIds.Contains(x.Id)).Select(x => x.Text).ToListAsync();
		}

		public async Task<SearchResult> GetWordsAsync(uint take, uint skip)
		{
			var total = _dbContext.Words.Count();
			var result = await _dbContext.Words.Skip((int)skip).Take((int)take).ToListAsync();
			var next = skip + take;

			return new SearchResult(result.Select(x => x.Text), total, next < total ? next : null);
		}

		private Task<Word> GetWordOrNull(string text) => 
			_dbContext.Words.FirstOrDefaultAsync(x => string.Equals(text, x.Text, StringComparison.OrdinalIgnoreCase));

		private async Task<Word> GetWordOrCreate(string text)
		{
			var word = await GetWordOrNull(text);
			if(word == null)
			{
				return (await _dbContext.Words.AddAsync(new Word(text))).Entity;
			}

			return word;
		}
	}
}
