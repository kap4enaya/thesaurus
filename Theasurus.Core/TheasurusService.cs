using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Theasurus.Core
{
	public class TheasurusService : ITheasurusService
	{
		private TheasurusDbContext _dbContext;

		public TheasurusService(TheasurusDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <inheritdoc/>
		public Task AddAsync(string word)
		{
			ValidateWord(word);
			return AddInternal(word, null);
		}

		/// <inheritdoc/>
		public Task AddAsync(string word, IEnumerable<string> synonyms)
		{
			ValidateWord(word);
			var cleanedSynonyms = synonyms?.Select(x =>
			{
				ValidateWord(x);
				return x.Trim().ToLower();
			}) ?? throw new ArgumentNullException(nameof(synonyms));

			return AddInternal(word, cleanedSynonyms);
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<string>> GetSynonymsAsync(string word)
		{
			ValidateWord(word);

			var existingWord = await GetWordOrNull(word.Trim().ToLower());
			if (existingWord == null)
			{
				throw new ArgumentException($"Word {word} is not present in the dictionary");
			}

			return (await GetSynonymsInternal(existingWord.Id)).Select(x => x.Text);
		}

		/// <inheritdoc/>
		public async Task<SearchResult> GetWordsAsync(uint take, uint skip)
		{
			var total = _dbContext.Words.Count();
			var result = await _dbContext.Words.Skip((int)skip).Take((int)take).ToListAsync();
			var next = skip + take;

			return new SearchResult(result.Select(x => x.Text), total, next < total ? next : null);
		}

		private async Task AddInternal(string word, IEnumerable<string>? synonyms)
		{
			var wordId = (await GetWordOrCreate(word.Trim().ToLower())).Id;
			var existingSynonyms = await GetSynonymsInternal(wordId);

			if (synonyms == null)
				return;

			foreach (var synonym in synonyms)
			{
				if (existingSynonyms.Any(x => x.Text == synonym))
					continue;//TODO: consider logging it

				var synonymId = (await GetWordOrCreate(synonym.Trim().ToLower())).Id;
				await _dbContext.AddAsync(new WordSynonym(wordId, synonymId));
			}
			await _dbContext.SaveChangesAsync();
		}

		private async Task<IEnumerable<Word>> GetSynonymsInternal(int wordId)
		{
			var synonymIds = await _dbContext.SynonymMapping.Where(x => x.WordId == wordId).Select(x => x.SynonymId).ToListAsync();
			return await _dbContext.Words.Where(x => synonymIds.Contains(x.Id)).ToListAsync();
		}

		private Task<Word> GetWordOrNull(string text)
		{
			return _dbContext.Words.FirstOrDefaultAsync(x => text == x.Text);
		}

		private async Task<Word> GetWordOrCreate(string text)
		{
			var word = await GetWordOrNull(text);
			if (word == null)
			{
				var wordAdded = await _dbContext.AddAsync(new Word(text));
				_dbContext.SaveChanges();//we have to save the changes in order to let the DB generate the Id for us
				return wordAdded.Entity;
			}

			return word;
		}

		private void ValidateWord(string word)
		{
			if (string.IsNullOrWhiteSpace(word))
			{
				throw new ArgumentException($"A word can not be null or white space");
			}
		}
	}
}
