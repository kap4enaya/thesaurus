using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Theasurus.App.Options;
using Theasurus.Core;

namespace Theasurus.App
{
	class Program
	{
		static void Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			using var context = new TheasurusDbContext(new DbContextOptionsBuilder<TheasurusDbContext>()
				.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
				.Options);

			var service = new TheasurusService(context);

			context.Database.EnsureCreated();

			Parser.Default.ParseArguments<AddOptions, SynonymsOptions, WordsOptions>(args)
				.WithParsed<AddOptions>(async options => await service.AddAsync(options.Word, options.Synonyms))
				.WithParsed<SynonymsOptions>(async options => await ShowSynonyms(options, service))
				.WithParsed<WordsOptions>(async options => await ShowWords(options, service));
		}

		private static async Task ShowSynonyms(SynonymsOptions options, ITheasurusService service)
		{
			Console.WriteLine($"Synonyms of the word \"{options.Word}\"");
			foreach (var synonym in await service.GetSynonymsAsync(options.Word))
			{
				Console.WriteLine(synonym);
			}
		}

		private static async Task ShowWords(WordsOptions options, ITheasurusService service)
		{
			var take = options.PageSize;
			uint? skip = 0;

			do
			{
				var result = await service.GetWordsAsync(take, skip.Value);
				skip = result.NextSkip;

				var totalPages = Math.Ceiling((double)result.TotalResults / options.PageSize);
				var currentPage = skip.HasValue ? (int)(skip / options.PageSize) : totalPages;

				if (result.Words.Any())
				{
					Console.WriteLine($"********** Page {currentPage} out of {totalPages} **********");
					foreach (var word in result.Words)
					{
						Console.WriteLine(word);
					}
				}
				else
				{
					Console.WriteLine("The dictionary is empty. Check \"add --help\" to learn how to add new words and synonyms.");
				}
			}
			while (skip.HasValue);
		}
	}
}
